using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessLogic.DTO;
using BussinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using DataAccess.Entities;
using System.Net;



namespace API_Client.Controllers
{
    [Route("api/[controller]")]
    public class PostulacionController : GenericController
    {

        private readonly ServicePostulacion _service;

        public PostulacionController(ServicePostulacion service) : base(service)
        {
            _service = service; // ahora sí lo seteás correctamente
        }
        [HttpGet]
        [Route("get_postulaciones")]
        public async Task<ApiResponse> GetPostulaciones()
        {
            try
            {
                string email = UserEmailFromJWT();
                List<PostulacionDTO> postulaciones = await _service.GetPostulaciones(email);
                return new ApiResponse(postulaciones);
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
        [Route("postularse_oferta")]
        public async Task<ApiResponse> PostularseOferta([FromBody] PostulacionDTO data)
        {
            try
            {
                if (data.IdOferta == null)
                {
                    throw new ApiException("IdOferta son obligatorios", (int)HttpStatusCode.BadRequest);
                }

                string email = UserEmailFromJWT();

                await _service.CrearPostulacion(data, email);

                return new ApiResponse("Postulación creada exitosamente");
            }
            catch (ApiException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet]
        [Route("get_postulacion")]
        public async Task<ApiResponse> GetPostulacionById([FromQuery] int idPostulacion)
        {
            try
            {
                if (idPostulacion == 0)
                    throw new ApiException("Debes indicar el ID de la postulación", (int)HttpStatusCode.BadRequest);
                PostulacionDTO _postulacion = await _service.GetPostulacionById(idPostulacion);
                return new ApiResponse("Postulación encontrada exitosamente", _postulacion);
            }
            catch (ApiException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Devuelve las postulaciones del último mes del estudiante (por IdPerfilCandidato).
        /// </summary>
        /// <param name="idEstudiante">Id del perfil candidato</param>
        [HttpGet("{idEstudiante:int}/ultimo-mes")]
        [ProducesResponseType(typeof(IEnumerable<PostulacionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> GetUltimoMes(int idEstudiante, CancellationToken ct)
        {
            try
            {
                if (idEstudiante == 0)
                    throw new ApiException("Debes indicar el ID del Estudiante", (int)HttpStatusCode.BadRequest);
                IList<PostulacionDTO> data = await _service.GetUltimoMesByEstudiante(idEstudiante, ct);
                return new ApiResponse("Postulación encontrada exitosamente", data);
            }
            catch (ApiException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("get_postulaciones_por_oferta/{idOferta}")]
        [ProducesResponseType(typeof(PostulacionDTO), StatusCodes.Status200OK)]
        public async Task<ApiResponse> GetPostulacionesPorOferta([FromRoute] int idOferta)
        {
            try
            {
                var data = await _service.GetPostulacionesPorOferta(idOferta);
                return new ApiResponse("Postulaciones obtenidas correctamente", data);
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostulacionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        [Route("get_postulaciones_candidatos_empresa")]
        public async Task<ApiResponse> GetPostulacionesCandidatosEmpresa()
        {
            try
            {
                string emailEmpresa = UserEmailFromJWT(); // Igual que en tu ejemplo
                IList<PostulacionCandidatoDTO> candidatos = await _service.GetPostulacionesCandidatosEmpresa(emailEmpresa);
                return new ApiResponse("Operación exitosa", candidatos);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
        }

        [HttpPut("{idPostulacion}/estado")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> CambiarEstadoPostulacion(
            int idPostulacion,
            [FromBody] string nombreEstado)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreEstado))
                    throw new ApiException("El nombre del estado es obligatorio.", (int)HttpStatusCode.BadRequest);

                // 📩 Recuperar el email de la empresa desde el token JWT
                string emailEmpresa = UserEmailFromJWT();

                // 🧩 Ejecutar lógica de negocio
                await _service.CambiarEstadoPostulacion(idPostulacion, nombreEstado, emailEmpresa);

                return new ApiResponse("Estado actualizado correctamente.");
            }
            catch (ApiException ex)
            {
                // Responde con el mensaje y código definidos en la excepción
                return new ApiResponse(ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al cambiar el estado de la postulación: {ex.Message}");
            }
        }
    }
}
