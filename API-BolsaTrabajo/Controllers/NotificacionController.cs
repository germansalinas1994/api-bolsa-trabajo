using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using BussinessLogic.Services;
using BussinessLogic.DTO;
using System.Net;

namespace API_Client.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificacionController : GenericController
{
    private readonly ServiceNotificacion _serviceNotificacion;

    public NotificacionController(ServiceNotificacion serviceNotificacion, GenericService genericService) 
        : base(genericService)
    {
        _serviceNotificacion = serviceNotificacion;
    }

    /// <summary>
    /// Obtiene todas las notificaciones del usuario autenticado
    /// </summary>
    [HttpGet]
    [Route("mis_notificaciones")]
    public async Task<ApiResponse> GetMisNotificaciones([FromQuery] bool? soloNoLeidas = null)
    {
        try
        {
            string email = UserEmailFromJWT();
            var notificaciones = await _serviceNotificacion.GetNotificacionesByEmail(email, soloNoLeidas);
            return new ApiResponse("Notificaciones obtenidas exitosamente", notificaciones);
        }
        catch (ApiException e)
        {
            throw e;
        }
        catch (Exception ex)
        {
            throw new ApiException(ex);
        }
    }

    /// <summary>
    /// Obtiene el contador de notificaciones del usuario autenticado
    /// </summary>
    [HttpGet]
    [Route("contador")]
    public async Task<ApiResponse> GetContador()
    {
        try
        {
            string email = UserEmailFromJWT();
            var contador = await _serviceNotificacion.GetContadorNotificaciones(email);
            return new ApiResponse(contador);
        }
        catch (ApiException e)
        {
            throw e;
        }
        catch (Exception ex)
        {
            throw new ApiException(ex);
        }
    }

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    [HttpPut]
    [Route("{id}/marcar_leida")]
    public async Task<ApiResponse> MarcarComoLeida([FromRoute] int id)
    {
        try
        {
            if (id == 0)
                throw new ApiException("El ID de la notificación es requerido", (int)HttpStatusCode.BadRequest);

            string email = UserEmailFromJWT();
            var notificacion = await _serviceNotificacion.MarcarComoLeida(id, email);
            return new ApiResponse("Notificación marcada como leída", notificacion);
        }
        catch (ApiException e)
        {
            throw e;
        }
        catch (Exception ex)
        {
            throw new ApiException(ex);
        }
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas
    /// </summary>
    [HttpPut]
    [Route("marcar_todas_leidas")]
    public async Task<ApiResponse> MarcarTodasComoLeidas()
    {
        try
        {
            string email = UserEmailFromJWT();
            await _serviceNotificacion.MarcarTodasComoLeidas(email);
            return new ApiResponse("Todas las notificaciones fueron marcadas como leídas");
        }
        catch (ApiException e)
        {
            throw e;
        }
        catch (Exception ex)
        {
            throw new ApiException(ex);
        }
    }

    /// <summary>
    /// Elimina una notificación (soft delete)
    /// </summary>
    [HttpDelete]
    [Route("{id}")]
    public async Task<ApiResponse> EliminarNotificacion([FromRoute] int id)
    {
        try
        {
            if (id == 0)
                throw new ApiException("El ID de la notificación es requerido", (int)HttpStatusCode.BadRequest);

            string email = UserEmailFromJWT();
            await _serviceNotificacion.EliminarNotificacion(id, email);
            return new ApiResponse("Notificación eliminada exitosamente");
        }
        catch (ApiException e)
        {
            throw e;
        }
        catch (Exception ex)
        {
            throw new ApiException(ex);
        }
    }

    /// <summary>
    /// Crea una nueva notificación (solo para testing o uso interno)
    /// </summary>
    [HttpPost]
    [Route("crear")]
    public async Task<ApiResponse> CrearNotificacion([FromBody] CrearNotificacionDTO data)
    {
        try
        {
            if (data == null)
                throw new ApiException("Los datos de la notificación son requeridos", (int)HttpStatusCode.BadRequest);

            var notificacion = await _serviceNotificacion.CrearNotificacion(data);
            return new ApiResponse("Notificación creada exitosamente", notificacion);
        }
        catch (ApiException e)
        {
            throw e;
        }
        catch (Exception ex)
        {
            throw new ApiException(ex);
        }
    }
}
