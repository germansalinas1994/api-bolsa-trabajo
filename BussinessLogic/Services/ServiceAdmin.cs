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

        public async Task<List<PerfilEmpresaDTO>> GetEmpresasPorVerificar(int idUsuario)
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
                List<PerfilEmpresa> perfilesEmpresas = (await _unitOfWork.GenericRepository<PerfilEmpresa>()
                    .GetByCriteriaIncludingRelations(pe => pe.FechaBaja == null)).ToList();

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
    }
}