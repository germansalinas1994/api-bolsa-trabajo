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




}

public class CambioEstadoValidacionDTO
{
    public int IdPerfilEmpresa { get; set; }
    public bool Aprobado { get; set; }
}
