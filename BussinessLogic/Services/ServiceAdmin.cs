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
                private readonly ServiceEmail _serviceEmail;

        public ServiceAdmin(IUnitOfWork unitOfWork, ServiceEmail serviceEmail) : base(unitOfWork)
        {
            _serviceEmail = serviceEmail;
        }



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
                var perfilEmpresa = await _unitOfWork.GenericRepository<PerfilEmpresa>().GetByIdIncludingRelations(idPerfilEmpresa);
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


                await _unitOfWork.GenericRepository<PerfilEmpresa>().Update(perfilEmpresa);
                await _unitOfWork.CommitAsync();
                EnviarMailCambioEstadoValidacion(perfilEmpresa, estadoValidacion);

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
        private async Task EnviarMailCambioEstadoValidacion(PerfilEmpresa perfilEmpresa, EstadoValidacion estadoValidacion)
        {
            var emailEmpresa = perfilEmpresa?.Usuario?.Email ?? "noreply@utnfrlp.edu.ar";
            var razonSocial = perfilEmpresa?.RazonSocial ?? "Su Empresa";
            var estado = estadoValidacion?.Nombre ?? "Sin estado";
            var aprobado = estadoValidacion?.Id == EstadoValidacion.IdEstadoAprobada;

            string mensajeEstado = aprobado
                ? $"<p>Nos complace informarle que su empresa <b>{razonSocial}</b> ha sido <b style='color:green'>APROBADA</b> para operar en la Bolsa de Trabajo de la UTN FRLP.</p>"
                : $"<p>Lamentamos informarle que su empresa <b>{razonSocial}</b> ha sido <b style='color:red'>RECHAZADA</b> en el proceso de validación de la Bolsa de Trabajo UTN FRLP.</p>";

            string cuerpoHtml = $@"
    <html>
      <body style='font-family: Arial, sans-serif; color: #333; background-color: #f5f5f5; padding: 20px;'>
        <table align='center' width='600' cellpadding='0' cellspacing='0' 
               style='background-color: #fff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
          <tr>
            <td style='background-color: #003366; padding: 20px; text-align: center;'>
              <img src='https://www.frlp.utn.edu.ar/sites/default/files/LOGO%20VERTICAL_1.jpg' 
                   alt='UTN FRLP Logo' width='120' style='display:block; margin:auto; border-radius:5px;' />
              <h2 style='color: #fff; margin-top: 10px;'>Bolsa de Trabajo - UTN FRLP</h2>
            </td>
          </tr>
          <tr>
            <td style='padding: 30px;'>
              <p>Estimado/a representante de <b>{razonSocial}</b>,</p>
              {mensajeEstado}
              {(aprobado ?
                        "<p>Desde este momento puede acceder al portal de la Bolsa de Trabajo para publicar ofertas laborales y gestionar postulaciones de estudiantes y graduados.</p>" :
                        "<p>Podrá volver a solicitar la validación cuando haya actualizado los datos requeridos o corregido la información pendiente.</p>")}
              <p>Estado actual: <b style='color:#003366'>{estado}</b></p>
              <p>
                Ingrese al portal desde 
                <a href='https://bolsadetrabajo.utnfrlp.edu.ar' 
                   style='color: #003366; text-decoration: none; font-weight: bold;'>
                   Bolsa de Trabajo UTN FRLP
                </a>.
              </p>
              <p style='margin-top: 30px; color: #555; font-size: 14px;'>
                Este es un mensaje automático. Por favor, no responda a este correo.
              </p>
            </td>
          </tr>
          <tr>
            <td style='background-color: #f0f0f0; text-align: center; padding: 15px; font-size: 12px; color: #666;'>
              © {DateTime.Now.Year} Universidad Tecnológica Nacional - Facultad Regional La Plata<br/>
              <a href='https://www.frlp.utn.edu.ar' style='color: #003366; text-decoration: none;'>www.frlp.utn.edu.ar</a>
            </td>
          </tr>
        </table>
      </body>
    </html>";

            string asunto = aprobado
                ? "Validación aprobada - Bolsa de Trabajo UTN FRLP"
                : "Validación rechazada - Bolsa de Trabajo UTN FRLP";

            await _serviceEmail.EnviarCorreoAsync(
                destinatario: emailEmpresa,
                asunto: asunto,
                cuerpoHtml: cuerpoHtml
            );
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