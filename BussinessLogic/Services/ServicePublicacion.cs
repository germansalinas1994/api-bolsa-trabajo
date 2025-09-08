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
                throw new ApiException(ex);
            }
        }

        public async Task<IList<OfertaDTO>> GetRecientes(int limit, CancellationToken ct = default)
        {
            try
            {
                var lista = await _unitOfWork.GenericRepository<Oferta>()
                    .GetByCriteriaIncludingSpecificRelations(
                        o => o.FechaBaja == null,
                        include: q => q
                            .Include(o => o.PerfilEmpresa).ThenInclude(pe => pe.Usuario)
                            .Include(o => o.Modalidad)
                            .Include(o => o.TipoContrato)
                            .Include(o => o.Localidad).ThenInclude(l => l.Provincia).ThenInclude(p => p.Pais)
                    );

                var recientes = lista
                    .OrderByDescending(o => o.FechaAlta)
                    .Take(limit)
                    .ToList();

                return recientes.Adapt<List<OfertaDTO>>();
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }

    }
}