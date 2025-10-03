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
    public class GenericService
    {
        //Instancio el UnitOfWork que vamos a usar
        protected readonly IUnitOfWork _unitOfWork;

        //Inyecto el UnitOfWork por el constructor, esto se hace para que se cree un nuevo contexto por cada vez que se llame a la clase
        public GenericService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UsuarioDTO> CargarUsuarioDesdeJWT(string email)
        {

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //busco que no exista una postulacion igual para la misma oferta y candidato
                var usuarioExistente = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email && u.FechaBaja == null && u.Activo == true)).FirstOrDefault();

                if (usuarioExistente != null)
                    return usuarioExistente.Adapt<UsuarioDTO>();

                //si el usuario no existe, lo creo
                Usuario nuevoUsuario = new();
                nuevoUsuario.Email = email;
                //el rol que va a tener es en base al dominio del email
                string dominio = email.Split('@')[1].ToLower();
                if (dominio == Usuario.DominioAdmin)
                    nuevoUsuario.IdRol = Rol.IdRolAdmin;

                else if (dominio == Usuario.DominioCandidato)
                    nuevoUsuario.IdRol = Rol.IdRolCandidato;
                else
                    nuevoUsuario.IdRol = Rol.IdRolEmpresa;

                nuevoUsuario.FechaAlta = DateTime.Now;
                nuevoUsuario.FechaModificacion = DateTime.Now;
                // nuevoUsuario.Nombre = email.Split('@')[0];
                nuevoUsuario.Activo = true;

                nuevoUsuario = await _unitOfWork.GenericRepository<Usuario>().Insert(nuevoUsuario);
                if (nuevoUsuario == null || nuevoUsuario.Id == 0)
                    throw new ApiException("No se pudo crear el usuario", (int)HttpStatusCode.InternalServerError);



                //en base al rol que tiene creo el perfil
                if (nuevoUsuario.IdRol == Rol.IdRolCandidato)
                {
                    PerfilCandidato nuevoPerfil = new();
                    nuevoPerfil.IdUsuario = nuevoUsuario.Id;
                    nuevoPerfil.FechaAlta = DateTime.Now;
                    nuevoPerfil.FechaModificacion = DateTime.Now;
                    nuevoPerfil = await _unitOfWork.GenericRepository<PerfilCandidato>().Insert(nuevoPerfil);
                }
                else if (nuevoUsuario.IdRol == Rol.IdRolEmpresa)
                {
                    PerfilEmpresa nuevaEmpresa = new();
                    nuevaEmpresa.IdUsuario = nuevoUsuario.Id;
                    //por defecto va en estado iniciada hasta que un admin lo habilite
                    nuevaEmpresa.IdEstadoValidacion = EstadoValidacion.IdEstadoIniciada;
                    nuevaEmpresa.FechaAlta = DateTime.Now;
                    nuevaEmpresa.FechaModificacion = DateTime.Now;
                    nuevaEmpresa = await _unitOfWork.GenericRepository<PerfilEmpresa>().Insert(nuevaEmpresa);
                }




                await _unitOfWork.CommitAsync();

                return nuevoUsuario.Adapt<UsuarioDTO>();


            }
            catch (ApiException)
            {
                await _unitOfWork.RollbackAsync();

                throw; // Re-lanzar la excepción ApiException sin envolverla nuevamente
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                throw ex.InnerException != null ? ex.InnerException : ex;
            }
          
        }

        public async Task<IList<CarreraDTO>> GetAllCarreras()
        {
            try
            {
                List<Carrera> carreras = (await _unitOfWork.GenericRepository<Carrera>()
                    .GetAll()

                    ).ToList();

                return carreras.Adapt<List<CarreraDTO>>();
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

        public async Task<IList<ModalidadDTO>> GetAllModalidades()
        {
            try
            {
                List<Modalidad> modalidades = (await _unitOfWork.GenericRepository<Modalidad>()
                    .GetAll()

                    ).ToList();

                return modalidades.Adapt<List<ModalidadDTO>>();
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

        public async Task<IList<TipoContratoDTO>> GetAllTiposContratos()
        {
            try
            {
                List<TipoContrato> tiposContratos = (await _unitOfWork.GenericRepository<TipoContrato>()
                    .GetAll()

                    ).ToList();

                return tiposContratos.Adapt<List<TipoContratoDTO>>();
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