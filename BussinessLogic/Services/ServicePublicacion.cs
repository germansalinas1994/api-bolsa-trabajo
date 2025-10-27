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
                Usuario usuario = (await _unitOfWork.GenericRepository<Usuario>().GetByCriteria(u=>u.Email == email)).FirstOrDefault();
                PerfilEmpresa perfil = (await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(p=>p.IdUsuario == usuario.Id)).FirstOrDefault();
                List<Oferta> ofertas = (await _unitOfWork.GenericRepository<Oferta>()
                    .GetAllIncludingSpecificRelations(
                        q => q.Where(o => o.IdPerfilEmpresa == perfil.Id)
                        .Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                    )
                ).OrderByDescending(p => p.FechaModificacion).ToList();

                return ofertas.Adapt<List<OfertaDTO>>();
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
                Usuario usuario = (await _unitOfWork.GenericRepository<Usuario>().GetByCriteria(e=>e.Email == email)).FirstOrDefault();
                if (usuario == null)
                    throw new ApiException("no existe el usuario", (int)HttpStatusCode.NotFound);
                
                PerfilEmpresa perfil = (await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(u=>u.IdUsuario == usuario.Id)).FirstOrDefault();
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
                await _unitOfWork.CommitAsync();

                // Obtener la oferta creada con todas las relaciones
                var ofertaCreada = await _unitOfWork.GenericRepository<Oferta>()
                    .GetByIdIncludingSpecificRelations(oferta.Id,
                        q => q.Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                    );

                return ofertaCreada.Adapt<OfertaDTO>();
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

                // Actualizar propiedades
                ofertaExistente.Titulo = data.Titulo;
                ofertaExistente.Descripcion = data.Descripcion;
                ofertaExistente.IdModalidad = data.IdModalidad ?? ofertaExistente.IdModalidad;
                ofertaExistente.IdTipoContrato = data.IdTipoContrato ?? ofertaExistente.IdTipoContrato;
                ofertaExistente.IdLocalidad = data.IdLocalidad ?? ofertaExistente.IdLocalidad;
                ofertaExistente.FechaInicio = data.FechaInicio ?? ofertaExistente.FechaInicio;
                ofertaExistente.FechaFin = data.FechaFin ?? ofertaExistente.FechaFin;
                ofertaExistente.FechaModificacion = DateTime.Now;

                await _unitOfWork.GenericRepository<Oferta>().Update(ofertaExistente);
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
                    );

                return ofertaActualizada.Adapt<OfertaDTO>();
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
                Oferta oferta = await _unitOfWork.GenericRepository<Oferta>().GetById(id);
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

        public async Task<IList<OfertaDTO>> GetPublicaciones(SearchPublicacionesDTO filtro)
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

                List<Oferta> oferta = search
                    .Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                    .Include(m => m.Modalidad)
                    .Include(tc => tc.TipoContrato)
                    .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                            .ThenInclude(p => p.Pais)
                    .OrderByDescending(o => o.FechaModificacion).ToList();

                return oferta.Adapt<List<OfertaDTO>>();
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


        public async Task<OfertaRecienteDTO> GetRecientes(int limit)
        {
            OfertaRecienteDTO ofertaReciente = new OfertaRecienteDTO();
            try
            {
                List<Oferta> o = (await _unitOfWork.GenericRepository<Oferta>()//aca digo que voy a la tabla oferta
                .GetAllIncludingSpecificRelations(q => q.Include(l => l.Localidad).ThenInclude(p => p.Provincia)
                .Include(tc => tc.TipoContrato)
                .Include(m => m.Modalidad)
                .Include(e => e.PerfilEmpresa)

            )).Where(f => f.FechaBaja == null).OrderByDescending(f => f.FechaModificacion).ToList();
                // int cantidad = o.Count;
                ofertaReciente.Ofertas = o.Adapt<List<OfertaDTO>>().ToList(); //mapeo a DTO y tomo los primeros 'limit' elementos
                ofertaReciente.CantidadOfertas = o.Count;
                return ofertaReciente; //mapeo a DTO y retorno
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw ex; }
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
                            .Include(o => o.OfertaCarreras).ThenInclude(oc => oc.Carrera) // Incluye carreras
                            .Include(o => o.Postulaciones) // Incluye postulaciones
                    ))
                    .ToList();

                // 🔹 Mapeo manual al DTO para agregar carrera y cantidad de postulantes
                var resultado = publicaciones.Select(o => new OfertaDTO
                {
                    Id = o.Id,
                    Titulo = o.Titulo,
                    Descripcion = o.Descripcion,
                    Modalidad = o.Modalidad?.Nombre,
                    TipoContrato = o.TipoContrato?.Nombre,
                    NombreEmpresa = o.PerfilEmpresa?.RazonSocial,
                    NombreLocalidad = o.Localidad?.Nombre,
                    NombreCarrera = o.OfertaCarreras.FirstOrDefault()?.Carrera?.Nombre ?? "Carrera no especificada",
                    CantidadPostulantes = o.Postulaciones?.Count() ?? 0,
                    FechaInicio = o.FechaInicio.ToString("yyyy-MM-dd"),
                    FechaFin = o.FechaFin?.ToString("yyyy-MM-dd"),
                }).ToList();

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