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
                Oferta nueva = new Oferta
                {
                    Titulo = "Oferta PRUEBA (insert genérico)",
                    Descripcion = "Creada desde GetAll_PRUEBA para testear Insert.",
                    IdPerfilEmpresa = 1,   // ⚠️ Debe existir
                    IdModalidad = 1,   // ⚠️ Debe existir
                    IdTipoContrato = 1,   // ⚠️ Debe existir
                    IdLocalidad = 1,   // opcional; si no existe, ponelo en null o a un Id válido
                    FechaInicio = DateTime.UtcNow,
                    FechaFin = null,
                    FechaAlta = DateTime.UtcNow,
                    FechaModificacion = DateTime.UtcNow,
                    FechaBaja = null
                };

                //Agrego la nueva categoria al repositorio
                await _unitOfWork.GenericRepository<Oferta>().Insert(nueva);

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
                //Traigo todas las categorias
                // var ofertas = await _unitOfWork.GenericRepository<Oferta>().GetAllIncludingSpecificRelations(q => q.Include(l => l.Localidad).ThenInclude(l => l.Provincia).ThenInclude(l => l.Pais));
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

        public async Task<IList<OfertaDTO>> GetByIdIncludingAllRelations()
        {
            try
            {
                List<OfertaDTO> ofertas = new List<OfertaDTO>();

                Oferta ofertaById = await _unitOfWork.GenericRepository<Oferta>().GetByIdIncludingRelations(2);


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


        public async Task<IList<OfertaDTO>> GetByIdIncludingSpecificsRelations()
        {
            try
            {
                List<OfertaDTO> ofertas = new List<OfertaDTO>();


                Oferta ofertaConRelaciones = await _unitOfWork.GenericRepository<Oferta>().GetByIdIncludingSpecificRelations(2, q => q.Include(t => t.Modalidad));


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

        public async Task<IList<OfertaDTO>> GetByCriteria()
        {
            try
            {

                List<Oferta> ofertasByCriteria = (await _unitOfWork.GenericRepository<Oferta>()
                .GetByCriteriaIncludingSpecificRelations(
                    o => o.IdTipoContrato == 1,
                    query => query.Include(l => l.Localidad).ThenInclude(p => p.Provincia).Include(t => t.TipoContrato))
                    ).ToList();



                return ofertasByCriteria.Adapt<List<OfertaDTO>>();
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