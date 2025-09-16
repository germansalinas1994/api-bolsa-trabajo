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
                List<PostulacionDTO> postulaciones = await _service.GetPostulaciones();
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
                if (data.IdPerfilCandidato == null || data.IdOferta == null)
                {
                    throw new ApiException("IdPerfilCandidato e IdOferta son obligatorios", (int)HttpStatusCode.BadRequest);
                }

                await _service.CrearPostulacion(data);

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
    }
}
