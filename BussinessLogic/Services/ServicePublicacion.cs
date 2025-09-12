using System;
using System.Security.Cryptography.X509Certificates;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using AutoWrapper.Wrappers;
using System.Net;

namespace BussinessLogic.Services
{
    public class ServicePublicacion : GenericService
    {
        public ServicePublicacion(IUnitOfWork unitOfWork) : base(unitOfWork) { }


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
                throw ex;
            }
        }

        public async Task<Oferta> GetPublicacionEntidadById(int id)
        {
            try
            {
                Oferta oferta = await _unitOfWork.GenericRepository<Oferta>().GetById(id);
                if (oferta == null)
                {
                    throw new ApiException("No se encontró la publicación con el ID proporcionado.", (int)HttpStatusCode.NotFound);
                }

                return oferta;
            }
            catch (ApiException)
            {
                //lanzo la excepcion que se captura en el controller
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IList<OfertaDTO>> GetPublicaciones(SearchPublicacionesDTO filtro)
        {
            try
            {
                var search = (await _unitOfWork.GenericRepository<Oferta>().Search()).Where(o => o.FechaBaja == null);
                if (filtro.Input != null)
                {
                    search = search.Where(o => o.Titulo.Contains(filtro.Input) || o.Descripcion.Contains(filtro.Input));
                }
                if (filtro.Modalidades != null && filtro.Modalidades.Count > 0)
                {
                    IList<int> idsModalidades = (await _unitOfWork.GenericRepository<Modalidad>().GetByCriteria(m => filtro.Modalidades.Contains(m.Codigo))).Select(m => m.Id).ToList();
                    search = search.Where(o => idsModalidades.Contains(o.IdModalidad));
                }
                if (filtro.TiposContrato != null && filtro.TiposContrato.Count > 0)
                {
                    IList<int> idsTiposContrato = (await _unitOfWork.GenericRepository<TipoContrato>().GetByCriteria(m => filtro.TiposContrato.Contains(m.Codigo))).Select(m => m.Id).ToList();
                    search = search.Where(o => idsTiposContrato.Contains(o.IdTipoContrato));
                }

                List<Oferta> oferta = search
                    .Include(pe => pe.PerfilEmpresa)
                        .ThenInclude(u => u.Usuario)
                    .Include(m => m.Modalidad)
                    .Include(tc => tc.TipoContrato)
                    .Include(l => l.Localidad)
                        .ThenInclude(p => p.Provincia)
                            .ThenInclude(p => p.Pais)
                    .ToList();

                return oferta.Adapt<List<OfertaDTO>>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<IList<OfertaDTO>> GetRecientes(int limit)
        {
            try
            {
                List<Oferta> o = (await _unitOfWork.GenericRepository<Oferta>()//aca digo que voy a la tabla oferta
                .GetAllIncludingSpecificRelations(q => q.Include(l => l.Localidad).ThenInclude(p => p.Provincia)
                .Include(tc => tc.TipoContrato)
                .Include(m=>m.Modalidad)
                .Include(e=>e.PerfilEmpresa)

            )).Where(f=>f.FechaBaja == null).OrderByDescending(f=>f.FechaAlta).ToList();
                // int cantidad = o.Count;
                return o.Adapt<List<OfertaDTO>>(); //mapeo a DTO y retorno
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw ex; }
        }
    }
}