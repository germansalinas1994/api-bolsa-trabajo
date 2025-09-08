using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessLogic.DTO;
using BussinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;
using DataAccess.Entities;


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
        [HttpGet]
        [ProducesResponseType(typeof(OfertaDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        [Route("get_publicaciones_empleo")]
        public async Task<ApiResponse> GetAllPublicaciones()
        {
            try
            {
                IList<OfertaDTO> pruebas = await _service.GetAllPublicaciones();
                ApiResponse response = new ApiResponse(new { data = pruebas });
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

    
        [HttpGet("recientes")]
        public async Task<ApiResponse> GetRecientes([FromQuery] int limit = 3, CancellationToken ct = default)
        {
            var list = await _service.GetRecientes(limit, ct);
            return new ApiResponse(list);
        }

       

    }
}

