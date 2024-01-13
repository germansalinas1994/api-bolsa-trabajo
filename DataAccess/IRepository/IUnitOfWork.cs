using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Entities;

namespace DataAccess.IRepository
{

    // Esta interfaz es para que la implementen los repositorios de cada entidad, generico para que no se repita codigo
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GenericRepository<T>() where T : class;

        //el beginTransactionAsync es para que se inicie la transaccion
        Task BeginTransactionAsync();

        //el commitAsync es para que se impacten los cambios en la base de datos
        Task CommitAsync();

        //el rollbackAsync es para que se deshagan los cambios en la base de datos
        Task RollbackAsync();
    }
}