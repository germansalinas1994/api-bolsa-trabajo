using System;
using System.Security.Cryptography.X509Certificates;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using AutoWrapper.Wrappers;
using System.Net;

namespace BussinessLogic.Services
{
    public class ServicePublicacion : GenericService
    {
        public ServicePublicacion(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public async Task<IList<OfertaDTO>> GetAllPublicaciones()
        {
            try
            {
                List<Oferta> oferta = (await _unitOfWork.GenericRepository<Oferta>()
                    .GetAllIncludingSpecificRelations(
                        q => q.Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                        .Include(o => o.OfertaCarreras)
                        .ThenInclude(oc => oc.Carrera)
                )

                    ).ToList();



                return oferta.Adapt<List<OfertaDTO>>();
            }
            catch (ApiException)
            {
                //lanzo la excepcion que se captura en el controller
                throw;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones en caso de error
                throw ex;
            }
        }

        public async Task<IList<OfertaDTO>> GetOfertasByEmpresa(string email)
        {
            try
            {
                Usuario usuario = (await _unitOfWork.GenericRepository<Usuario>().GetByCriteria(u => u.Email == email)).FirstOrDefault();
                PerfilEmpresa perfil = (await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(p => p.IdUsuario == usuario.Id)).FirstOrDefault();
                List<Oferta> ofertas = (await _unitOfWork.GenericRepository<Oferta>()
                    .GetAllIncludingSpecificRelations(
                        q => q.Where(o => o.IdPerfilEmpresa == perfil.Id && o.FechaBaja == null)
                        .Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                        .Include(o => o.OfertaCarreras)
                        .ThenInclude(oc => oc.Carrera)
                    )
                ).OrderByDescending(p => p.FechaModificacion).ToList();

                var ofertasDTO = ofertas.Adapt<List<OfertaDTO>>();

                // Agregar cupos desde OfertaHistorial y contar postulaciones aprobadas
                foreach (var ofertaDTO in ofertasDTO)
                {
                    // Obtener cupos del último historial activo
                    var historial = (await _unitOfWork.GenericRepository<OfertaHistorial>()
                        .GetByCriteria(h => h.IdOferta == ofertaDTO.Id && h.FechaBaja == null))
                        .OrderByDescending(h => h.FechaModificacion)
                        .FirstOrDefault();

                    ofertaDTO.Cupos = historial?.Cupos ?? 1;

                    // Contar postulaciones con ÚLTIMO estado Aprobado
                    var todosHistoriales = (await _unitOfWork.GenericRepository<PostulacionHistorial>()
                        .GetAllIncludingSpecificRelations(
                            q => q.Include(ph => ph.Postulacion)
                        ))
                        .Where(ph => ph.Postulacion != null 
                                  && ph.Postulacion.IdOferta == ofertaDTO.Id 
                                  && ph.FechaBaja == null);

                    // Agrupar por postulación y tomar solo el último estado de cada una
                    var ultimosEstados = todosHistoriales
                        .GroupBy(ph => ph.IdPostulacion)
                        .Select(g => g.OrderByDescending(ph => ph.FechaModificacion).First())
                        .Where(ph => ph.IdEstadoPostulacion == EstadoPostulacion.IdEstadoAprobada);

                    ofertaDTO.CantidadPostulantes = ultimosEstados.Count();
                }

                return ofertasDTO;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OfertaDTO> CrearOferta(CrearOfertaDTO data, string email)
        {
            try
            {
                // Validar que la fecha de inicio esté presente al crear
                if (!data.FechaInicio.HasValue)
                {
                    throw new ApiException("La fecha de inicio es obligatoria al crear una oferta", (int)HttpStatusCode.BadRequest);
                }

                Usuario usuario = (await _unitOfWork.GenericRepository<Usuario>().GetByCriteria(e => e.Email == email)).FirstOrDefault();
                if (usuario == null)
                    throw new ApiException("no existe el usuario", (int)HttpStatusCode.NotFound);

                PerfilEmpresa perfil = (await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(u => u.IdUsuario == usuario.Id)).FirstOrDefault();
                if (perfil.IdEstadoValidacion != EstadoValidacion.IdEstadoAprobada)
                {
                    throw new ApiException("El perfil de la empresa no está aprobado para crear ofertas.", (int)HttpStatusCode.Forbidden);
                }


                if (perfil == null)
                {
                    // Auto-crear perfil de empresa si no existe
                    perfil = await CrearPerfilEmpresaAutomatico(usuario);
                }
                var oferta = data.Adapt<Oferta>();
                oferta.FechaAlta = DateTime.Now;
                oferta.FechaModificacion = DateTime.Now;
                oferta.IdPerfilEmpresa = perfil.Id;
                await _unitOfWork.GenericRepository<Oferta>().Insert(oferta);

                // Crear registro en OfertaHistorial con el estado y cupos
                var estadoPublicada = await _unitOfWork.GenericRepository<EstadoOferta>()
                    .GetByCriteria(e => e.Codigo == "PUBLICADA")
                    .ContinueWith(t => t.Result.FirstOrDefault());

                if (estadoPublicada != null)
                {
                    var ofertaHistorial = new OfertaHistorial
                    {
                        IdOferta = oferta.Id,
                        IdEstadoOferta = estadoPublicada.Id,
                        Cupos = data.Cupos ?? 1, // Por defecto 1 cupo
                        Motivo = "Publicación inicial",
                        FechaAlta = DateTime.Now,
                        FechaModificacion = DateTime.Now
                    };
                    await _unitOfWork.GenericRepository<OfertaHistorial>().Insert(ofertaHistorial);
                }

                // Crear registros de OfertaCarrera si se proporcionaron carreras
                // Se crea un registro OfertaCarrera por cada carrera seleccionada
                // Esto permite que una oferta tenga múltiples carreras asociadas
                if (data.IdCarreras != null && data.IdCarreras.Count > 0)
                {
                    var carrerasUnicas = data.IdCarreras.Distinct().ToList();
                    var fechaActual = DateTime.Now;
                    
                    foreach (var idCarrera in carrerasUnicas)
                    {
                        // Verificar que la carrera exista
                        var carreraExiste = await _unitOfWork.GenericRepository<Carrera>().GetById(idCarrera);
                        if (carreraExiste == null)
                        {
                            throw new ApiException($"La carrera con ID {idCarrera} no existe", (int)HttpStatusCode.BadRequest);
                        }
                        
                        // Crear un nuevo registro OfertaCarrera para cada carrera
                        var ofertaCarrera = new OfertaCarrera
                        {
                            IdOferta = oferta.Id,
                            IdCarrera = idCarrera,
                            FechaAlta = fechaActual,
                            FechaModificacion = fechaActual
                        };
                        // Insertar el registro (cada carrera tiene su propio registro OfertaCarrera)
                        await _unitOfWork.GenericRepository<OfertaCarrera>().Insert(ofertaCarrera);
                    }
                }

                await _unitOfWork.CommitAsync();

                // Obtener la oferta creada con todas las relaciones desde la base de datos
                // Nota: Cargamos todas las OfertaCarreras y el mapeo de Mapster las filtrará por FechaBaja == null
                var ofertaCreada = await _unitOfWork.GenericRepository<Oferta>()
                    .GetByIdIncludingSpecificRelations(oferta.Id,
                        q => q.Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                        .Include(o => o.OfertaCarreras)
                        .ThenInclude(oc => oc.Carrera),
                        asNoTracking: true,
                        asSplitQuery: true
                    );

                var ofertaDto = ofertaCreada.Adapt<OfertaDTO>();

                // Agregar cupos del historial (recién creado)
                ofertaDto.Cupos = data.Cupos ?? 1;
                ofertaDto.CantidadPostulantes = 0; // Nueva oferta, sin postulantes

                return ofertaDto;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OfertaDTO> ActualizarOferta(int id, CrearOfertaDTO data)
        {
            try
            {
                var ofertaExistente = await _unitOfWork.GenericRepository<Oferta>().GetById(id);
                if (ofertaExistente == null)
                {
                    throw new ApiException("Oferta no encontrada", (int)HttpStatusCode.NotFound);
                }

                // Actualizar propiedades (solo si se proporcionan)
                // Título y descripción siempre se actualizan porque son campos obligatorios
                if (!string.IsNullOrEmpty(data.Titulo))
                    ofertaExistente.Titulo = data.Titulo;
                
                if (!string.IsNullOrEmpty(data.Descripcion))
                    ofertaExistente.Descripcion = data.Descripcion;
                
                // Los demás campos solo se actualizan si se proporcionan
                if (data.IdModalidad.HasValue)
                    ofertaExistente.IdModalidad = data.IdModalidad.Value;
                
                if (data.IdTipoContrato.HasValue)
                    ofertaExistente.IdTipoContrato = data.IdTipoContrato.Value;
                
                if (data.IdLocalidad.HasValue)
                    ofertaExistente.IdLocalidad = data.IdLocalidad.Value;
                
                // Las fechas solo se actualizan si se proporcionan (para permitir edición sin modificar fechas)
                if (data.FechaInicio.HasValue)
                    ofertaExistente.FechaInicio = data.FechaInicio.Value;
                
                if (data.FechaFin.HasValue)
                    ofertaExistente.FechaFin = data.FechaFin.Value;
                
                ofertaExistente.FechaModificacion = DateTime.Now;

                await _unitOfWork.GenericRepository<Oferta>().Update(ofertaExistente);
                
                // Actualizar cupos en OfertaHistorial si se proporciona
                if (data.Cupos.HasValue)
                {
                    var ultimoHistorial = (await _unitOfWork.GenericRepository<OfertaHistorial>()
                        .GetByCriteria(h => h.IdOferta == id && h.FechaBaja == null))
                        .OrderByDescending(h => h.FechaModificacion)
                        .FirstOrDefault();

                    if (ultimoHistorial != null)
                    {
                        ultimoHistorial.Cupos = data.Cupos.Value;
                        ultimoHistorial.FechaModificacion = DateTime.Now;
                        await _unitOfWork.GenericRepository<OfertaHistorial>().Update(ultimoHistorial);
                    }
                }

                // Actualizar carreras si se proporcionan
                // Se crea un nuevo registro OfertaCarrera por cada carrera nueva
                if (data.IdCarreras != null && data.IdCarreras.Count > 0)
                {
                    // Eliminar duplicados si los hay
                    var carrerasUnicas = data.IdCarreras.Distinct().ToList();
                    
                    // Obtener las carreras existentes (activas y eliminadas)
                    var todasCarrerasExistentes = (await _unitOfWork.GenericRepository<OfertaCarrera>()
                        .GetByCriteria(oc => oc.IdOferta == id))
                        .ToList();
                    
                    var carrerasExistentes = todasCarrerasExistentes.Where(oc => oc.FechaBaja == null).ToList();

                    // Marcar como eliminadas las carreras que ya no están en la lista
                    foreach (var carreraExistente in carrerasExistentes)
                    {
                        if (!carrerasUnicas.Contains(carreraExistente.IdCarrera))
                        {
                            carreraExistente.FechaBaja = DateTime.Now;
                            carreraExistente.FechaModificacion = DateTime.Now;
                            await _unitOfWork.GenericRepository<OfertaCarrera>().Update(carreraExistente);
                        }
                    }

                    // Agregar nuevas carreras que no existían o reactivar las que estaban eliminadas
                    // Cada carrera nueva crea su propio registro OfertaCarrera
                    var idsCarrerasExistentes = carrerasExistentes.Select(c => c.IdCarrera).ToList();
                    var idsCarrerasEliminadas = todasCarrerasExistentes
                        .Where(oc => oc.FechaBaja != null)
                        .Select(c => c.IdCarrera)
                        .ToList();
                    var fechaActual = DateTime.Now;
                    
                    foreach (var idCarrera in carrerasUnicas)
                    {
                        // Verificar que la carrera exista
                        var carreraExiste = await _unitOfWork.GenericRepository<Carrera>().GetById(idCarrera);
                        if (carreraExiste == null)
                        {
                            throw new ApiException($"La carrera con ID {idCarrera} no existe", (int)HttpStatusCode.BadRequest);
                        }
                        
                        if (!idsCarrerasExistentes.Contains(idCarrera))
                        {
                            if (idsCarrerasEliminadas.Contains(idCarrera))
                            {
                                var carreraEliminada = todasCarrerasExistentes
                                    .FirstOrDefault(oc => oc.IdCarrera == idCarrera && oc.FechaBaja != null);
                                if (carreraEliminada != null)
                                {
                                    carreraEliminada.FechaBaja = null;
                                    carreraEliminada.FechaModificacion = fechaActual;
                                    await _unitOfWork.GenericRepository<OfertaCarrera>().Update(carreraEliminada);
                                }
                            }
                            else
                            {
                                // Crear un nuevo registro OfertaCarrera para esta carrera
                                var nuevaOfertaCarrera = new OfertaCarrera
                                {
                                    IdOferta = id,
                                    IdCarrera = idCarrera,
                                    FechaAlta = fechaActual,
                                    FechaModificacion = fechaActual
                                };
                                await _unitOfWork.GenericRepository<OfertaCarrera>().Insert(nuevaOfertaCarrera);
                            }
                        }
                    }
                }
                else
                {
                    // Si no se proporcionan carreras, marcar todas las existentes como eliminadas
                    var carrerasExistentes = (await _unitOfWork.GenericRepository<OfertaCarrera>()
                        .GetByCriteria(oc => oc.IdOferta == id && oc.FechaBaja == null))
                        .ToList();
                    
                    foreach (var carreraExistente in carrerasExistentes)
                    {
                        carreraExistente.FechaBaja = DateTime.Now;
                        carreraExistente.FechaModificacion = DateTime.Now;
                        await _unitOfWork.GenericRepository<OfertaCarrera>().Update(carreraExistente);
                    }
                }
                
                await _unitOfWork.CommitAsync();

                // Obtener la oferta actualizada con todas las relaciones
                var ofertaActualizada = await _unitOfWork.GenericRepository<Oferta>()
                    .GetByIdIncludingSpecificRelations(id,
                        q => q.Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                        .Include(o => o.OfertaCarreras)
                        .ThenInclude(oc => oc.Carrera),
                        asNoTracking: true,
                        asSplitQuery: true
                    );

                var ofertaDto = ofertaActualizada.Adapt<OfertaDTO>();

                // Agregar cupos del último historial activo
                var historial = (await _unitOfWork.GenericRepository<OfertaHistorial>()
                    .GetByCriteria(h => h.IdOferta == id && h.FechaBaja == null))
                    .OrderByDescending(h => h.FechaModificacion)
                    .FirstOrDefault();

                ofertaDto.Cupos = historial?.Cupos ?? 1;

                // Contar postulaciones con ÚLTIMO estado Aprobado
                var todosHistoriales = (await _unitOfWork.GenericRepository<PostulacionHistorial>()
                    .GetAllIncludingSpecificRelations(
                        q => q.Include(ph => ph.Postulacion)
                    ))
                    .Where(ph => ph.Postulacion != null 
                              && ph.Postulacion.IdOferta == id 
                              && ph.FechaBaja == null);

                // Agrupar por postulación y tomar solo el último estado de cada una
                var ultimosEstados = todosHistoriales
                    .GroupBy(ph => ph.IdPostulacion)
                    .Select(g => g.OrderByDescending(ph => ph.FechaModificacion).First())
                    .Where(ph => ph.IdEstadoPostulacion == EstadoPostulacion.IdEstadoAprobada);

                ofertaDto.CantidadPostulantes = ultimosEstados.Count();

                return ofertaDto;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task EliminarOferta(int id)
        {
            try
            {
                var oferta = await _unitOfWork.GenericRepository<Oferta>().GetById(id);
                if (oferta == null)
                {
                    throw new ApiException("Oferta no encontrada", (int)HttpStatusCode.NotFound);
                }

                oferta.FechaBaja = DateTime.Now;
                await _unitOfWork.GenericRepository<Oferta>().Update(oferta);
                await _unitOfWork.CommitAsync();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Oferta> GetPublicacionEntidadById(int id)
        {
            try
            {
                Oferta oferta = await _unitOfWork.GenericRepository<Oferta>().GetByIdIncludingSpecificRelations(id, o => o.Include(pe => pe.PerfilEmpresa).ThenInclude(u => u.Usuario));
                if (oferta == null)
                {
                    throw new ApiException("No se encontró la publicación con el ID proporcionado.", (int)HttpStatusCode.NotFound);
                }

                return oferta;
            }
            catch (ApiException)
            {
                //lanzo la excepcion que se captura en el controller
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OfertaDTO> GetOfertaById(int id, int? IdPerfilCandidato = null)
        {
            try
            {
                Oferta oferta = await _unitOfWork.GenericRepository<Oferta>()
                    .GetByIdIncludingSpecificRelations(id,
                        q => q.Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                        .Include(o => o.OfertaCarreras)
                        .ThenInclude(oc => oc.Carrera)
                        .Include(p => p.Postulaciones)
                    );

                if (oferta == null)
                {
                    throw new ApiException("No se encontró la oferta con el ID proporcionado.", (int)HttpStatusCode.NotFound);
                }

                // Verificar si la oferta está eliminada
                if (oferta.FechaBaja != null)
                {
                    throw new ApiException("La oferta no está disponible.", (int)HttpStatusCode.NotFound);
                }

                var ofertaDto = oferta.Adapt<OfertaDTO>();

                // Si se proporciona IdPerfilCandidato, verificar si puede postularse
                if (IdPerfilCandidato.HasValue)
                {
                    bool postulado = oferta.Postulaciones
                        .Any(p => p.IdPerfilCandidato == IdPerfilCandidato.Value && p.FechaBaja == null);
                    ofertaDto.PuedePostularse = !postulado;
                }
                else
                {
                    ofertaDto.PuedePostularse = true;
                }

                // Obtener cupos del último historial activo
                var historial = (await _unitOfWork.GenericRepository<OfertaHistorial>()
                    .GetByCriteria(h => h.IdOferta == id && h.FechaBaja == null))
                    .OrderByDescending(h => h.FechaModificacion)
                    .FirstOrDefault();

                ofertaDto.Cupos = historial?.Cupos ?? 1;

                // Contar postulaciones con ÚLTIMO estado Aprobado
                var todosHistoriales = (await _unitOfWork.GenericRepository<PostulacionHistorial>()
                    .GetAllIncludingSpecificRelations(
                        q => q.Include(ph => ph.Postulacion)
                    ))
                    .Where(ph => ph.Postulacion != null 
                              && ph.Postulacion.IdOferta == id 
                              && ph.FechaBaja == null);

                // Agrupar por postulación y tomar solo el último estado de cada una
                var ultimosEstados = todosHistoriales
                    .GroupBy(ph => ph.IdPostulacion)
                    .Select(g => g.OrderByDescending(ph => ph.FechaModificacion).First())
                    .Where(ph => ph.IdEstadoPostulacion == EstadoPostulacion.IdEstadoAprobada);

                ofertaDto.CantidadPostulantes = ultimosEstados.Count();

                return ofertaDto;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IList<OfertaDTO>> GetPublicaciones(SearchPublicacionesDTO filtro, int IdPerfilCandidato)
        {
            try
            {
                var search = (await _unitOfWork.GenericRepository<Oferta>().Search()).Where(o => o.FechaBaja == null);
                if (filtro.Input != null)
                {
                    search = search.Where(o => o.Titulo.Contains(filtro.Input) || o.Descripcion.Contains(filtro.Input));
                }
                if (filtro.Modalidades != null && filtro.Modalidades.Count > 0)
                {
                    IList<int> idsModalidades = (await _unitOfWork.GenericRepository<Modalidad>().GetByCriteria(m => filtro.Modalidades.Contains(m.Codigo))).Select(m => m.Id).ToList();
                    search = search.Where(o => idsModalidades.Contains(o.IdModalidad));
                }
                if (filtro.TiposContrato != null && filtro.TiposContrato.Count > 0)
                {
                    IList<int> idsTiposContrato = (await _unitOfWork.GenericRepository<TipoContrato>().GetByCriteria(m => filtro.TiposContrato.Contains(m.Codigo))).Select(m => m.Id).ToList();
                    search = search.Where(o => idsTiposContrato.Contains(o.IdTipoContrato));
                }

                search = search.Where(o => o.PerfilEmpresa.FechaBaja == null && o.PerfilEmpresa.IdEstadoValidacion == EstadoValidacion.IdEstadoAprobada);

                List<Oferta> ofertas = search
                    .Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                    .Include(m => m.Modalidad)
                    .Include(tc => tc.TipoContrato)
                    .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                            .ThenInclude(p => p.Pais)
                    .Include(p => p.Postulaciones)
                    .Include(o => o.OfertaCarreras)
                        .ThenInclude(oc => oc.Carrera)
                    .OrderByDescending(o => o.FechaModificacion).ToList();

                var ofertasDto = ofertas.Adapt<List<OfertaDTO>>();

                foreach (var dto in ofertasDto)
                {
                    var ofertaOriginal = ofertas.First(o => o.Id == dto.Id);

                    // Si el candidato tiene una postulación activa, no puede postularse
                    bool postulado = ofertaOriginal.Postulaciones
                        .Any(p => p.IdPerfilCandidato == IdPerfilCandidato && p.FechaBaja == null);

                    //si esta postulado, no puede postularse
                    dto.PuedePostularse = !postulado;

                    // Obtener cupos del último historial activo
                    var historial = (await _unitOfWork.GenericRepository<OfertaHistorial>()
                        .GetByCriteria(h => h.IdOferta == dto.Id && h.FechaBaja == null))
                        .OrderByDescending(h => h.FechaModificacion)
                        .FirstOrDefault();

                    dto.Cupos = historial?.Cupos ?? 1;

                    // Contar postulaciones con ÚLTIMO estado Aprobado
                    var todosHistoriales = (await _unitOfWork.GenericRepository<PostulacionHistorial>()
                        .GetAllIncludingSpecificRelations(
                            q => q.Include(ph => ph.Postulacion)
                        ))
                        .Where(ph => ph.Postulacion != null 
                                  && ph.Postulacion.IdOferta == dto.Id 
                                  && ph.FechaBaja == null);

                    // Agrupar por postulación y tomar solo el último estado de cada una
                    var ultimosEstados = todosHistoriales
                        .GroupBy(ph => ph.IdPostulacion)
                        .Select(g => g.OrderByDescending(ph => ph.FechaModificacion).First())
                        .Where(ph => ph.IdEstadoPostulacion == EstadoPostulacion.IdEstadoAprobada);

                    dto.CantidadPostulantes = ultimosEstados.Count();
                }

                return ofertasDto;

            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<PerfilEmpresa> CrearPerfilEmpresaAutomatico(Usuario usuario)
        {
            try
            {
                // Obtener estado de validación pendiente
                var estadoValidacion = (await _unitOfWork.GenericRepository<EstadoValidacion>()
                    .GetByCriteria(e => e.Codigo == "Pendiente")).FirstOrDefault();

                if (estadoValidacion == null)
                    throw new ApiException("No se encontró el estado de validación 'Pendiente'", (int)HttpStatusCode.InternalServerError);

                // Crear perfil de empresa automático
                var perfilEmpresa = new PerfilEmpresa
                {
                    IdUsuario = usuario.Id,
                    RazonSocial = usuario.Nombre ?? "Empresa " + usuario.Email.Split('@')[0],
                    Cuit = "30-00000000-0", // CUIT genérico
                    Descripcion = "Perfil de empresa creado automáticamente",
                    IdEstadoValidacion = estadoValidacion.Id,
                    FechaAlta = DateTime.Now,
                    FechaModificacion = DateTime.Now,
                    FechaBaja = null
                };

                await _unitOfWork.GenericRepository<PerfilEmpresa>().Insert(perfilEmpresa);
                await _unitOfWork.CommitAsync();

                return perfilEmpresa;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al crear perfil de empresa automático: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }


        public async Task<OfertaRecienteDTO> GetRecientes(string email, int limit)
        {
            try
            {
                // 🔹 Buscar el candidato asociado al email
                var candidato = (await _unitOfWork.GenericRepository<PerfilCandidato>()
                    .GetByCriteriaIncludingSpecificRelations(
                        e => e.Usuario.Email == email && e.FechaBaja == null,
                        q => q.Include(u => u.Usuario)
                    ))
                    .FirstOrDefault();

                if (candidato == null)
                    throw new Exception($"No se encontró ningún candidato asociado al email: {email}");

                int idCarreraCandidato = candidato.IdCarrera ?? 0;

                // 🔹 Obtener las ofertas con sus relaciones
                var ofertasQuery = await _unitOfWork.GenericRepository<Oferta>()
                    .GetAllIncludingSpecificRelations(q => q
                        .Include(o => o.Localidad).ThenInclude(l => l.Provincia)
                        .Include(o => o.TipoContrato)
                        .Include(o => o.Modalidad)
                        .Include(o => o.PerfilEmpresa)
                        .Include(o => o.OfertaCarreras).ThenInclude(oc => oc.Carrera)
                        .Include(o => o.Postulaciones)
                    );

                // 🔹 Filtrar por carrera y por ofertas activas
                var ofertas = ofertasQuery
                    .Where(o => o.FechaBaja == null &&
                                o.OfertaCarreras.Any(oc => oc.IdCarrera == idCarreraCandidato))
                    .OrderByDescending(o => o.FechaModificacion)
                    .Take(limit)
                    .ToList();

                // 🔹 Mapear a DTO
                var dto = new OfertaRecienteDTO
                {
                    CantidadOfertas = ofertas.Count,
                    Ofertas = ofertas.Select(o => new OfertaDTO
                    {
                        Id = o.Id,
                        Titulo = o.Titulo,
                        Descripcion = o.Descripcion,
                        NombreLocalidad = o.Localidad?.Nombre,
                        NombreProvincia = o.Localidad?.Provincia?.Nombre,
                        NombreEmpresa = o.PerfilEmpresa?.RazonSocial,
                        TipoContrato = o.TipoContrato?.Codigo,
                        Modalidad = o.Modalidad?.Codigo,
                        FechaInicio = o.FechaInicio.ToString("dd/MM/yyyy"),
                        FechaFin = o.FechaFin?.ToString("dd/MM/yyyy") ?? "",
                        NombreCarrera = o.OfertaCarreras != null && o.OfertaCarreras.Any(oc => oc.FechaBaja == null && oc.Carrera != null)
                            ? string.Join(", ", o.OfertaCarreras.Where(oc => oc.FechaBaja == null && oc.Carrera != null).Select(oc => oc.Carrera.Nombre))
                            : null,
                        CantidadPostulantes = o.Postulaciones?.Count ?? 0,
                        CartaPresentacion = null,
                        Observacion = null,
                        PuedePostularse = true
                    }).ToList()
                };

                return dto;
            }
            catch (ApiException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ofertas recientes: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Devuelve todas las publicaciones (ofertas) de una empresa según el email del usuario.
        /// </summary>
        public async Task<IList<OfertaDTO>> GetPublicacionesEmpresa(string email)
        {
            try
            {
                // 🔹 Buscamos el perfil de empresa asociado al email
                var empresa = (await _unitOfWork.GenericRepository<PerfilEmpresa>()
                    .GetByCriteriaIncludingSpecificRelations(
                        e => e.Usuario.Email == email,
                        q => q.Include(u => u.Usuario)
                    ))
                    .FirstOrDefault();

                if (empresa == null)
                    throw new Exception($"No se encontró ninguna empresa asociada al email: {email}");

                // 🔹 Buscamos las ofertas publicadas por esta empresa con relaciones necesarias
                var publicaciones = (await _unitOfWork.GenericRepository<Oferta>()
                    .GetByCriteriaIncludingSpecificRelations(
                        o => o.IdPerfilEmpresa == empresa.Id && o.FechaBaja == null,
                        q => q
                            .Include(m => m.Modalidad)
                            .Include(t => t.TipoContrato)
                            .Include(l => l.Localidad).ThenInclude(p => p.Provincia)
                            .Include(o => o.OfertaCarreras).ThenInclude(oc => oc.Carrera)
                    ))
                    .ToList();

                // 🔹 Mapeo manual al DTO
                var resultado = new List<OfertaDTO>();
                
                foreach (var o in publicaciones)
                {
                    // Obtener cupos del último historial activo
                    var historial = (await _unitOfWork.GenericRepository<OfertaHistorial>()
                        .GetByCriteria(h => h.IdOferta == o.Id && h.FechaBaja == null))
                        .OrderByDescending(h => h.FechaModificacion)
                        .FirstOrDefault();

                    // Contar postulaciones con ÚLTIMO estado Aprobado
                    var todosHistoriales = (await _unitOfWork.GenericRepository<PostulacionHistorial>()
                        .GetAllIncludingSpecificRelations(
                            q => q.Include(ph => ph.Postulacion)
                        ))
                        .Where(ph => ph.Postulacion != null 
                                  && ph.Postulacion.IdOferta == o.Id 
                                  && ph.FechaBaja == null);

                    // Agrupar por postulación y tomar solo el último estado de cada una
                    var ultimosEstados = todosHistoriales
                        .GroupBy(ph => ph.IdPostulacion)
                        .Select(g => g.OrderByDescending(ph => ph.FechaModificacion).First())
                        .Where(ph => ph.IdEstadoPostulacion == EstadoPostulacion.IdEstadoAprobada);

                    var ofertaDto = new OfertaDTO
                    {
                        Id = o.Id,
                        Titulo = o.Titulo,
                        Descripcion = o.Descripcion,
                        Modalidad = o.Modalidad?.Nombre,
                        TipoContrato = o.TipoContrato?.Nombre,
                        NombreEmpresa = o.PerfilEmpresa?.RazonSocial,
                        NombreLocalidad = o.Localidad?.Nombre,
                        NombreCarrera = o.OfertaCarreras != null && o.OfertaCarreras.Any(oc => oc.FechaBaja == null && oc.Carrera != null)
                            ? string.Join(", ", o.OfertaCarreras.Where(oc => oc.FechaBaja == null && oc.Carrera != null).Select(oc => oc.Carrera.Nombre))
                            : null,
                        Cupos = historial?.Cupos ?? 1,
                        CantidadPostulantes = ultimosEstados.Count(),
                        FechaInicio = o.FechaInicio.ToString("yyyy-MM-dd"),
                        FechaFin = o.FechaFin?.ToString("yyyy-MM-dd"),
                    };
                    
                    resultado.Add(ofertaDto);
                }

                return resultado;
            }
            catch (ApiException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener publicaciones de la empresa: {ex.Message}", ex);
            }
        }
        public async Task<IList<PostulacionDTO>> GetPostulacionesEmpresa(string email)
        {
            try
            {
                // 1️⃣ Buscamos el perfil de empresa asociado al email del usuario
                var empresa = (await _unitOfWork.GenericRepository<PerfilEmpresa>()
                    .GetByCriteriaIncludingSpecificRelations(
                        e => e.Usuario.Email == email && e.FechaBaja == null,
                        q => q.Include(u => u.Usuario)
                    ))
                    .FirstOrDefault();

                if (empresa == null)
                    throw new Exception($"No se encontró ninguna empresa asociada al email: {email}");

                // 2️⃣ Obtenemos las postulaciones de las ofertas publicadas por esta empresa
                var postulaciones = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.Oferta.IdPerfilEmpresa == empresa.Id && p.FechaBaja == null,
                        q => q
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.Modalidad)
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.TipoContrato)
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.Localidad)
                                    .ThenInclude(l => l.Provincia)
                                        .ThenInclude(pr => pr.Pais)
                            .Include(p => p.Oferta.PerfilEmpresa)
                            .Include(p => p.Historial.OrderByDescending(h => h.FechaAlta).Take(1))
                                .ThenInclude(h => h.EstadoPostulacion)
                            .Include(p => p.PerfilCandidato)
                                .ThenInclude(pc => pc.Usuario)
                    ))
                    .OrderByDescending(p => p.FechaModificacion).ToList();

                // 3️⃣ Proyectamos a DTO
                var result = postulaciones.Select(p => new PostulacionDTO
                {
                    Id = p.Id,
                    IdPerfilCandidato = p.IdPerfilCandidato,
                    NombreCandidato = p.PerfilCandidato?.Usuario.Nombre ?? "Candidato desconocido",
                    IdOferta = p.IdOferta,
                    CartaPresentacion = p.CartaPresentacion,
                    Observacion = p.Observacion,
                    EstadoPostulacion = p.Historial
                        .OrderByDescending(h => h.FechaAlta)
                        .FirstOrDefault()?.EstadoPostulacion?.Nombre ?? "Sin estado",
                    FechaPostulacion = p.FechaAlta.ToString("yyyy-MM-dd"),
                    NombreEmpresa = p.Oferta?.PerfilEmpresa?.RazonSocial ?? "Empresa desconocida",
                    TituloOferta = p.Oferta?.Titulo ?? "Sin título",
                    DescripcionOferta = p.Oferta?.Descripcion,
                    DescripcionModalidad = p.Oferta?.Modalidad?.Nombre ?? "-",
                    DescripcionTipoContrato = p.Oferta?.TipoContrato?.Nombre ?? "-",
                    DescripcionLocalidad = p.Oferta?.Localidad?.Nombre ?? "-",
                    DescripcionProvincia = p.Oferta?.Localidad?.Provincia?.Nombre ?? "-",
                    DescripcionPais = p.Oferta?.Localidad?.Provincia?.Pais?.Nombre ?? "-"
                }).ToList();

                return result;
            }
            catch (ApiException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener postulaciones de la empresa: {ex.Message}", ex);
            }
        }
    }
}