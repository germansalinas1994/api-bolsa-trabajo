using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using BussinessLogic.Services;
using BussinessLogic.DTO;
using System.Net;

namespace API_Client.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : GenericController
{
    private readonly ServiceAdmin _service;

    public AdminController(ServiceAdmin service) : base(service)
    {
        _service = service;
    }

    [HttpPost]
    [Route("get_empresas_por_verificar")]
    public async Task<ApiResponse> GetEmpresasPorVerificar([FromBody] GenericSearchDTO filtros)
    {
        try
        {
            int idUsuario = await GetIdUsuarioFromJWT();
            List<PerfilEmpresaDTO> empresas = await _service.GetEmpresasPorVerificar(idUsuario, filtros);
            return new ApiResponse(empresas);
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

    [HttpPost]
    [Route("cambiar_estado_validacion")]
    public async Task<ApiResponse> CambiarEstadoValidacion([FromBody] CambioEstadoValidacionDTO body)
    {
        try
        {
            int idUsuario = await GetIdUsuarioFromJWT();

            await _service.CambiarEstadoValidacion(body.IdPerfilEmpresa, body.Aprobado, idUsuario);

            return new ApiResponse("Operación realizada con éxito", (int)HttpStatusCode.OK);
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

    [HttpGet]
    [Route("get_usuarios")]
    public async Task<ApiResponse> GetUsuarios()
    {
        try
        {
            List<UsuarioDTO> usuarios = await _service.GetUsuarios();
            return new ApiResponse(usuarios);
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

    [HttpPost]
    [Route("baja_usuario")]
    public async Task<ApiResponse> BajaUsuario([FromBody] int idUsuario)
    {
        try
        {
            int idAdmin = await GetIdUsuarioFromJWT();
            await _service.BajaUsuario(idUsuario, idAdmin);

            return new ApiResponse("Operación realizada con éxito", (int)HttpStatusCode.OK);
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

    [HttpPost]
    [Route("alta_usuario")]
    public async Task<ApiResponse> AltaUsuario([FromBody] int idUsuarioAlta)
    {
        try
        {
            int idUsuario = await GetIdUsuarioFromJWT();
            await _service.AltaUsuario(idUsuarioAlta, idUsuario);

            return new ApiResponse("Operación realizada con éxito", (int)HttpStatusCode.OK);
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

    [HttpPost]
    [Route("ver_detalle_usuario")]
    public async Task<ApiResponse> VerDetalleUsuario([FromBody] int idUsuarioRegistrado)
    {
        try
        {
            PerfilCompletoDTO perfilCompleto = await _service.VerDetalleUsuario(idUsuarioRegistrado);
            return new ApiResponse(perfilCompleto);
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

    [HttpGet]
    [Route("get_dashboard_empresa")]
    public async Task<ApiResponse> GetDashboardAdmin()
    {
        try
        {
            var dashboard = await _service.GetDashboardAdmin();
            return new ApiResponse(dashboard);
        }
        catch (ApiException e)
        {
            throw e;
        }
        catch (Exception ex)
        {
            throw new ApiException("Error al obtener datos del dashboard de empresa", 500, ex.Message);
        }
    }


}

public class CambioEstadoValidacionDTO
{
    public int IdPerfilEmpresa { get; set; }
    public bool Aprobado { get; set; }
}

public class ActualizarRolUsuarioDTO
{
    public int IdUsuario { get; set; }
    public int idRol { get; set; }
}
