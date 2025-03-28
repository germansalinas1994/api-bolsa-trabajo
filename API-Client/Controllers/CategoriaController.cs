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
    public class CategoriaController : GenericController
    {

        //Instancio el service que vamos a usar
        private ServiceCategoria _service;

        //Inyecto el service por el constructor
        public CategoriaController(ServiceCategoria service)
        {
            _service = service;
        }
        // GET: api/values

        //Metodo para traer todas las categorias
        [HttpGet]
        [Route("/categorias")]
        public async Task<ApiResponse> GetCategorias()
        {
            try
            {
                IList<CategoriaDTO> categorias = await _service.GetAllCategorias();
                ApiResponse response = new ApiResponse(new { data = categorias, cantidadCategorias = categorias.Count() });
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

