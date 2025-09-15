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
    public class ServiceCandidato : GenericService
    {
        public ServiceCandidato(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<PerfilCandidatoDTO> GetPerfilById(int perfilId)
        {
            try
            {
                // Buscar el perfil candidato por ID
                var perfilesCandidatos = await _unitOfWork.GenericRepository<PerfilCandidato>().GetAll();
                var perfilCandidato = perfilesCandidatos.FirstOrDefault(p => p.Id == perfilId && p.FechaBaja == null);

                if (perfilCandidato == null)
                {
                    throw new ApiException("Perfil de candidato no encontrado", 404);
                }

                // Obtener usuario relacionado
                var usuarios = await _unitOfWork.GenericRepository<Usuario>().GetAll();
                var usuario = usuarios.FirstOrDefault(u => u.Id == perfilCandidato.IdUsuario);

                // Obtener género relacionado si existe
                Genero? genero = null;
                if (perfilCandidato.IdGenero.HasValue)
                {
                    var generos = await _unitOfWork.GenericRepository<Genero>().GetAll();
                    genero = generos.FirstOrDefault(g => g.Id == perfilCandidato.IdGenero.Value);
                }

                // Obtener carrera relacionada si existe
                Carrera? carrera = null;
                if (perfilCandidato.IdCarrera.HasValue)
                {
                    var carreras = await _unitOfWork.GenericRepository<Carrera>().GetAll();
                    carrera = carreras.FirstOrDefault(c => c.Id == perfilCandidato.IdCarrera.Value);
                }

                // Obtener rol relacionado a través del usuario
                Rol? rol = null;
                if (usuario != null)
                {
                    var roles = await _unitOfWork.GenericRepository<Rol>().GetAll();
                    rol = roles.FirstOrDefault(r => r.Id == usuario.IdRol);
                }

                // Mapear campos básicos de PerfilCandidato
                var perfilDTO = new PerfilCandidatoDTO
                {
                    Id = perfilCandidato.Id,
                    Descripcion = perfilCandidato.Descripcion,
                    IdUsuario = perfilCandidato.IdUsuario,
                    IdGenero = perfilCandidato.IdGenero,
                    Legajo = perfilCandidato.Legajo,
                    AnioEgreso = perfilCandidato.AnioEgreso,
                    FechaAlta = perfilCandidato.FechaAlta,
                    FechaModificacion = perfilCandidato.FechaModificacion,
                    FechaBaja = perfilCandidato.FechaBaja
                };
                
                // Agregar información del usuario (según diagrama)
                if (usuario != null)
                {
                    perfilDTO.Nombre = usuario.Nombre;
                    perfilDTO.Email = usuario.Email;
                    perfilDTO.UsuarioActivo = usuario.Activo;
                    perfilDTO.IdRol = usuario.IdRol;
                }
                
                // Agregar información del género (según diagrama)
                if (genero != null)
                {
                    perfilDTO.GeneroNombre = genero.Nombre;
                    perfilDTO.GeneroCodigo = genero.Codigo;
                }
                
                // Agregar información del rol (según diagrama)
                if (rol != null)
                {
                    perfilDTO.RolNombre = rol.Nombre;
                    perfilDTO.RolCodigo = rol.Codigo;
                }
                
                // Agregar información de la carrera (según diagrama)
                if (carrera != null)
                {
                    perfilDTO.IdCarrera = carrera.Id;
                    perfilDTO.CarreraNombre = carrera.Nombre;
                    perfilDTO.CarreraCodigo = carrera.Codigo;
                    // Mantener compatibilidad con campo legacy
                    perfilDTO.Carrera = carrera.Nombre;
                }
                
                // Convertir CV de byte[] a string base64 si existe
                if (perfilCandidato.Cv != null && perfilCandidato.Cv.Length > 0)
                {
                    perfilDTO.Cv = Convert.ToBase64String(perfilCandidato.Cv);
                }
                
                // Calcular porcentaje de perfil completado
                perfilDTO.PorcentajePerfil = CalcularPorcentajePerfil(perfilCandidato, usuario);
                
                // Campos que no están en el diagrama - los dejamos null
                perfilDTO.Telefono = null; // No existe en el diagrama
                perfilDTO.Localidad = null; // No está relacionado en el diagrama  
                perfilDTO.Carrera = null; // No está relacionado en el diagrama

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

        public async Task<PerfilCandidatoDTO> GetPerfilByUsuarioId(int usuarioId)
        {
            try
            {
                // Buscar el perfil candidato
                var perfilesCandidatos = await _unitOfWork.GenericRepository<PerfilCandidato>().GetAll();
                var perfilCandidato = perfilesCandidatos.FirstOrDefault(p => p.IdUsuario == usuarioId && p.FechaBaja == null);

                if (perfilCandidato == null)
                {
                    throw new ApiException("Perfil de candidato no encontrado", 404);
                }

                // Obtener usuario relacionado
                var usuarios = await _unitOfWork.GenericRepository<Usuario>().GetAll();
                var usuario = usuarios.FirstOrDefault(u => u.Id == perfilCandidato.IdUsuario);

                // Obtener género relacionado si existe
                Genero? genero = null;
                if (perfilCandidato.IdGenero.HasValue)
                {
                    var generos = await _unitOfWork.GenericRepository<Genero>().GetAll();
                    genero = generos.FirstOrDefault(g => g.Id == perfilCandidato.IdGenero.Value);
                }

                // Obtener carrera relacionada si existe
                Carrera? carrera = null;
                if (perfilCandidato.IdCarrera.HasValue)
                {
                    var carreras = await _unitOfWork.GenericRepository<Carrera>().GetAll();
                    carrera = carreras.FirstOrDefault(c => c.Id == perfilCandidato.IdCarrera.Value);
                }

                // Mapear a DTO
                var perfilDTO = perfilCandidato.Adapt<PerfilCandidatoDTO>();
                
                // Agregar información del usuario
                if (usuario != null)
                {
                    perfilDTO.Nombre = usuario.Nombre;
                    perfilDTO.Email = usuario.Email;
                }
                
                // Agregar información del género
                perfilDTO.GeneroNombre = genero?.Nombre;
                
                // Agregar información de la carrera
                if (carrera != null)
                {
                    perfilDTO.IdCarrera = carrera.Id;
                    perfilDTO.CarreraNombre = carrera.Nombre;
                    perfilDTO.CarreraCodigo = carrera.Codigo;
                    // Mantener compatibilidad con campo legacy
                    perfilDTO.Carrera = carrera.Nombre;
                }
                
                // Convertir CV de byte[] a string base64 si existe
                if (perfilCandidato.Cv != null && perfilCandidato.Cv.Length > 0)
                {
                    perfilDTO.Cv = Convert.ToBase64String(perfilCandidato.Cv);
                }
                
                // Calcular porcentaje de perfil completado (lógica básica)
                perfilDTO.PorcentajePerfil = CalcularPorcentajePerfil(perfilCandidato, usuario);
                
                // Datos que no están en el diagrama - los dejamos null
                perfilDTO.Telefono = null; // No existe en el diagrama
                perfilDTO.Localidad = null; // No está relacionado en el diagrama  
                perfilDTO.Carrera = null; // No está relacionado en el diagrama

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

        public async Task<PerfilCandidatoDTO> UpdatePerfil(PerfilCandidatoDTO perfilDTO)
        {
            try
            {
                var perfilesCandidatos = await _unitOfWork.GenericRepository<PerfilCandidato>().GetAll();
                var perfilExistente = perfilesCandidatos.FirstOrDefault(p => p.Id == perfilDTO.Id && p.FechaBaja == null);

                if (perfilExistente == null)
                {
                    throw new ApiException("Perfil de candidato no encontrado", 404);
                }

                // Actualizar campos
                perfilExistente.Descripcion = perfilDTO.Descripcion ?? perfilExistente.Descripcion;
                perfilExistente.IdGenero = perfilDTO.IdGenero ?? perfilExistente.IdGenero;
                perfilExistente.Legajo = perfilDTO.Legajo ?? perfilExistente.Legajo;
                perfilExistente.AnioEgreso = perfilDTO.AnioEgreso ?? perfilExistente.AnioEgreso;
                perfilExistente.FechaModificacion = DateTime.Now;

                // Convertir CV de base64 a byte[] si se proporciona
                if (!string.IsNullOrEmpty(perfilDTO.Cv))
                {
                    perfilExistente.Cv = Convert.FromBase64String(perfilDTO.Cv);
                }

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.GenericRepository<PerfilCandidato>().Update(perfilExistente);
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

        private int CalcularPorcentajePerfil(PerfilCandidato perfil, Usuario? usuario)
        {
            int porcentaje = 0;

            // Campos básicos (20 puntos cada uno)
            if (!string.IsNullOrEmpty(perfil.Descripcion)) porcentaje += 20;
            if (!string.IsNullOrEmpty(perfil.Legajo)) porcentaje += 15;
            if (perfil.AnioEgreso.HasValue) porcentaje += 15;
            if (perfil.IdGenero.HasValue) porcentaje += 10;
            if (perfil.Cv != null && perfil.Cv.Length > 0) porcentaje += 25;
            
            // Campos del usuario
            if (usuario != null)
            {
                if (!string.IsNullOrEmpty(usuario.Nombre)) porcentaje += 10;
                if (!string.IsNullOrEmpty(usuario.Email)) porcentaje += 5;
            }

            return Math.Min(porcentaje, 100); // Máximo 100%
        }
    }
}

