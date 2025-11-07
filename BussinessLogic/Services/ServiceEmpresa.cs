using System;
using System.Linq;
using System.Threading.Tasks;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using AutoWrapper.Wrappers;

namespace BussinessLogic.Services
{
    public class ServiceEmpresa : GenericService
    {
        public ServiceEmpresa(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<PerfilEmpresaDTO> GetPerfilById(int perfilId)
        {
            try
            {
                // Buscar el perfil empresa por ID usando GetByCriteria para filtrar por ID y FechaBaja
                var perfilesEmpresas = await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(p => p.Id == perfilId && p.FechaBaja == null);
                var perfilEmpresa = perfilesEmpresas.FirstOrDefault();

                if (perfilEmpresa == null)
                {
                    throw new ApiException("Perfil de empresa no encontrado", 404);
                }

                // Obtener usuario relacionado usando GetById
                var usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(perfilEmpresa.IdUsuario);

                // Obtener estado de validación si existe
                EstadoValidacion? estadoValidacion = null;
                if (perfilEmpresa.IdEstadoValidacion.HasValue)
                {
                    estadoValidacion = await _unitOfWork.GenericRepository<EstadoValidacion>().GetById(perfilEmpresa.IdEstadoValidacion.Value);
                }

                // Obtener rol relacionado a través del usuario
                Rol? rol = null;
                if (usuario != null)
                {
                    rol = await _unitOfWork.GenericRepository<Rol>().GetById(usuario.IdRol);
                }

                // Obtener ofertas publicadas por la empresa
                var ofertas = await _unitOfWork.GenericRepository<Oferta>().GetByCriteria(o => o.IdPerfilEmpresa == perfilId && o.FechaBaja == null);

                // Mapear campos básicos de PerfilEmpresa
                var perfilDTO = new PerfilEmpresaDTO
                {
                    Id = perfilEmpresa.Id,
                    IdUsuario = perfilEmpresa.IdUsuario,
                    Descripcion = perfilEmpresa.Descripcion,
                    RazonSocial = perfilEmpresa.RazonSocial,
                    Cuit = perfilEmpresa.Cuit,
                    IdEstadoValidacion = perfilEmpresa.IdEstadoValidacion,
                    FechaAlta = perfilEmpresa.FechaAlta,
                    FechaModificacion = perfilEmpresa.FechaModificacion,
                    FechaBaja = perfilEmpresa.FechaBaja
                };

                // Agregar información del usuario
                if (usuario != null)
                {
                    perfilDTO.Nombre = usuario.Nombre;
                    perfilDTO.Email = usuario.Email;
                    perfilDTO.UsuarioActivo = usuario.Activo;
                    perfilDTO.IdRol = usuario.IdRol;
                    perfilDTO.FotoPerfil = usuario.FotoPerfil;
                }

                // Agregar información del rol
                if (rol != null)
                {
                    perfilDTO.RolNombre = rol.Nombre;
                    perfilDTO.RolCodigo = rol.Codigo;
                }

                // Agregar información del estado de validación
                if (estadoValidacion != null)
                {
                    perfilDTO.EstadoValidacionNombre = estadoValidacion.Nombre;
                    perfilDTO.EstadoValidacionCodigo = estadoValidacion.Codigo;
                }

                // Mapear ofertas a DTOs
                var ofertasDTO = new List<OfertaDTO>();
                foreach (var oferta in ofertas)
                {
                    var modalidad = await _unitOfWork.GenericRepository<Modalidad>().GetById(oferta.IdModalidad);
                    var tipoContrato = await _unitOfWork.GenericRepository<TipoContrato>().GetById(oferta.IdTipoContrato);
                    
                    Localidad? localidad = null;
                    if (oferta.IdLocalidad.HasValue)
                    {
                        localidad = await _unitOfWork.GenericRepository<Localidad>().GetById(oferta.IdLocalidad.Value);
                    }

                    ofertasDTO.Add(new OfertaDTO
                    {
                        Id = oferta.Id,
                        Titulo = oferta.Titulo,
                        Descripcion = oferta.Descripcion,
                        Modalidad = modalidad?.Nombre ?? "",
                        TipoContrato = tipoContrato?.Nombre ?? "",
                        FechaInicio = oferta.FechaInicio.ToString("yyyy-MM-dd"),
                        FechaFin = oferta.FechaFin?.ToString("yyyy-MM-dd") ?? "",
                        NombreLocalidad = localidad?.Nombre ?? "",
                        NombreEmpresa = perfilEmpresa.RazonSocial
                    });
                }

                perfilDTO.Ofertas = ofertasDTO;

                // Calcular porcentaje de perfil completado
                perfilDTO.PorcentajePerfil = CalcularPorcentajePerfil(perfilEmpresa, usuario, ofertas.Count());

                // Campos que no están en el diagrama - los dejamos null por ahora
                perfilDTO.Telefono = null;
                perfilDTO.Localidad = null;

                return perfilDTO;
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

        public async Task<PerfilEmpresaDTO> GetPerfilByUsuarioId(int usuarioId)
        {
            try
            {
                // Buscar el perfil empresa usando GetByCriteria para filtrar por IdUsuario y FechaBaja
                var perfilesEmpresas = await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(p => p.IdUsuario == usuarioId && p.FechaBaja == null);
                var perfilEmpresa = perfilesEmpresas.FirstOrDefault();

                if (perfilEmpresa == null)
                {
                    throw new ApiException("Perfil de empresa no encontrado", 404);
                }

                // Reutilizar la lógica de GetPerfilById
                return await GetPerfilById(perfilEmpresa.Id);
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

        public async Task<PerfilEmpresaDTO> UpdatePerfil(PerfilEmpresaDTO perfilDTO)
        {
            try
            {
                // Buscar el perfil existente usando GetByCriteria para filtrar por Id y FechaBaja
                var perfilesEmpresas = await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(p => p.Id == perfilDTO.Id && p.FechaBaja == null);
                var perfilExistente = perfilesEmpresas.FirstOrDefault();

                if (perfilExistente == null)
                {
                    throw new ApiException("Perfil de empresa no encontrado", 404);
                }

                // Actualizar campos
                perfilExistente.Descripcion = perfilDTO.Descripcion ?? perfilExistente.Descripcion;
                perfilExistente.RazonSocial = perfilDTO.RazonSocial ?? perfilExistente.RazonSocial;
                perfilExistente.Cuit = perfilDTO.Cuit ?? perfilExistente.Cuit;
                perfilExistente.IdEstadoValidacion = perfilDTO.IdEstadoValidacion ?? perfilExistente.IdEstadoValidacion;
                perfilExistente.FechaModificacion = DateTime.Now;

                // Si se proporcionó una foto de perfil, actualizar en el usuario
                if (perfilDTO.FotoPerfil != null)
                {
                    var usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(perfilExistente.IdUsuario);
                    if (usuario != null)
                    {
                        usuario.FotoPerfil = perfilDTO.FotoPerfil;
                        await _unitOfWork.GenericRepository<Usuario>().Update(usuario);
                    }
                }

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.GenericRepository<PerfilEmpresa>().Update(perfilExistente);
                await _unitOfWork.CommitAsync();

                // Retornar el perfil actualizado
                return await GetPerfilByUsuarioId(perfilExistente.IdUsuario);
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

        private int CalcularPorcentajePerfil(PerfilEmpresa perfil, Usuario? usuario, int cantidadOfertas)
        {
            int porcentaje = 0;

            // Campos básicos (20 puntos cada uno)
            if (!string.IsNullOrEmpty(perfil.Descripcion)) porcentaje += 25;
            if (!string.IsNullOrEmpty(perfil.RazonSocial)) porcentaje += 20;
            if (!string.IsNullOrEmpty(perfil.Cuit)) porcentaje += 15;
            if (perfil.IdEstadoValidacion.HasValue) porcentaje += 10;

            // Campos del usuario
            if (usuario != null)
            {
                if (!string.IsNullOrEmpty(usuario.Nombre)) porcentaje += 10;
                if (!string.IsNullOrEmpty(usuario.Email)) porcentaje += 10;
            }

            // Ofertas publicadas
            if (cantidadOfertas > 0) porcentaje += 10;

            return Math.Min(porcentaje, 100); // Máximo 100%
        }
    }
}

