using System;
using System.Security.Cryptography.X509Certificates;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BussinessLogic.Services
{
    public class ServiceCategoria
    {
        //Instancio el UnitOfWork que vamos a usar
        private readonly IUnitOfWork _unitOfWork;

        //Inyecto el UnitOfWork por el constructor, esto se hace para que se cree un nuevo contexto por cada vez que se llame a la clase
        public ServiceCategoria(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //Metodo para traer todas las categorias
        public async Task<IList<CategoriaDTO>> GetAllCategorias()
        {
            try
            {
                //Traigo todas las categorias
                var categorias = await _unitOfWork.GenericRepository<Categoria>().GetAll();

                //Mapeo las categorias a CategoriaDTO
                var categoriasDTO = categorias.Adapt<IList<CategoriaDTO>>();

                //Devuelvo las categorias
                return categoriasDTO;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
   

    }
}