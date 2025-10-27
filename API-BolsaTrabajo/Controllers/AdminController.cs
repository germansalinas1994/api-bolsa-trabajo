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

    [HttpGet]
    [Route("get_empresas_por_verificar")]
    public async Task<ApiResponse> GetEmpresasPorVerificar()
    {
        try
        {
            int idUsuario = await GetIdUsuarioFromJWT();
            List<PerfilEmpresaDTO> empresas = await _service.GetEmpresasPorVerificar(idUsuario);
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


}