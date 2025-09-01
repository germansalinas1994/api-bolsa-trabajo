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
    public class ServicePrueba
    {
        //Instancio el UnitOfWork que vamos a usar
        private readonly IUnitOfWork _unitOfWork;

        //Inyecto el UnitOfWork por el constructor, esto se hace para que se cree un nuevo contexto por cada vez que se llame a la clase
        public ServicePrueba(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Add_PRUEBA(OfertaDTO nuevaPrueba)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                //Mapeo la nueva categoria a Categoria
                Oferta newPrueba = new Oferta
                {

                };

                //Agrego la nueva categoria al repositorio
                await _unitOfWork.GenericRepository<Oferta>().Insert(newPrueba);

                await _unitOfWork.CommitAsync();
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

        //Metodo para traer todas las categorias
        public async Task<IList<OfertaDTO>> GetAll_PRUEBA()
        {
            try
            {
                List<OfertaDTO> ofertas = new List<OfertaDTO>();
                //Traigo todas las categorias
                // var ofertas = await _unitOfWork.GenericRepository<Oferta>().GetAllIncludingSpecificRelations(q => q.Include(l => l.Localidad).ThenInclude(l => l.Provincia).ThenInclude(l => l.Pais));
                List<OfertaCategoria> oh = (await _unitOfWork.GenericRepository<OfertaCategoria>().GetAllIncludingAllRelations()).ToList();
                
                // List<OfertaDTO> ofertas = (await _unitOfWork.GenericRepository<Oferta>().GetAll()).Adapt<List<OfertaDTO>>();
                return ofertas;
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