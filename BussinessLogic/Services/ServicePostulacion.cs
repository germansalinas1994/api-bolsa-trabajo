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
    public class ServicePostulacion : GenericService
    {
        private readonly ServicePublicacion _servicePublicacion;

        public ServicePostulacion(IUnitOfWork unitOfWork, ServicePublicacion servicePublicacion)
            : base(unitOfWork)
        {
            _servicePublicacion = servicePublicacion;
        }
        public async Task CrearPostulacion(PostulacionDTO data, string email)
        {
            bool commitRealizado = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                Usuario usuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault();

                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);


                PerfilCandidato perfilCandidato = (await _unitOfWork.GenericRepository<PerfilCandidato>()
                    .GetByCriteria(u => u.IdUsuario == usuario.Id)).FirstOrDefault();
                    
                if (perfilCandidato == null)  
                    throw new ApiException("El perfil de candidato no existe para el usuario", (int)HttpStatusCode.NotFound);


                //busco que no exista una postulacion igual para la misma oferta y candidato
                var postulacionExistente = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteria(p => p.IdOferta == data.IdOferta && p.IdPerfilCandidato == perfilCandidato.Id && p.FechaBaja == null)).FirstOrDefault();
                if (postulacionExistente != null)
                    throw new ApiException("Ya existe una postulación para esta oferta con el mismo perfil de candidato", (int)HttpStatusCode.Conflict);


                //Recupero la oferta
                Oferta oferta = await _servicePublicacion.GetPublicacionEntidadById(data.IdOferta.Value);
                //recupero el candidato
                if (oferta == null)
                    throw new ApiException("La oferta no existe", (int)HttpStatusCode.NotFound);
       

                Postulacion nuevaPostulacion = new();
                nuevaPostulacion.IdOferta = oferta.Id;
                nuevaPostulacion.IdPerfilCandidato = perfilCandidato.Id;
                nuevaPostulacion.CartaPresentacion = data.CartaPresentacion;
                nuevaPostulacion.Observacion = data.Observacion;
                nuevaPostulacion.FechaAlta = DateTime.Now;
                nuevaPostulacion.FechaModificacion = DateTime.Now;
                Postulacion postulacionPersistida = await _unitOfWork.GenericRepository<Postulacion>().Insert(nuevaPostulacion);

                //produzco un error aproposito para verificar que funcione la transaccion


                PostulacionHistorial historial = new();
                historial.IdPostulacion = postulacionPersistida.Id;

                EstadoPostulacion estadoIniciada = await _unitOfWork.GenericRepository<EstadoPostulacion>().GetById(EstadoPostulacion.IdEstadoIniciada);
                if (estadoIniciada == null)
                    throw new ApiException("El estado 'Iniciada' no está definido en la base de datos.", (int)HttpStatusCode.NotFound);

                historial.IdEstadoPostulacion = estadoIniciada.Id;
                historial.FechaAlta = DateTime.Now;
                historial.FechaModificacion = DateTime.Now;
                historial.Motivo = "Postulación creada";
                await _unitOfWork.GenericRepository<PostulacionHistorial>().Insert(historial);

                // throw new Exception("Error de prueba para verificar la transacción");


                await _unitOfWork.CommitAsync();
                commitRealizado = true;


            }
            catch (ApiException)
            {
                throw; // Re-lanzar la excepción ApiException sin envolverla nuevamente
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!commitRealizado)
                {
                    await _unitOfWork.RollbackAsync();
                }
            }
        }

        public async Task<List<PostulacionDTO>> GetPostulaciones(string email)
        {
            try
            {
                int idUsuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault().Id;

                if (idUsuario == 0)
                {
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);
                }
                
                int idPerfil = (await _unitOfWork.GenericRepository<PerfilCandidato>()
                    .GetByCriteria(u => u.IdUsuario == idUsuario)).FirstOrDefault().Id;

                List<Postulacion> postulaciones = (await _unitOfWork
                    .GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        x => x.IdPerfilCandidato == idPerfil,
                        q => q
                            .Include(p => p.Oferta)
                            .ThenInclude(to => to.TipoContrato)
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.PerfilEmpresa)
                            .Include(p => p.Oferta)
                                .ThenInclude(m => m.Modalidad)
                            .Include(p => p.Historial
                                .OrderByDescending(h => h.FechaModificacion)
                                .Take(1))
                                .ThenInclude(h => h.EstadoPostulacion)
                            .Include(p => p.PerfilCandidato)

                    )).ToList();


                return postulaciones.Adapt<List<PostulacionDTO>>();
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

        public async Task<PostulacionDTO> GetPostulacionById(int idPostulacion)
        {
            try
            {
                Postulacion _postulacion = await _unitOfWork.GenericRepository<Postulacion>().GetByIdIncludingSpecificRelations(idPostulacion,
                q => q.Include(h => h.Historial).ThenInclude(ep => ep.EstadoPostulacion)
                .Include(o => o.Oferta).ThenInclude(pf => pf.PerfilEmpresa)
                .Include(o => o.Oferta).ThenInclude(m => m.Modalidad)
                .Include(o => o.Oferta).ThenInclude(tc => tc.TipoContrato)
                .Include(o => o.Oferta).ThenInclude(l => l.Localidad).ThenInclude(p => p.Provincia).ThenInclude(pa => pa.Pais)
                .Include(pc => pc.PerfilCandidato)
                 );
                /*                 Postulacion _postulacion = await _unitOfWork.GenericRepository<Postulacion>().GetById(idPostulacion); 
                 */
                if (_postulacion == null)
                    throw new ApiException("No existe la postulación", (int)HttpStatusCode.NotFound);
                return _postulacion.Adapt<PostulacionDTO>();
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

        // GET último mes por idPerfilCandidato
        public async Task<IList<PostulacionDTO>> GetUltimoMesByEstudiante(int idPerfilCandidato, CancellationToken ct = default)
        {
            try
            {
                var desde = DateTime.UtcNow.AddDays(-30);

                var postulaciones = await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.IdPerfilCandidato == idPerfilCandidato &&
                             p.FechaBaja == null &&
                             p.FechaAlta >= desde,
                        include: q => q
                            .AsNoTracking()
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.PerfilEmpresa)
                                    .ThenInclude(pe => pe.Usuario)
                    );

                var postIds = postulaciones.Select(p => p.Id).Distinct().ToList();
                if (postIds.Count == 0) return new List<PostulacionDTO>();

                var historiales = await _unitOfWork.GenericRepository<PostulacionHistorial>()
                    .GetByCriteriaIncludingSpecificRelations(
                        h => postIds.Contains(h.IdPostulacion),
                        include: q => q
                            .AsNoTracking()
                            .Include(h => h.EstadoPostulacion)
                    );

                var estados = historiales
                    .GroupBy(h => h.IdPostulacion)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(h => h.FechaBaja == null)
                              .OrderByDescending(h => h.FechaAlta)
                              .Select(h => h.EstadoPostulacion.Nombre)
                              .FirstOrDefault()
                           ?? g.OrderByDescending(h => h.FechaAlta)
                               .Select(h => h.EstadoPostulacion.Nombre)
                               .FirstOrDefault()
                           ?? "En revisión"
                    );

                var ctx = new MapContext();
                ctx.Parameters["Estados"] = estados;

                var result = postulaciones
                .OrderByDescending(p => p.FechaAlta)
                .ToList()
                .BuildAdapter()                           // <-- crea el adaptador para esta conversión
                .AddParameters("Estados", estados)        // <-- pasa parámetros al mapeo (MapContext.Parameters)
                .AdaptToType<List<PostulacionDTO>>();     // <-- destino

                return result;
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }
        
        public async Task<IList<PostulacionDTO>> GetPostulacionesPorOferta(int idOferta)
        {
            try
            {
                var postulaciones = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.IdOferta == idOferta && p.FechaBaja == null,
                        q => q
                            .Include(p => p.PerfilCandidato)
                                .ThenInclude(pc => pc.Usuario)
                            .Include(p => p.Historial)
                                .ThenInclude(h => h.EstadoPostulacion)
                    )).ToList();

                var resultado = postulaciones.Select(p => new PostulacionDTO
                {
                    Id = p.Id,
                    IdPerfilCandidato = p.IdPerfilCandidato,
                    IdOferta = p.IdOferta,
                    CartaPresentacion = p.CartaPresentacion,
                    Observacion = p.Observacion,
                    EstadoPostulacion = p.Historial?
                        .OrderByDescending(h => h.FechaAlta)
                        .FirstOrDefault()?.EstadoPostulacion?.Nombre ?? "Sin estado",
                    FechaPostulacion = p.FechaAlta.ToString("yyyy-MM-dd"),
                    TituloOferta = p.Oferta?.Titulo,
                    NombreEmpresa = p.Oferta?.PerfilEmpresa?.RazonSocial
                }).ToList();

                return resultado;
            }
            catch (ApiException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener postulaciones de la oferta: {ex.Message}", ex);
            }
        }
    }
}