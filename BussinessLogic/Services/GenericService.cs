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
    public class GenericService
    {
        //Instancio el UnitOfWork que vamos a usar
        protected readonly IUnitOfWork _unitOfWork;

        //Inyecto el UnitOfWork por el constructor, esto se hace para que se cree un nuevo contexto por cada vez que se llame a la clase
        public GenericService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<CarreraDTO>> GetAllCarreras()
        {
            try
            {
                List<Carrera> carreras = (await _unitOfWork.GenericRepository<Carrera>()
                    .GetAll()

                    ).ToList();

                return carreras.Adapt<List<CarreraDTO>>();
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

        public async Task<IList<ModalidadDTO>> GetAllModalidades()
        {
            try
            {
                List<Modalidad> modalidades = (await _unitOfWork.GenericRepository<Modalidad>()
                    .GetAll()

                    ).ToList();

                return modalidades.Adapt<List<ModalidadDTO>>();
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

        public async Task<IList<TipoContratoDTO>> GetAllTiposContratos()
        {
            try
            {
                List<TipoContrato> tiposContratos = (await _unitOfWork.GenericRepository<TipoContrato>()
                    .GetAll()

                    ).ToList();

                return tiposContratos.Adapt<List<TipoContratoDTO>>();
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