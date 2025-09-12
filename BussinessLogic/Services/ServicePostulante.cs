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
    public class ServicePostulante : GenericService
    {
        private readonly ServicePublicacion _servicePublicacion;

        public ServicePostulante(IUnitOfWork unitOfWork, ServicePublicacion servicePublicacion)
            : base(unitOfWork)
        {
            _servicePublicacion = servicePublicacion;
        }
        public async Task CrearPostulacion(PostulacionDTO data)
        {
            bool commitRealizado = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(); //transaccion en bd para rollback

                //busco que no exista una postulacion igual para la misma oferta y candidato
                var postulacionExistente = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteria(p => p.IdOferta == data.IdOferta && p.IdPerfilCandidato == data.IdPerfilCandidato && p.FechaBaja == null)).FirstOrDefault();
                if (postulacionExistente != null)
                    throw new ApiException("Ya existe una postulación para esta oferta con el mismo perfil de candidato", (int)HttpStatusCode.Conflict);


                //Recupero la oferta
                Oferta oferta = await _servicePublicacion.GetPublicacionEntidadById(data.IdOferta.Value);
                //recupero el candidato
                PerfilCandidato candidato = await _unitOfWork.GenericRepository<PerfilCandidato>().GetById(data.IdPerfilCandidato.Value);
                if (candidato == null)
                {
                    throw new ApiException("El perfil de candidato no existe", (int)HttpStatusCode.NotFound);
                }

                Postulacion nuevaPostulacion = new();
                nuevaPostulacion.IdOferta = oferta.Id;
                nuevaPostulacion.IdPerfilCandidato = candidato.Id;
                nuevaPostulacion = data.Adapt<Postulacion>();
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

        public async Task<List<PostulacionDTO>> GetPostulaciones()
        {
            try
            {
                List<Postulacion> postulaciones = (await _unitOfWork
                    .GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        x => x.IdPerfilCandidato == 1,
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
        public async Task<IList<PostulacionDTO>> GetPostulacionesPorEstado(int idPerfilCandidato, int idEstado)
        {
            try
            {
                var query = (await _unitOfWork.GenericRepository<Postulacion>().Search())
                    .AsNoTracking()
                    .Where(p => p.IdPerfilCandidato == idPerfilCandidato)
                    .Where(p =>
                        p.Historial
                        .OrderByDescending(h => h.FechaModificacion)
                        .Select(h => h.IdEstadoPostulacion)
                        .FirstOrDefault() == idEstado
                    );

                var postulaciones = await query
                    .Include(p => p.Oferta).ThenInclude(o => o.PerfilEmpresa).ThenInclude(pe => pe.Usuario)
                    .Include(p => p.Oferta).ThenInclude(o => o.Modalidad)
                    .Include(p => p.Oferta).ThenInclude(o => o.TipoContrato)
                    .Include(p => p.PerfilCandidato)
                    .Include(p => p.Historial.OrderByDescending(h => h.FechaModificacion).Take(1))
                        .ThenInclude(h => h.EstadoPostulacion)
                    .ToListAsync();

                return postulaciones.Adapt<List<PostulacionDTO>>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch
            {
                throw; // conserva el stack trace
            }
        }

    }
}