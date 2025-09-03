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
    public class PruebaController : GenericController
    {

        //Instancio el service que vamos a usar
        private ServicePrueba _service;

        //Inyecto el service por el constructor
        public PruebaController(ServicePrueba service)
        {
            _service = service;
        }

        //Metodo para traer todas las categorias
        [HttpGet]
        [Route("/getAll_prueba")]
        [ProducesResponseType(typeof(OfertaDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> GetAll_Ejemplo()
        {
            try
            {
                IList<OfertaDTO> pruebas = await _service.GetAll_PRUEBA();
                ApiResponse response = new ApiResponse(new { data = pruebas, cantidad = pruebas.Count });
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
        [Route("/get_by_criteria_prueba")]
        [ProducesResponseType(typeof(OfertaDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> GetByCriteria()
        {
            try
            {
                IList<OfertaDTO> pruebas = await _service.GetByCriteria();
                ApiResponse response = new ApiResponse(new { data = pruebas, cantidad = pruebas.Count });
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


        [HttpPost]
        [Route("/add_prueba")]
        public async Task<ApiResponse> Add_Prueba([FromBody] OfertaDTO nuevaPrueba)
        {
            try
            {
                await _service.Add_PRUEBA(nuevaPrueba);
                return new ApiResponse(new { message = "Prueba agregada exitosamente." });
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

