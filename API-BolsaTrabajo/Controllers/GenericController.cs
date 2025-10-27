using System.IdentityModel.Tokens.Jwt;
using System.Net;
using AutoWrapper.Wrappers;
using BussinessLogic.DTO;
using BussinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Client.Controllers
{
    [ApiController]
    [Authorize]
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
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                var nameSpace = config["Auth0:Namespace"];
                //puedo leer el token
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Buscar primero en claims del usuario validado por el middleware
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == nameSpace + "email")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    throw new ApiException(
                        "Email vacío, no se puede encontrar el usuario",
                        (int)HttpStatusCode.Unauthorized,
                        "No tiene permiso para realizar esta acción"
                    );
                }

                return email;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(
                    "Error al obtener el email del usuario",
                    (int)HttpStatusCode.Unauthorized,
                    ex.Message
                );
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
                string email = UserEmailFromJWT();
                IList<TipoContratoDTO> tiposContratos = await _service.GetAllTiposContratos();

                return new ApiResponse("Operación exitosa", tiposContratos);
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
                return new ApiResponse("Operación exitosa", carreras);
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
        [ProducesResponseType(typeof(ModalidadDTO), StatusCodes.Status200OK)]
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
                return new ApiResponse("Operación exitosa", modalidades);
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
        [ProducesResponseType(typeof(LocalidadDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        [Route("get_localidades")]
        public async Task<ApiResponse> GetAllLocalidades()
        {
            try
            {
                IList<LocalidadDTO> localidades = await _service.GetAllLocalidades();
                return new ApiResponse("Operación exitosa", localidades);
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

        [HttpPost]
        [Route("cargar_usuario")]
        public async Task<ApiResponse> CargarUsuario()
        {
            try
            {
                string email = UserEmailFromJWT();
                UsuarioDTO usuario = await _service.CargarUsuarioDesdeJWT(email);
                return new ApiResponse("Operación exitosa", usuario);
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

                throw new ApiException(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        [HttpGet]
        [Route("get_perfil_empresa_usuario")]
        public async Task<ApiResponse> GetPerfilEmpresaUsuario()
        {
            try
            {
                string email = UserEmailFromJWT();
                int idPerfilEmpresa = await _service.GetPerfilEmpresaUsuario(email);
                return new ApiResponse("Operación exitosa", new { idPerfilEmpresa });
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        public async Task<int> GetIdUsuarioFromJWT()
        {
            try
            {
                string email = UserEmailFromJWT();
                int idUsuario = await _service.GetIdUsuarioFromEmail(email);
                return idUsuario;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        public async Task<int> GetIdPerfilFromJWT()
        {
            try
            {
                string email = UserEmailFromJWT();
                int idPerfil = await _service.GetIdPerfilFromEmail(email);
                return idPerfil;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }
    }
}