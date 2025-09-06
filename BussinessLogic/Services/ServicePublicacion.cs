using System;
using System.Security.Cryptography.X509Certificates;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using AutoWrapper.Wrappers;

namespace BussinessLogic.Services
{
    public class ServicePublicacion
    {
        //Instancio el UnitOfWork que vamos a usar
        private readonly IUnitOfWork _unitOfWork;

        //Inyecto el UnitOfWork por el constructor, esto se hace para que se cree un nuevo contexto por cada vez que se llame a la clase
        public ServicePublicacion(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

       
        public async Task<IList<OfertaDTO>> GetAllPublicaciones()
        {
            try
            {
                List<Oferta> oferta = (await _unitOfWork.GenericRepository<Oferta>()
                    .GetAllIncludingSpecificRelations(
                        q => q.Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                        .Include(m => m.Modalidad)
                        .Include(tc => tc.TipoContrato)
                        .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                        .ThenInclude(p => p.Pais)
                )

                    ).ToList();



                return oferta.Adapt<List<OfertaDTO>>();
            }
            catch (ApiException)
            {
                //lanzo la excepcion que se captura en el controller
                throw;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones en caso de error
                throw new ApiException(ex);
            }
        }

    

    }
}