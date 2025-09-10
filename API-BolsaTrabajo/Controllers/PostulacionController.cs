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
    public class PostulacionController : GenericController
    {

        private readonly ServicePostulacion _service;

        public PostulacionController(ServicePostulacion service) : base(service)
        {
            _service = service; // ahora sí lo seteás correctamente
        }
        [HttpGet]
        [Route("get_postulaciones")]
        public async Task<ApiResponse> GetPostulaciones()
        {
            try
            {
                List<PostulacionDTO> postulaciones = await _service.GetPostulaciones();
                return new ApiResponse(postulaciones);
            }
            catch (ApiException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }
        }

        [HttpPost]
        [Route("postularse_oferta")]
        public async Task<ApiResponse> PostularseOferta([FromBody] PostulacionDTO data)
        {
            try
            {
                if (data.IdPerfilCandidato == null || data.IdOferta == null)
                {
                    throw new ApiException("IdPerfilCandidato e IdOferta son obligatorios", (int)HttpStatusCode.BadRequest);
                }

                await _service.CrearPostulacion(data);

                return new ApiResponse("Postulación creada exitosamente");
            }
            catch (ApiException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex);
            }



        }
    }
}

