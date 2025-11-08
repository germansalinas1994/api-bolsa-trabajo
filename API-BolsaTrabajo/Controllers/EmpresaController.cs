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

            // Obtener el email del usuario autenticado desde el JWT
            string userEmail = UserEmailFromJWT();
            
            // Obtener el idPerfilEmpresa del usuario autenticado usando el GenericService heredado
            int idPerfilEmpresaUsuario = await _service.GetPerfilEmpresaUsuario(userEmail);
            
            // Verificar que el perfil que se intenta actualizar pertenece al usuario autenticado
            if (perfilDTO.Id != idPerfilEmpresaUsuario)
            {
                throw new ApiException("No tienes permiso para actualizar este perfil", (int)HttpStatusCode.Forbidden);
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

    [HttpPost]
    [Route("upload_foto_perfil")]
    public async Task<ApiResponse> UploadFotoPerfil([FromForm] IFormFile foto, [FromQuery] int perfilId)
    {
        try
        {
            if (foto == null || foto.Length == 0)
            {
                throw new ApiException("Archivo de foto requerido", (int)HttpStatusCode.BadRequest);
            }

            // Obtener el email del usuario autenticado desde el JWT
            string userEmail = UserEmailFromJWT();
            
            // Obtener el idPerfilEmpresa del usuario autenticado
            int idPerfilEmpresaUsuario = await _service.GetPerfilEmpresaUsuario(userEmail);
            
            // Verificar que el perfil que se intenta actualizar pertenece al usuario autenticado
            if (perfilId != idPerfilEmpresaUsuario)
            {
                throw new ApiException("No tienes permiso para actualizar la foto de este perfil", (int)HttpStatusCode.Forbidden);
            }

            // Validar tipo de archivo - solo imágenes
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(foto.ContentType.ToLowerInvariant()))
            {
                throw new ApiException("Tipo de archivo no válido. Solo se permiten imágenes (JPG, PNG, GIF, WEBP)", (int)HttpStatusCode.BadRequest);
            }

            // Validar extensión del archivo
            var fileExtension = Path.GetExtension(foto.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ApiException("Extensión de archivo no válida. Solo se permiten .jpg, .jpeg, .png, .gif, .webp", (int)HttpStatusCode.BadRequest);
            }

            // Validar tamaño del archivo (máximo 2MB)
            const long maxFileSize = 2 * 1024 * 1024; // 2MB
            if (foto.Length > maxFileSize)
            {
                throw new ApiException("El archivo es demasiado grande. El tamaño máximo permitido es 2MB", (int)HttpStatusCode.BadRequest);
            }

            // Convertir archivo a byte array
            byte[] fotoBytes;
            using (var memoryStream = new MemoryStream())
            {
                await foto.CopyToAsync(memoryStream);
                fotoBytes = memoryStream.ToArray();
            }

            // Obtener el perfil actual
            var perfilActual = await _serviceEmpresa.GetPerfilById(perfilId);

            // Actualizar solo la foto de perfil
            perfilActual.FotoPerfil = Convert.ToBase64String(fotoBytes);

            var perfilActualizado = await _serviceEmpresa.UpdatePerfil(perfilActual);

            return new ApiResponse("Foto de perfil subida exitosamente");
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
    [Route("get_porcentaje_perfil")]
    public async Task<int> GetPorcentajePerfil()
    {
        try
        {
            string email = UserEmailFromJWT();
            var porcentaje = _serviceEmpresa.CalcularPorcentaje(email);
            return porcentaje;
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
