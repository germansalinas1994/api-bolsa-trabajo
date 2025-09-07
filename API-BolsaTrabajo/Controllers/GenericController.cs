using System.IdentityModel.Tokens.Jwt;
using System.Net;
using AutoWrapper.Wrappers;
using BussinessLogic.DTO;
using BussinessLogic.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_Client.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenericController : Controller
    { //Instancio el service que vamos a usar
        private GenericService _service;

        //Inyecto el service por el constructor
        public GenericController(GenericService service)
        {
            _service = service;
        }
        protected string UserEmailFromJWT()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                string email = jwtToken.Claims.First(claim => claim.Type == "email").Value;

                if (string.IsNullOrEmpty(email))
                {
                    throw new ApiException("Email vacío, no se puede encontrar el usuario", (int)HttpStatusCode.Unauthorized, "No tiene permiso para realizar esta acción");
                }

                return email;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException("Error al obtener el email del usuario", (int)HttpStatusCode.Unauthorized, ex.Message);
            }


        }



        [HttpGet]
        [ProducesResponseType(typeof(TipoContratoDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        [Route("get_tipos_contratos")]
        public async Task<ApiResponse> GetAllTiposContratos()
        {
            try
            {
                IList<TipoContratoDTO> tiposContratos = await _service.GetAllTiposContratos();
                ApiResponse response = new ApiResponse(new { data = tiposContratos });
                return response;
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

        [HttpGet]
        [ProducesResponseType(typeof(TipoContratoDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        [Route("get_carreras")]
        public async Task<ApiResponse> GetAllCarreras()
        {
            try
            {
                IList<CarreraDTO> carreras = await _service.GetAllCarreras();
                ApiResponse response = new ApiResponse(new { data = carreras });
                return response;
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


        [HttpGet]
        [ProducesResponseType(typeof(TipoContratoDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        [Route("get_modalidades")]
        public async Task<ApiResponse> GetAllModalidades()
        {
            try
            {
                IList<ModalidadDTO> modalidades = await _service.GetAllModalidades();
                ApiResponse response = new ApiResponse(new { data = modalidades });
                return response;
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
    }
}