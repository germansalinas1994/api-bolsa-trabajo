using System.IdentityModel.Tokens.Jwt;
using System.Net;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace API_Client.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenericController : Controller
    {
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
            catch(ApiException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new ApiException("Error al obtener el email del usuario", (int)HttpStatusCode.Unauthorized, ex.Message);
            }


        }
    }
}