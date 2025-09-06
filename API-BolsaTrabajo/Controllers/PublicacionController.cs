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

        //Instancio el service que vamos a usar
        private ServicePublicacion _service;

        //Inyecto el service por el constructor
        public PublicacionController(ServicePublicacion service)
        {
            _service = service;
        }

        //Metodo para traer todas las categorias
        [HttpGet]
        [ProducesResponseType(typeof(OfertaDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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

    


       

    }
}

