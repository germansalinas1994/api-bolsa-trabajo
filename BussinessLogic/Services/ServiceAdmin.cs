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
    public class ServiceAdmin : GenericService
    {
        public ServiceAdmin(IUnitOfWork unitOfWork) : base(unitOfWork) { }

     

        public async Task AltaUsuario(int idUsuarioAlta, int idUsuario)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                Usuario usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(idUsuario);

                if (usuario == null)
                {
                    throw new ApiException("Usuario no encontrado", 404);
                }

                if (usuario.IdRol != Rol.IdRolAdmin)
                {
                    throw new ApiException("Acceso denegado. El usuario no es administrador.", 403);
                }

                Usuario usuarioAlta = await _unitOfWork.GenericRepository<Usuario>().GetById(idUsuarioAlta);
                if (usuarioAlta == null)
                {
                    throw new ApiException("Usuario a dar de alta no encontrado", 404);
                }
                usuarioAlta.FechaBaja = null;
                usuarioAlta.FechaModificacion = DateTime.UtcNow;
                usuarioAlta.Activo = true;
                await _unitOfWork.GenericRepository<Usuario>().Update(usuarioAlta);

                if (usuarioAlta.IdRol == Rol.IdRolEmpresa)
                {
                    //busco el perfil que tiene de acuerdo al rol que tenia
                    var perfilEmpresa = (await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(pe => pe.IdUsuario == usuarioAlta.Id)).FirstOrDefault();
                    if (perfilEmpresa != null)
                    {
                        perfilEmpresa.FechaBaja = null;
                        perfilEmpresa.FechaModificacion = DateTime.UtcNow;
                        perfilEmpresa.IdEstadoValidacion = EstadoValidacion.IdEstadoIniciada;
                        await _unitOfWork.GenericRepository<PerfilEmpresa>().Update(perfilEmpresa);
                    }
                }
                else if (usuarioAlta.IdRol == Rol.IdRolCandidato)
                {
                    var perfilCandidato = (await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteria(pe => pe.IdUsuario == usuarioAlta.Id)).FirstOrDefault();
                    if (perfilCandidato != null)
                    {
                        perfilCandidato.FechaBaja = null;
                        perfilCandidato.FechaModificacion = DateTime.UtcNow;
                        await _unitOfWork.GenericRepository<PerfilCandidato>().Update(perfilCandidato);
                    }
                }

                await _unitOfWork.CommitAsync();

            }
            catch (ApiException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new ApiException("Error al cambiar el estado de validación", 500, ex.Message);
            }
        }

        public async Task BajaUsuario(int idUsuarioBaja, int idUsuario)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                Usuario usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(idUsuario);

                if (usuario == null)
                {
                    throw new ApiException("Usuario no encontrado", 404);
                }

                if (usuario.IdRol != Rol.IdRolAdmin)
                {
                    throw new ApiException("Acceso denegado. El usuario no es administrador.", 403);
                }

                Usuario usuarioBaja = await _unitOfWork.GenericRepository<Usuario>().GetById(idUsuarioBaja);
                if (usuarioBaja == null)
                {
                    throw new ApiException("Usuario a dar de baja no encontrado", 404);
                }

                usuarioBaja.FechaBaja = DateTime.UtcNow;
                usuarioBaja.FechaModificacion = DateTime.UtcNow;
                usuarioBaja.Activo = false;
                await _unitOfWork.GenericRepository<Usuario>().Update(usuarioBaja);

                //busco el perfil que tiene de acuerdo al rol que tenia
                if (usuarioBaja.IdRol == Rol.IdRolEmpresa)
                {
                    var perfilEmpresa = (await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteria(pe => pe.IdUsuario == usuarioBaja.Id)).FirstOrDefault();
                    if (perfilEmpresa != null)
                    {
                        perfilEmpresa.FechaBaja = DateTime.UtcNow;
                        perfilEmpresa.FechaModificacion = DateTime.UtcNow;
                        perfilEmpresa.IdEstadoValidacion = EstadoValidacion.IdEstadoRechazada;

                        await _unitOfWork.GenericRepository<PerfilEmpresa>().Update(perfilEmpresa);
                    }
                }
                else if (usuarioBaja.IdRol == Rol.IdRolCandidato)
                {
                    var perfilCandidato = (await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteria(pe => pe.IdUsuario == usuarioBaja.Id)).FirstOrDefault();
                    if (perfilCandidato != null)
                    {
                        perfilCandidato.FechaBaja = DateTime.UtcNow;
                        await _unitOfWork.GenericRepository<PerfilCandidato>().Update(perfilCandidato);
                    }
                }
                await _unitOfWork.CommitAsync();
            }
            catch (ApiException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new ApiException("Error al cambiar el estado de validación", 500, ex.Message);
            }
        }

        public async Task CambiarEstadoValidacion(int idPerfilEmpresa, bool aprobado, int idUsuario)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                EstadoValidacion estadoValidacion;
                // Verificar que el usuario es admin
                var usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(idUsuario);
                if (usuario == null)
                {
                    throw new ApiException("Usuario no encontrado", 404);
                }

                var rol = await _unitOfWork.GenericRepository<Rol>().GetById(usuario.IdRol);
                if (rol == null || rol.Id != Rol.IdRolAdmin)
                {
                    throw new ApiException("Acceso denegado. El usuario no es administrador.", 403);
                }

                // Obtener el perfil de la empresa
                var perfilEmpresa = await _unitOfWork.GenericRepository<PerfilEmpresa>().GetById(idPerfilEmpresa);
                if (perfilEmpresa == null || perfilEmpresa.FechaBaja != null)
                {
                    throw new ApiException("Perfil de empresa no encontrado", 404);
                }

                // Cambiar el estado de validación
                if (aprobado)
                {
                    estadoValidacion = (await _unitOfWork.GenericRepository<EstadoValidacion>().GetByCriteria(ev => ev.Id == EstadoValidacion.IdEstadoAprobada)).FirstOrDefault();
                }
                else
                {
                    estadoValidacion = (await _unitOfWork.GenericRepository<EstadoValidacion>().GetByCriteria(ev => ev.Id == EstadoValidacion.IdEstadoRechazada)).FirstOrDefault();
                }

                if (estadoValidacion == null)
                {
                    throw new ApiException("Estado de validación no encontrado", 404);
                }


                //aca se puede generar la notificacion para la empresa sobre el cambio de estado
                perfilEmpresa.IdEstadoValidacion = estadoValidacion.Id;
                perfilEmpresa.FechaModificacion = DateTime.UtcNow;

                // Generar notificación para la empresa

                //tambien en caso de ser rechazado, se tienen que setear fecha baja a las ofertas activas de la empresa
                // if (estadoValidacion.Id == EstadoValidacion.IdEstadoRechazada)
                // {
                //     var ofertasActivas = (await _unitOfWork.GenericRepository<Oferta>().GetByCriteria(o => o.IdPerfilEmpresa == idPerfilEmpresa && o.FechaBaja == null)).ToList();
                //     foreach (var oferta in ofertasActivas)
                //     {
                //         oferta.FechaBaja = DateTime.UtcNow;
                //         oferta.FechaModificacion = DateTime.UtcNow;
                //         await _unitOfWork.GenericRepository<Oferta>().Update(oferta);

                //         //por cada oferta tengo que poner el historial en baja tambien
                //         //TENGO QUE CREAR UNA NUEVA OFERTA HISTORIAL CON FECHA DE BAJA
                //         var ofertaHistorial = new OfertaHistorial
                //         {
                //             IdOferta = oferta.Id,
                //             IdEstadoOferta = EstadoOferta.IdEstadoBaja,
                //             FechaAlta = DateTime.UtcNow,
                //             FechaModificacion = DateTime.UtcNow,
                //             FechaBaja = DateTime.UtcNow,
                //         };
                //     }
                // }                  

                await _unitOfWork.GenericRepository<PerfilEmpresa>().Update(perfilEmpresa);
                await _unitOfWork.CommitAsync();

            }
            catch (ApiException ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new ApiException("Error al cambiar el estado de validación", 500, ex.Message);
            }
        }

        public async Task<List<PerfilEmpresaDTO>> GetEmpresasPorVerificar(int idUsuario, GenericSearchDTO filtro)
        {
            try
            {
                // Verificar que el usuario es admin
                var usuario = await _unitOfWork.GenericRepository<Usuario>().GetById(idUsuario);
                if (usuario == null)
                {
                    throw new ApiException("Usuario no encontrado", 404);
                }

                var rol = await _unitOfWork.GenericRepository<Rol>().GetById(usuario.IdRol);
                if (rol == null || rol.Id != Rol.IdRolAdmin)
                {
                    throw new ApiException("Acceso denegado. El usuario no es administrador.", 403);
                }

                // Obtener las empresas que necesitan verificación
                var search = (await _unitOfWork.GenericRepository<PerfilEmpresa>().Search()).Where(o => o.FechaBaja == null);

                if (filtro.Input != null)
                {
                    search = search.Where(o => o.Descripcion.Contains(filtro.Input) || o.RazonSocial.Contains(filtro.Input) || o.Cuit.Contains(filtro.Input) || o.Usuario.Nombre.Contains(filtro.Input));
                }
                if (filtro.Estados != null && filtro.Estados.Count > 0)
                {
                    IList<int> idsEstados = (await _unitOfWork.GenericRepository<EstadoValidacion>().GetByCriteria(m => filtro.Estados.Contains(m.Codigo))).Select(m => m.Id).ToList();
                    search = search.Where(o => idsEstados.Contains(o.IdEstadoValidacion ?? 0));
                }

                List<PerfilEmpresa> perfilesEmpresas = search
                                .Include(pe => pe.Usuario)
                                .ThenInclude(u => u.Rol)
                                .Include(pe => pe.EstadoValidacion)
                                .OrderByDescending(o => o.FechaModificacion).ToList();

                // Mapear a DTOs
                List<PerfilEmpresaDTO> perfilesDTO = perfilesEmpresas
                    .Adapt<List<PerfilEmpresaDTO>>();

                return perfilesDTO;
            }
            catch (ApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ApiException("Error al obtener las empresas por verificar", 500, ex.Message);
            }
        }

        public async Task<List<UsuarioDTO>> GetUsuarios()
        {
            try
            {
                var search = await _unitOfWork.GenericRepository<Usuario>().Search();

                // search = search.Where(u => u.FechaBaja == null);

                List<Usuario> usuarios = search
                                .Include(u => u.Rol)
                                .OrderBy(u => u.Nombre).ToList();


                List<UsuarioDTO> usuariosDTO = usuarios.Adapt<List<UsuarioDTO>>();

                return usuariosDTO;
            }
            catch (ApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ApiException("Error al obtener los usuarios", 500, ex.Message);
            }
        }

        public async Task<PerfilCompletoDTO> VerDetalleUsuario(int idUsuarioRegistrado)
        {
            try
            {
                var perfilCompleto = new PerfilCompletoDTO();
                var usuario = await _unitOfWork.GenericRepository<Usuario>().GetByIdIncludingRelations(idUsuarioRegistrado);

                if (usuario == null)
                {
                    throw new ApiException("Usuario no encontrado", 404);
                }

                //obtengo el perfil segun el rol
                if (usuario.IdRol == Rol.IdRolCandidato)
                {
                    var perfilCandidato = (await _unitOfWork.GenericRepository<PerfilCandidato>().GetByCriteriaIncludingRelations(pe => pe.IdUsuario == usuario.Id)).FirstOrDefault();
                    if (perfilCandidato == null)
                    {
                        throw new ApiException("Perfil de candidato no encontrado", (int)HttpStatusCode.NotFound);
                    }

                    perfilCompleto = perfilCandidato.Adapt<PerfilCompletoDTO>();
                }
                else if (usuario.IdRol == Rol.IdRolEmpresa)
                {
                    var perfilEmpresa = (await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByCriteriaIncludingRelations(pe => pe.IdUsuario == usuario.Id)).FirstOrDefault();
                    if (perfilEmpresa == null)
                    {
                        throw new ApiException("Perfil de empresa no encontrado", (int)HttpStatusCode.NotFound);
                    }
                    perfilCompleto = perfilEmpresa.Adapt<PerfilCompletoDTO>();
                }
                else
                {
                    perfilCompleto = usuario.Adapt<PerfilCompletoDTO>();
                }


                return perfilCompleto;

            }
            catch (ApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ApiException("Error al obtener los usuarios", 500, ex.Message);
            }
        }
        

    }
}