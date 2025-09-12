using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using BussinessLogic.Services;
using BussinessLogic.DTO;
using System.Net;

namespace API_Client.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CandidatoController : GenericController
{
    private readonly ServiceCandidato _serviceCandidato;

    public CandidatoController(ServiceCandidato serviceCandidato, BussinessLogic.Services.GenericService genericService) 
        : base(genericService)
    {
        _serviceCandidato = serviceCandidato;
    }

    [HttpGet]
    [Route("get_perfil")]
    public async Task<ApiResponse> GetPerfil([FromQuery] int? perfilId = null, [FromQuery] int? usuarioId = null)
    {
        try
        {
            PerfilCandidatoDTO perfil;
            
            if (perfilId.HasValue)
            {
                // Buscar por ID de perfil
                perfil = await _serviceCandidato.GetPerfilById(perfilId.Value);
            }
            else if (usuarioId.HasValue)
            {
                // Buscar por ID de usuario
                perfil = await _serviceCandidato.GetPerfilByUsuarioId(usuarioId.Value);
            }
            else
            {
                // Por defecto usar perfilId = 2 (según los datos que tienes)
                perfil = await _serviceCandidato.GetPerfilById(2);
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
    public async Task<ApiResponse> UpdatePerfil([FromBody] PerfilCandidatoDTO perfilDTO)
    {
        try
        {
            if (perfilDTO == null)
            {
                throw new ApiException("Los datos del perfil son requeridos", (int)HttpStatusCode.BadRequest);
            }

            var perfilActualizado = await _serviceCandidato.UpdatePerfil(perfilDTO);
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

    [HttpPost]
    [Route("upload_cv")]
    public async Task<ApiResponse> UploadCv([FromForm] IFormFile cv, [FromQuery] int perfilId = 2)
    {
        try
        {
            if (cv == null || cv.Length == 0)
            {
                throw new ApiException("Archivo CV requerido", (int)HttpStatusCode.BadRequest);
            }

            // Validar tipo de archivo (opcional)
            var allowedTypes = new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
            if (!allowedTypes.Contains(cv.ContentType))
            {
                throw new ApiException("Tipo de archivo no válido. Solo se permiten PDF, DOC y DOCX", (int)HttpStatusCode.BadRequest);
            }

            // Convertir archivo a byte array
            byte[] cvBytes;
            using (var memoryStream = new MemoryStream())
            {
                await cv.CopyToAsync(memoryStream);
                cvBytes = memoryStream.ToArray();
            }

            // Obtener el perfil actual
            var perfilActual = await _serviceCandidato.GetPerfilById(perfilId);
            
            // Actualizar solo el CV
            perfilActual.Cv = Convert.ToBase64String(cvBytes);
            
            var perfilActualizado = await _serviceCandidato.UpdatePerfil(perfilActual);
            
            return new ApiResponse("CV subido exitosamente");
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