using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAccess;
using Microsoft.EntityFrameworkCore.Query;

namespace DataAccess.IRepository
{

    // Esta interfaz es para que la implementen los repositorios de cada entidad, generico para que no se repita codigo
    public interface IGenericRepository<T> where T : class
    {
        Task<IList<T>> GetAll();
        Task<IList<T>> GetAllIncludingRelations();
        Task<IList<T>> GetAllIncludingSpecificRelations(
                  Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
                  bool asNoTracking = true);

        Task<T?> GetById(int id);
        Task<T?> GetByIdIncludingRelations(int id);

        Task<T> Insert(T entity);
        Task<T> Update(T entity);
        Task<bool> HardDelete(int id);
        Task<bool> SoftDelete(int id);
        Task<IList<T>> GetByCriteria(Expression<Func<T, bool>> predicate);
        Task<IList<T>> GetByCriteriaMemory(Expression<Func<T, bool>> predicate);

        Task<IList<T>> GetByCriteriaIncludingRelations(Expression<Func<T, bool>> predicate);
        Task<IList<T>> GetByCriteriaIncludingSpecificRelations(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null);
        Task<IList<T>> GetAllIncludingAllRelations(int maxDepth = 5, bool asNoTracking = true);
        IQueryable<T> Search();

        Task<T?> GetByIdIncludingSpecificRelations(
    object id,
    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
    bool asNoTracking = true,
    bool asSplitQuery = true);


        //Otros metodos que se pueden implementar en el futuro

        // Task<bool> Exist(int id);
        // Task<bool> Exist(Func<T, bool> predicate);
        // Task<bool> RemoveAll(IEnumerable<T> entities);
        // Task<IEnumerable<T>> AddRange(IEnumerable<T> entities);


        // //el guid es para que el repositorio de cada entidad tenga un identificador unico pero esto ya se hace ya que tenemos los id autoincrementales
        // Guid GetGuid();


    }
}