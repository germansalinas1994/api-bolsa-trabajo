using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using BussinessLogic.Services;
using BussinessLogic.DTO;
using System.Net;

namespace API_Client.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmpresaController : GenericController
{
    private readonly ServiceEmpresa _serviceEmpresa;

    public EmpresaController(ServiceEmpresa serviceEmpresa, BussinessLogic.Services.GenericService genericService) 
        : base(genericService)
    {
        _serviceEmpresa = serviceEmpresa;
    }

    [HttpGet]
    [Route("get_perfil")]
    public async Task<ApiResponse> GetPerfil([FromQuery] int? perfilId = null, [FromQuery] int? usuarioId = null)
    {
        try
        {
            PerfilEmpresaDTO perfil;
            
            if (perfilId.HasValue)
            {
                // Buscar por ID de perfil
                perfil = await _serviceEmpresa.GetPerfilById(perfilId.Value);
            }
            else if (usuarioId.HasValue)
            {
                // Buscar por ID de usuario
                perfil = await _serviceEmpresa.GetPerfilByUsuarioId(usuarioId.Value);
            }
            else
            {
                // Por defecto usar perfilId = 1 (puedes ajustar según tus datos)
                perfil = await _serviceEmpresa.GetPerfilById(1);
            }
            
            return new ApiResponse(perfil);
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

    [HttpPut]
    [Route("update_perfil")]
    public async Task<ApiResponse> UpdatePerfil([FromBody] PerfilEmpresaDTO perfilDTO)
    {
        try
        {
            if (perfilDTO == null)
            {
                throw new ApiException("Los datos del perfil son requeridos", (int)HttpStatusCode.BadRequest);
            }

            var perfilActualizado = await _serviceEmpresa.UpdatePerfil(perfilDTO);
            return new ApiResponse(perfilActualizado);
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

