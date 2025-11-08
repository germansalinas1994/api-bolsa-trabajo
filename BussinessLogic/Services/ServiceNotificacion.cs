using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using AutoWrapper.Wrappers;
using System.Net;

namespace BussinessLogic.Services
{
    public class ServiceNotificacion : GenericService
    {
        public ServiceNotificacion(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Crea una nueva notificación
        /// </summary>
        public async Task<NotificacionDTO> CrearNotificacion(CrearNotificacionDTO data)
        {
            try
            {
                // Verificar que el usuario existe
                var usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(data.IdUsuario);
                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);

                // Verificar que la postulación existe
                var postulacion = await _unitOfWork.GenericRepository<Postulacion>().GetById(data.IdPostulacion);
                if (postulacion == null)
                    throw new ApiException("La postulación no existe", (int)HttpStatusCode.NotFound);

                var notificacion = new Notificacion
                {
                    IdUsuario = data.IdUsuario,
                    Mensaje = data.Mensaje,
                    Asunto = data.Asunto,
                    IdPostulacion = data.IdPostulacion,
                    Leido = false,
                    FechaEnvio = DateTime.Now,
                    FechaAlta = DateTime.Now,
                    FechaModificacion = DateTime.Now
                };

                await _unitOfWork.BeginTransactionAsync();
                var notificacionCreada = await _unitOfWork.GenericRepository<Notificacion>().Insert(notificacion);
                await _unitOfWork.CommitAsync();

                return notificacionCreada.Adapt<NotificacionDTO>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al crear la notificación: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Obtiene todas las notificaciones de un usuario por email
        /// </summary>
        public async Task<List<NotificacionDTO>> GetNotificacionesByEmail(string email, bool? soloNoLeidas = null)
        {
            try
            {
                var usuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault();

                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);

                var query = await _unitOfWork.GenericRepository<Notificacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        n => n.IdUsuario == usuario.Id && n.FechaBaja == null,
                        q => q
                            .Include(n => n.Postulacion)
                                .ThenInclude(p => p.Oferta)
                                    .ThenInclude(o => o.PerfilEmpresa)
                            .Include(n => n.Postulacion)
                                .ThenInclude(p => p.PerfilCandidato)
                                    .ThenInclude(pc => pc.Usuario)
                            .Include(n => n.Usuario)
                    );

                var notificaciones = query
                    .Where(n => !soloNoLeidas.HasValue || n.Leido != soloNoLeidas.Value)
                    .OrderByDescending(n => n.FechaEnvio)
                    .ToList();

                return notificaciones.Select(n => new NotificacionDTO
                {
                    Id = n.Id,
                    IdUsuario = n.IdUsuario,
                    Mensaje = n.Mensaje,
                    Leido = n.Leido,
                    FechaEnvio = n.FechaEnvio,
                    Asunto = n.Asunto,
                    IdPostulacion = n.IdPostulacion,
                    TituloOferta = n.Postulacion?.Oferta?.Titulo,
                    NombreEmpresa = n.Postulacion?.Oferta?.PerfilEmpresa?.RazonSocial,
                    NombreCandidato = n.Postulacion?.PerfilCandidato?.Usuario?.Nombre,
                    EmailUsuario = n.Usuario?.Email
                }).ToList();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al obtener las notificaciones: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Obtiene el contador de notificaciones no leídas de un usuario
        /// </summary>
        public async Task<NotificacionCountDTO> GetContadorNotificaciones(string email)
        {
            try
            {
                var usuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault();

                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);

                var notificaciones = await _unitOfWork.GenericRepository<Notificacion>()
                    .GetByCriteria(n => n.IdUsuario == usuario.Id && n.FechaBaja == null);

                var noLeidas = notificaciones.Count(n => !n.Leido);
                var total = notificaciones.Count();

                return new NotificacionCountDTO
                {
                    NoLeidas = noLeidas,
                    Total = total
                };
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al obtener el contador de notificaciones: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        public async Task<NotificacionDTO> MarcarComoLeida(int idNotificacion, string email)
        {
            try
            {
                var usuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault();

                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);

                var notificacion = await _unitOfWork.GenericRepository<Notificacion>().GetById(idNotificacion);
                
                if (notificacion == null)
                    throw new ApiException("La notificación no existe", (int)HttpStatusCode.NotFound);

                if (notificacion.IdUsuario != usuario.Id)
                    throw new ApiException("No tienes permisos para modificar esta notificación", (int)HttpStatusCode.Forbidden);

                notificacion.Leido = true;
                notificacion.FechaModificacion = DateTime.Now;

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.GenericRepository<Notificacion>().Update(notificacion);
                await _unitOfWork.CommitAsync();

                return notificacion.Adapt<NotificacionDTO>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al marcar la notificación como leída: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Marca todas las notificaciones de un usuario como leídas
        /// </summary>
        public async Task MarcarTodasComoLeidas(string email)
        {
            try
            {
                var usuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault();

                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);

                var notificacionesNoLeidas = (await _unitOfWork.GenericRepository<Notificacion>()
                    .GetByCriteria(n => n.IdUsuario == usuario.Id && !n.Leido && n.FechaBaja == null)).ToList();

                await _unitOfWork.BeginTransactionAsync();
                foreach (var notificacion in notificacionesNoLeidas)
                {
                    notificacion.Leido = true;
                    notificacion.FechaModificacion = DateTime.Now;
                    await _unitOfWork.GenericRepository<Notificacion>().Update(notificacion);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al marcar todas las notificaciones como leídas: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Elimina (marca como eliminada) una notificación
        /// </summary>
        public async Task EliminarNotificacion(int idNotificacion, string email)
        {
            try
            {
                var usuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault();

                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);

                var notificacion = await _unitOfWork.GenericRepository<Notificacion>().GetById(idNotificacion);
                
                if (notificacion == null)
                    throw new ApiException("La notificación no existe", (int)HttpStatusCode.NotFound);

                if (notificacion.IdUsuario != usuario.Id)
                    throw new ApiException("No tienes permisos para eliminar esta notificación", (int)HttpStatusCode.Forbidden);

                notificacion.FechaBaja = DateTime.Now;
                notificacion.FechaModificacion = DateTime.Now;

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.GenericRepository<Notificacion>().Update(notificacion);
                await _unitOfWork.CommitAsync();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al eliminar la notificación: {ex.Message}", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
