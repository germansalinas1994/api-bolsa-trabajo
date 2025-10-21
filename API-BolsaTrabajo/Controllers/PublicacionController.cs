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
                return  new ApiResponse("Operación exitosa", ofertas);
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
        [Route("get_all_ofertas")]
        public async Task<ApiResponse> GetAllOfertas()
        {
            try
            {
                IList<OfertaDTO> ofertas = await _service.GetAllPublicaciones();
                return new ApiResponse("Operación exitosa", ofertas);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
        }

        [HttpGet]
        [Route("get_ofertas_by_empresa")]
        public async Task<ApiResponse> GetOfertasByEmpresa()
        {
            try
            {
                string email = UserEmailFromJWT();
                IList<OfertaDTO> ofertas = await _service.GetOfertasByEmpresa(email);
                return new ApiResponse("Operación exitosa", ofertas);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
        }

        [HttpPost]
        [Route("crear_oferta")]
        public async Task<ApiResponse> CrearOferta([FromBody] CrearOfertaDTO data)
        {
            try
            {
                if (data.Titulo == null || data.Descripcion == null)
                {
                    throw new ApiException("Titulo y Descripcion son obligatorios", (int)HttpStatusCode.BadRequest);
                }
                string email = UserEmailFromJWT();
                OfertaDTO oferta = await _service.CrearOferta(data, email);
                return new ApiResponse("Oferta creada exitosamente", oferta);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
        }

        [HttpPut]
        [Route("actualizar_oferta/{id}")]
        public async Task<ApiResponse> ActualizarOferta(int id, [FromBody] CrearOfertaDTO data)
        {
            try
            {
                OfertaDTO oferta = await _service.ActualizarOferta(id, data);
                return new ApiResponse("Oferta actualizada exitosamente", oferta);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
        }

        [HttpDelete]
        [Route("eliminar_oferta/{id}")]
        public async Task<ApiResponse> EliminarOferta(int id)
        {
            try
            {
                await _service.EliminarOferta(id);
                return new ApiResponse("Oferta eliminada exitosamente");
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
        }

    }
}

