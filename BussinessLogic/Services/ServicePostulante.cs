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
                await _unitOfWork.BeginTransactionAsync();

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
    }
}