using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using BussinessLogic.DTO;
using BussinessLogic.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BolsaTrabajo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostulacionesController : ControllerBase
    {
        private readonly ServicePostulacion _service;

        public PostulacionesController(ServicePostulacion service)
        {
            _service = service;
        }

        /// <summary>
        /// Devuelve todas las postulaciones del estudiante (por IdPerfilCandidato).
        /// </summary>
        /// <param name="idEstudiante">Id del perfil candidato</param>
        [HttpGet("{idEstudiante:int}")]
        [ProducesResponseType(typeof(IEnumerable<PostulacionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> GetAll(int idEstudiante, CancellationToken ct)
        {
            var data = await _service.GetAllByEstudiante(idEstudiante, ct);
            return new ApiResponse(new { data });
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
            var data = await _service.GetUltimoMesByEstudiante(idEstudiante, ct);
            return new ApiResponse(new { data });
        }
    }
}
