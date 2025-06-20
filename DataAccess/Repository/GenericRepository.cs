using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.IRepository;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace DataAccess.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        //Considerar que como usamos addScoped en el startup, cada vez que se haga un request se va a crear un nuevo contexto
        //Por lo tanto no es necesario hacer un using para el contexto ya que se va a cerrar automaticamente al finalizar el request

        //Instancio el contexto que vamos a usar, para esto tengo que agregarlo en el startup
        protected readonly DbBolsaTrabajoContext _context;
        public GenericRepository(DbBolsaTrabajoContext mydbContext)
        {
            _context = mydbContext;
        }

        #region Traer todos los datos de una tabla
        //Metodo para traer todos los datos de una tabla
        public async Task<IList<T>> GetAll()
        {
            //quiero incluir los elementos de la tabla general de la base de datos

            return await _context.Set<T>().ToListAsync();
        }
        #endregion

        #region buscar por ID
        //Metodo para traer un dato de un tipo de objeto por id
        public async Task<T?> GetById(int id)
        {

            //Esto lo hace por la base, hace el where de la base confirmado
            var entity = await _context.Set<T>().FindAsync(id);

            if (entity == null)
            {
                return null;
            }
            return entity;
        }
        #endregion

        #region insertar entidad generica
        //Metodo para insertar un dato de un tipo de objeto
        public async Task<T> Insert(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        #endregion

        #region Actualizar entidad generica 

        //Metodo para actualizar un dato de un tipo de objeto
        public async Task<T> Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        #endregion

        #region Eliminado Fisico
        //Metodo para eliminar un dato de un tipo de objeto, en este caso no se usa el soft delete
        public async Task<bool> HardDelete(int id)
        {
            var entity = await GetById(id);

            if (entity == null)
            {
                return false;
            }
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Soft Delete Generica
        //Metodo para hacer un soft delete de un dato de un tipo de objeto
        public async Task<bool> SoftDelete(int id)
        {
            var entity = await GetById(id);
            if (entity == null)
            {
                // No se encontró la entidad, retornamos false
                return false;
            }

            // Establecemos la fecha y hora actuales en la propiedad FechaHasta
            // Asumimos que todas las entidades tienen esta propiedad
            var propertyInfo = entity.GetType().GetProperty("FechaHasta");
            if (propertyInfo == null)
            {
                // La entidad no tiene la propiedad FechaHasta, retornamos false
                return false;
            }
            propertyInfo.SetValue(entity, DateTime.Now);

            // Guardamos los cambios en la base de datos
            await _context.SaveChangesAsync();

            return true;
        }


        #endregion

        #region Buscar bajo algun criterio
        //Metodo para traer datos de un tipo de objeto por algun criterio
        public async Task<IList<T>> GetByCriteria(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }


        //Otra forma de implementar el get by creiteria pero en este caso trae todo y busca en memoria, lo cual es mejor no hacer ya que no es tan eficiente
        //Es preferible usar el get by criteria ya que si hay muchos datos en la base es ineficiente
        #endregion

        #region Buscar bajo algun criterio en memoria
        public async Task<IList<T>> GetByCriteriaMemory(Expression<Func<T, bool>> predicate)
        {
            return await Task.FromResult(_context.Set<T>().Where(predicate).ToList());
        }
        #endregion


        public async Task<IList<T>> GetAllIncludingRelations()
        {
            var query = _context.Set<T>().AsQueryable();

            // Obtener las propiedades de navegación de la entidad T
            var navigationProperties = _context.Model.FindEntityType(typeof(T))
                                            .GetNavigations()
                                            .Select(navigation => navigation.Name);

            // Incluir cada propiedad de navegación en la consulta
            foreach (var propertyName in navigationProperties)
            {
                query = query.Include(propertyName);
            }

            return await query.ToListAsync();
        }

        public async Task<IList<T>> GetByCriteriaIncludingRelations(Expression<Func<T, bool>> predicate)
        {
            var query = _context.Set<T>().AsQueryable();

            // Obtener las propiedades de navegación de la entidad T
            var entityType = _context.Model.FindEntityType(typeof(T));
            var navigationProperties = entityType.GetNavigations().Select(nav => nav.Name);

            // Aplicar las inclusiones de las propiedades de navegación
            foreach (var navigationProperty in navigationProperties)
            {
                query = query.Include(navigationProperty);
            }

            // Aplicar el criterio de búsqueda
            query = query.Where(predicate);

            return await query.ToListAsync();
        }

        public async Task<IList<T>> GetByCriteriaIncludingSpecificRelations(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _context.Set<T>().AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            query = query.Where(predicate);

            return await query.ToListAsync();
        }


        public IQueryable<T> Search()
        {
            return _context.Set<T>().AsQueryable();
        }




        public async Task<T?> GetByIdIncludingRelations(int id)
        {
            try
            {
                var query = _context.Set<T>().AsQueryable();

                // Obtener las propiedades de navegación de la entidad T
                var entityType = _context.Model.FindEntityType(typeof(T));
                var navigationProperties = entityType.GetNavigations().Select(nav => nav.Name);

                // Incluir cada propiedad de navegación
                foreach (var navigationProperty in navigationProperties)
                {
                    query = query.Include(navigationProperty);
                }

                // Construir el nombre de la clave primaria siguiendo la convención 'Id' + Nombre de la Clase
                var primaryKey = "Id" + typeof(T).Name;

                // Crear una expresión lambda para filtrar por la clave primaria
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, primaryKey);
                var constant = Expression.Constant(id);
                var equals = Expression.Equal(property, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

                return await query.FirstOrDefaultAsync(lambda);
            }
            catch (Exception ex)
            {
                // Manejo de la excepción
                // Aquí puedes registrar el error o manejarlo según tu política de manejo de errores.
                throw;
            }
        }
    }

}