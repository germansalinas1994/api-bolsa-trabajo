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
                // Buscar el perfil candidato por ID usando GetByCriteria para filtrar por ID y FechaBaja
                var perfilesCandidatos = await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteria(p => p.Id == perfilId && p.FechaBaja == null);
                var perfilCandidato = perfilesCandidatos.FirstOrDefault();

                if (perfilCandidato == null)
                {
                    throw new ApiException("Perfil de candidato no encontrado", 404);
                }

                // Obtener usuario relacionado usando GetById
                var usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(perfilCandidato.IdUsuario);

                // Obtener género relacionado si existe usando GetById
                Genero? genero = null;
                if (perfilCandidato.IdGenero.HasValue)
                {
                    genero = await _unitOfWork.GenericRepository<Genero>().GetById(perfilCandidato.IdGenero.Value);
                }

                // Obtener carrera relacionada si existe usando GetById
                Carrera? carrera = null;
                if (perfilCandidato.IdCarrera.HasValue)
                {
                    carrera = await _unitOfWork.GenericRepository<Carrera>().GetById(perfilCandidato.IdCarrera.Value);
                }

                // Obtener rol relacionado a través del usuario usando GetById
                Rol? rol = null;
                if (usuario != null)
                {
                    rol = await _unitOfWork.GenericRepository<Rol>().GetById(usuario.IdRol);
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
                    perfilDTO.FotoPerfil = usuario.FotoPerfil; // Agregar foto de perfil
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
                // Buscar el perfil candidato usando GetByCriteria para filtrar por IdUsuario y FechaBaja
                var perfilesCandidatos = await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteria(p => p.IdUsuario == usuarioId && p.FechaBaja == null);
                var perfilCandidato = perfilesCandidatos.FirstOrDefault();

                if (perfilCandidato == null)
                {
                    throw new ApiException("Perfil de candidato no encontrado", 404);
                }

                // Obtener usuario relacionado usando GetById
                var usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(perfilCandidato.IdUsuario);

                // Obtener género relacionado si existe usando GetById
                Genero? genero = null;
                if (perfilCandidato.IdGenero.HasValue)
                {
                    genero = await _unitOfWork.GenericRepository<Genero>().GetById(perfilCandidato.IdGenero.Value);
                }

                // Obtener carrera relacionada si existe usando GetById
                Carrera? carrera = null;
                if (perfilCandidato.IdCarrera.HasValue)
                {
                    carrera = await _unitOfWork.GenericRepository<Carrera>().GetById(perfilCandidato.IdCarrera.Value);
                }

                // Mapear a DTO
                var perfilDTO = perfilCandidato.Adapt<PerfilCandidatoDTO>();
                
                // Agregar información del usuario
                if (usuario != null)
                {
                    perfilDTO.Nombre = usuario.Nombre;
                    perfilDTO.Email = usuario.Email;
                    perfilDTO.FotoPerfil = usuario.FotoPerfil; // Agregar foto de perfil
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
                // Buscar el perfil existente usando GetByCriteria para filtrar por Id y FechaBaja
                var perfilesCandidatos = await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteria(p => p.Id == perfilDTO.Id && p.FechaBaja == null);
                var perfilExistente = perfilesCandidatos.FirstOrDefault();

                if (perfilExistente == null)
                {
                    throw new ApiException("Perfil de candidato no encontrado", 404);
                }

                // Actualizar campos del perfil
                perfilExistente.Descripcion = perfilDTO.Descripcion ?? perfilExistente.Descripcion;
                perfilExistente.IdGenero = perfilDTO.IdGenero ?? perfilExistente.IdGenero;
                perfilExistente.IdCarrera = perfilDTO.IdCarrera ?? perfilExistente.IdCarrera;
                perfilExistente.Legajo = perfilDTO.Legajo ?? perfilExistente.Legajo;
                perfilExistente.AnioEgreso = perfilDTO.AnioEgreso ?? perfilExistente.AnioEgreso;
                perfilExistente.FechaModificacion = DateTime.Now;

                // Convertir CV de base64 a byte[] si se proporciona
                if (!string.IsNullOrEmpty(perfilDTO.Cv))
                {
                    perfilExistente.Cv = Convert.FromBase64String(perfilDTO.Cv);
                }

                // Si se proporciona un nombre o foto de perfil, actualizar el usuario
                if (!string.IsNullOrEmpty(perfilDTO.Nombre) || !string.IsNullOrEmpty(perfilDTO.FotoPerfil))
                {
                    var usuarios = await _unitOfWork.GenericRepository<Usuario>().GetByCriteria(u => u.Id == perfilExistente.IdUsuario && u.FechaBaja == null);
                    var usuario = usuarios.FirstOrDefault();
                    
                    if (usuario != null)
                    {
                        if (!string.IsNullOrEmpty(perfilDTO.Nombre))
                        {
                            usuario.Nombre = perfilDTO.Nombre;
                        }
                        if (!string.IsNullOrEmpty(perfilDTO.FotoPerfil))
                        {
                            usuario.FotoPerfil = perfilDTO.FotoPerfil;
                        }
                        await _unitOfWork.GenericRepository<Usuario>().Update(usuario);
                    }
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

        public async Task<object> VerificarPerfilCompleto(string email)
        {
            try
            {
                // Buscar usuario por email
                var usuarios = await _unitOfWork.GenericRepository<Usuario>().GetByCriteria(u => u.Email == email && u.FechaBaja == null);
                var usuario = usuarios.FirstOrDefault();

                if (usuario == null)
                {
                    return new { perfilCompleto = false, mensaje = "Usuario no encontrado" };
                }

                // Buscar perfil de candidato
                var perfiles = await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteria(p => p.IdUsuario == usuario.Id && p.FechaBaja == null);
                var perfil = perfiles.FirstOrDefault();

                if (perfil == null)
                {
                    return new { perfilCompleto = false, mensaje = "Perfil no encontrado" };
                }

                // Verificar si los campos obligatorios están completos
                bool perfilCompleto = !string.IsNullOrEmpty(usuario.Nombre) &&
                                     perfil.IdGenero.HasValue &&
                                     perfil.IdCarrera.HasValue &&
                                     !string.IsNullOrEmpty(perfil.Legajo) &&
                                     perfil.AnioEgreso.HasValue &&
                                     usuario.Activo == true;

                return new 
                { 
                    perfilCompleto = perfilCompleto,
                    usuario = new 
                    {
                        id = usuario.Id,
                        email = usuario.Email,
                        nombre = usuario.Nombre,
                        activo = usuario.Activo
                    },
                    perfil = new 
                    {
                        id = perfil.Id,
                        idGenero = perfil.IdGenero,
                        idCarrera = perfil.IdCarrera,
                        legajo = perfil.Legajo,
                        anioEgreso = perfil.AnioEgreso
                    }
                };
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

        public async Task<PerfilCandidatoDTO> CompletarPerfil(PerfilCandidatoDTO perfilDTO)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validar datos obligatorios
                if (string.IsNullOrEmpty(perfilDTO.Email))
                {
                    throw new ApiException("Email es requerido", 400);
                }
                if (string.IsNullOrEmpty(perfilDTO.Nombre))
                {
                    throw new ApiException("Nombre es requerido", 400);
                }
                if (!perfilDTO.IdGenero.HasValue)
                {
                    throw new ApiException("Género es requerido", 400);
                }
                if (!perfilDTO.IdCarrera.HasValue)
                {
                    throw new ApiException("Carrera es requerida", 400);
                }
                if (string.IsNullOrEmpty(perfilDTO.Legajo))
                {
                    throw new ApiException("Legajo es requerido", 400);
                }

                // Buscar usuario por email
                var usuarios = await _unitOfWork.GenericRepository<Usuario>().GetByCriteria(u => u.Email == perfilDTO.Email && u.FechaBaja == null);
                var usuario = usuarios.FirstOrDefault();

                if (usuario == null)
                {
                    throw new ApiException("Usuario no encontrado", 404);
                }

                // Actualizar datos del usuario
                usuario.Nombre = perfilDTO.Nombre;
                usuario.Activo = true;
                usuario.FechaModificacion = DateTime.Now;
                await _unitOfWork.GenericRepository<Usuario>().Update(usuario);

                // Buscar o crear perfil de candidato
                var perfiles = await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteria(p => p.IdUsuario == usuario.Id && p.FechaBaja == null);
                var perfil = perfiles.FirstOrDefault();

                if (perfil == null)
                {
                    // Crear nuevo perfil
                    perfil = new PerfilCandidato
                    {
                        IdUsuario = usuario.Id,
                        IdGenero = perfilDTO.IdGenero,
                        IdCarrera = perfilDTO.IdCarrera,
                        Legajo = perfilDTO.Legajo,
                        AnioEgreso = perfilDTO.AnioEgreso ?? DateTime.Now.Year,
                        Descripcion = perfilDTO.Descripcion ?? string.Empty,
                        FechaAlta = DateTime.Now,
                        FechaModificacion = DateTime.Now
                    };
                    perfil = await _unitOfWork.GenericRepository<PerfilCandidato>().Insert(perfil);
                }
                else
                {
                    // Actualizar perfil existente
                    perfil.IdGenero = perfilDTO.IdGenero;
                    perfil.IdCarrera = perfilDTO.IdCarrera;
                    perfil.Legajo = perfilDTO.Legajo;
                    perfil.AnioEgreso = perfilDTO.AnioEgreso ?? perfil.AnioEgreso;
                    perfil.Descripcion = perfilDTO.Descripcion ?? perfil.Descripcion;
                    perfil.FechaModificacion = DateTime.Now;
                    await _unitOfWork.GenericRepository<PerfilCandidato>().Update(perfil);
                }

                await _unitOfWork.CommitAsync();

                // Retornar el perfil actualizado
                return await GetPerfilById(perfil.Id);
            }
            catch (ApiException)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new ApiException(ex);
            }
        }

        public async Task<int> CalcularPorcentaje(string email)
        {
            int porcentaje = 0;

            Usuario usuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault();
            var perfil = (await _unitOfWork.GenericRepository<PerfilCandidato>()
                    .GetByCriteria(p => p.IdUsuario == usuario.Id)).FirstOrDefault();

            // Campos básicos (20 puntos cada uno)
            if (!string.IsNullOrEmpty(perfil.Descripcion)) porcentaje += 20;
            if (!string.IsNullOrEmpty(perfil.Legajo)) porcentaje += 15;
            if (perfil.AnioEgreso.HasValue) porcentaje += 15;
            if (perfil.IdGenero.HasValue) porcentaje += 10;
            if (perfil.Cv != null && perfil.Cv.Length > 0) porcentaje += 25;
            
            // Campos del usuario
            if (perfil != null)
            {
                if (!string.IsNullOrEmpty(usuario.Nombre)) porcentaje += 10;
                if (!string.IsNullOrEmpty(usuario.Email)) porcentaje += 5;
            }

            return Math.Min(porcentaje, 100); // Máximo 100%
        }

    }
}

