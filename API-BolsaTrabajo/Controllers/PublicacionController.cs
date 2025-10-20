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
    public class PublicacionController : GenericController
    {

        private readonly ServicePublicacion _service;

        public PublicacionController(ServicePublicacion service) : base(service)
        {
            _service = service; // ahora sí lo seteás correctamente
        }

        //Inyecto el service por el constructor

        //Metodo para traer todas las categorias
        [HttpPost]
        [ProducesResponseType(typeof(OfertaDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        [Route("get_publicaciones_empleo")]
        public async Task<ApiResponse> GetPublicaciones([FromBody] SearchPublicacionesDTO filtro)
        {
            try
            {
                // IList<OfertaDTO> ofertas = await _service.GetAllPublicaciones();
                IList<OfertaDTO> ofertas = await _service.GetPublicaciones(filtro);
                return new ApiResponse("Operación exitosa", ofertas);
            }
            catch (ApiException)
            {
                //lanzo la excepcion que se captura en el service
                throw;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                // throw new ApiException("Mensaje de error que quiero enviar", (int)HttpStatusCode.Unauthorized, ex.Message);

                throw new ApiException(ex);
            }


        }


        [HttpGet("get_publicaciones_recientes")]
        public async Task<ApiResponse> GetPublicacionesRecientes([FromQuery] int limit = 3)
        {
            try
            { //aca puedo agregar validaciones, por ejemplo que el limit no sea negativo o cero
                if (limit <= 0) throw new ApiException("El parámetro 'limit' debe ser mayor a 0.", (int)HttpStatusCode.BadRequest); // con esto me devuelve un 400
                OfertaRecienteDTO ofertas = await _service.GetRecientes(limit);
                return new ApiResponse("Ofertas encontradas", ofertas);
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw ex; }
        }

        //    [HttpGet("get_publicaciones_por-carrera/{idCarrera}")]
        //      public async Task<IActionResult> GetPorCarrera([FromRoute] int idCarrera, CancellationToken ct = default)
        //     {
        //             try
        //             {
        //                 if (idCarrera <= 0) return BadRequest(new ApiResponse("El parámetro 'idCarrera' debe ser mayor a 0."));
        //                 IList<OfertaDTO> data = await _service.GetPorCarreraAsync(idCarrera, ct);
        //                 return Ok(data);
        //             }
        //             catch (ApiException) { throw; }
        //             catch (System.Exception ex) { throw new ApiException(ex); }
        //     }

        /// <summary>
        /// Devuelve todas las publicaciones de una empresa según su email.
        /// </summary>
        [HttpGet("get_publicaciones_empresa/{email}")]
        [ProducesResponseType(typeof(IEnumerable<OfertaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> GetPublicacionesEmpresa(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ApiException("El parámetro 'email' es obligatorio.", (int)HttpStatusCode.BadRequest);

                IList<OfertaDTO> publicaciones = await _service.GetPublicacionesEmpresa(email);

                if (publicaciones == null || publicaciones.Count == 0)
                    throw new ApiException("No se encontraron publicaciones para la empresa indicada.", (int)HttpStatusCode.NotFound);

                return new ApiResponse("Publicaciones de la empresa obtenidas correctamente", publicaciones);
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }

        [HttpGet("get_postulaciones_empresa")]
        [ProducesResponseType(typeof(PostulacionDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> GetPostulacionesEmpresa([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    throw new ApiException("Debe especificar un email válido.", (int)HttpStatusCode.BadRequest);

                var postulaciones = await _service.GetPostulacionesEmpresa(email);
                return new ApiResponse("Postulaciones recuperadas correctamente", postulaciones);
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }
    }
}

