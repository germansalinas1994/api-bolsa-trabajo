using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BussinessLogic.DTO;
using BussinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using AutoWrapper.Wrappers;


namespace API_Ecommerce.Controllers
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
        // GET: api/values

        //Metodo para traer todas las categorias
        [HttpGet]
        [Route("/getAll_prueba")]
        public async Task<ApiResponse> GetAll_Ejemplo()
        {
            try
            {
                IList<PruebaDTO> pruebas = await _service.GetAll_PRUEBA();
                ApiResponse response = new ApiResponse(new { data = pruebas, cantidad = pruebas.Count() });
                return response;
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                throw new ApiException(ex);
            }


        }

       

    }
}

