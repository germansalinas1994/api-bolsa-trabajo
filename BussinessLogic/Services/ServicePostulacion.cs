using System;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using AutoWrapper.Wrappers;
using System.Linq;

namespace BussinessLogic.Services
{
    public class ServicePostulacion : GenericService
    {
        public ServicePostulacion(IUnitOfWork unitOfWork) : base(unitOfWork) { }

         // GET ALL por idPerfilCandidato
        public async Task<IList<PostulacionDTO>> GetAllByEstudiante(int idPerfilCandidato, CancellationToken ct = default)
        {
            try
            {
                // 1) Postulaciones (sin historial) + Oferta/Empresa
                var postulaciones = await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.IdPerfilCandidato == idPerfilCandidato && p.FechaBaja == null,
                        include: q => q
                            .AsNoTracking()
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.PerfilEmpresa)
                                    .ThenInclude(pe => pe.Usuario)
                    );

                var postIds = postulaciones.Select(p => p.Id).Distinct().ToList();
                if (postIds.Count == 0) return new List<PostulacionDTO>();

                // 2) Historiales + Estado para esas postulaciones (una query)
                var historiales = await _unitOfWork.GenericRepository<PostulacionHistorial>()
                    .GetByCriteriaIncludingSpecificRelations(
                        h => postIds.Contains(h.IdPostulacion),
                        include: q => q
                            .AsNoTracking()
                            .Include(h => h.EstadoPostulacion)
                    );

                // 3) Diccionario { IdPostulacion -> EstadoNombre (último activo o más reciente) }
                var estados = historiales
                    .GroupBy(h => h.IdPostulacion)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(h => h.FechaBaja == null)
                              .OrderByDescending(h => h.FechaAlta)
                              .Select(h => h.EstadoPostulacion.Nombre)
                              .FirstOrDefault()
                           ?? g.OrderByDescending(h => h.FechaAlta)
                               .Select(h => h.EstadoPostulacion.Nombre)
                               .FirstOrDefault()
                           ?? "En revisión"
                    );

                // 4) Adapt con MapContext (sin función manual)
                var ctx = new MapContext();
                ctx.Parameters["Estados"] = estados;

                var result = postulaciones
                    .OrderByDescending(p => p.FechaAlta)
                    .ToList()
                    .BuildAdapter()                           // <-- crea el adaptador para esta conversión
                    .AddParameters("Estados", estados)        // <-- pasa parámetros al mapeo (MapContext.Parameters)
                    .AdaptToType<List<PostulacionDTO>>();     // <-- destino

                return result;
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }

        // GET último mes por idPerfilCandidato
        public async Task<IList<PostulacionDTO>> GetUltimoMesByEstudiante(int idPerfilCandidato, CancellationToken ct = default)
        {
            try
            {
                var desde = DateTime.UtcNow.AddDays(-30);

                var postulaciones = await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.IdPerfilCandidato == idPerfilCandidato &&
                             p.FechaBaja == null &&
                             p.FechaAlta >= desde,
                        include: q => q
                            .AsNoTracking()
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.PerfilEmpresa)
                                    .ThenInclude(pe => pe.Usuario)
                    );

                var postIds = postulaciones.Select(p => p.Id).Distinct().ToList();
                if (postIds.Count == 0) return new List<PostulacionDTO>();

                var historiales = await _unitOfWork.GenericRepository<PostulacionHistorial>()
                    .GetByCriteriaIncludingSpecificRelations(
                        h => postIds.Contains(h.IdPostulacion),
                        include: q => q
                            .AsNoTracking()
                            .Include(h => h.EstadoPostulacion)
                    );

                var estados = historiales
                    .GroupBy(h => h.IdPostulacion)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(h => h.FechaBaja == null)
                              .OrderByDescending(h => h.FechaAlta)
                              .Select(h => h.EstadoPostulacion.Nombre)
                              .FirstOrDefault()
                           ?? g.OrderByDescending(h => h.FechaAlta)
                               .Select(h => h.EstadoPostulacion.Nombre)
                               .FirstOrDefault()
                           ?? "En revisión"
                    );

                var ctx = new MapContext();
                ctx.Parameters["Estados"] = estados;

                var result = postulaciones
                .OrderByDescending(p => p.FechaAlta)
                .ToList()
                .BuildAdapter()                           // <-- crea el adaptador para esta conversión
                .AddParameters("Estados", estados)        // <-- pasa parámetros al mapeo (MapContext.Parameters)
                .AdaptToType<List<PostulacionDTO>>();     // <-- destino

            return result;
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }
    }

}
