using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.IRepository;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Metadata;


namespace DataAccess.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        protected readonly DbBolsaTrabajoContext _context;
        public GenericRepository(DbBolsaTrabajoContext mydbContext)
        {
            _context = mydbContext;
        }

        #region Traer todos los datos de una tabla
        public async Task<IList<T>> GetAll()
        {

            return await _context.Set<T>().ToListAsync();
        }
        #endregion

        #region buscar por ID
        public async Task<T?> GetById(int id)
        {
            var entityType = _context.Model.FindEntityType(typeof(T))
                             ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no está mapeado en el DbContext.");

            var pk = entityType.FindPrimaryKey()
                     ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no tiene clave primaria definida.");

            if (pk.Properties.Count != 1 || pk.Properties[0].ClrType != typeof(int))
                throw new InvalidOperationException($"El repositorio espera una PK int simple, pero {typeof(T).Name} tiene otra configuración.");

            // Si pasa la validación, usamos FindAsync
            return await _context.Set<T>().FindAsync(id);
        }
        #endregion

        #region insertar entidad generica
        public async Task<T> Insert(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        #endregion

        #region Actualizar entidad generica 

        public async Task<T> Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        #endregion

        #region Eliminado Fisico
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
        public async Task<bool> SoftDelete(int id)
        {
            var entity = await GetById(id);
            if (entity == null)
            {
                return false;
            }


            var propertyInfo = entity.GetType().GetProperty("FechaBaja");
            if (propertyInfo == null)
            {
                return false;
            }
            propertyInfo.SetValue(entity, DateTime.Now);

            await _context.SaveChangesAsync();

            return true;
        }


        #endregion

        #region Buscar bajo algun criterio
        public async Task<IList<T>> GetByCriteria(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }


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

            var navigationProperties = _context.Model.FindEntityType(typeof(T))
                                            .GetNavigations()
                                            .Select(navigation => navigation.Name);

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

            foreach (var navigationProperty in navigationProperties)
            {
                query = query.Include(navigationProperty);
            }

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


        public async Task<IQueryable<T>> Search()
        {
            return await Task.FromResult(_context.Set<T>().AsQueryable());
        }

        // public async Task<IQueryable<T>> Search()
        // {
        //     return _context.Set<T>().AsQueryable();
        // }


        public async Task<T?> GetByIdIncludingRelations(
            int id)
        {
            var entityType = _context.Model.FindEntityType(typeof(T))
                             ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no está mapeado en el DbContext.");

            var pk = entityType.FindPrimaryKey()
                     ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no tiene clave primaria definida.");

            if (pk.Properties.Count != 1 || pk.Properties[0].ClrType != typeof(int) || pk.Properties[0].Name != "Id")
                throw new InvalidOperationException($"Se esperaba una PK simple 'Id' de tipo int para {typeof(T).Name}.");

            IQueryable<T> query = _context.Set<T>();

            foreach (var navName in entityType.GetNavigations().Select(n => n.Name))
                query = query.Include(navName);


            var param = Expression.Parameter(typeof(T), "x");
            var prop = Expression.Property(param, "Id");             // PK = "Id"
            var body = Expression.Equal(prop, Expression.Constant(id));
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return await query.FirstOrDefaultAsync(lambda);
        }


       

        public async Task<IList<T>> GetAllIncludingSpecificRelations(
           Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
           bool asNoTracking = true)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
                query = include(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }


        public async Task<IList<T>> GetAllIncludingAllRelations(int maxDepth = 5, bool asNoTracking = true)
        {
            if (maxDepth < 1) maxDepth = 1;

            IQueryable<T> query = _context.Set<T>();
            if (asNoTracking) query = query.AsNoTracking();

            var includePaths = BuildReferenceIncludePaths(typeof(T), maxDepth);

            foreach (var path in includePaths)
                query = query.Include(path);

            return await query.ToListAsync();
        }


        private IEnumerable<string> BuildReferenceIncludePaths(Type rootClrType, int maxDepth)
        {
            var paths = new HashSet<string>();
            var rootEntity = _context.Model.FindEntityType(rootClrType);
            if (rootEntity is null) return paths;

            void Recurse(IEntityType current, string basePath, int depth, HashSet<IEntityType> stack)
            {
                if (depth == 0) return;

                var refNavs = current.GetNavigations()
                                     .Where(n => !n.IsCollection);

                foreach (var nav in refNavs)
                {
                    var nextPath = string.IsNullOrEmpty(basePath) ? nav.Name : $"{basePath}.{nav.Name}";
                    paths.Add(nextPath);

                    var target = nav.TargetEntityType;

                    if (stack.Add(target))
                    {
                        Recurse(target, nextPath, depth - 1, stack);
                        stack.Remove(target);
                    }
                }
            }

            var stack = new HashSet<IEntityType> { rootEntity };
            Recurse(rootEntity, string.Empty, maxDepth, stack);

            return paths;
        }

        public async Task<T?> GetByIdIncludingSpecificRelations(
    object id,
    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
    bool asNoTracking = true,
    bool asSplitQuery = true)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
                query = include(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            if (asSplitQuery)
                query = query.AsSplitQuery();  

            var et = _context.Model.FindEntityType(typeof(T))
                     ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no está mapeado.");
            var pk = et.FindPrimaryKey()
                     ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no tiene PK definida.");

            if (pk.Properties.Count != 1)
                throw new NotSupportedException("Este helper soporta solo clave primaria simple.");

            var keyProp = pk.Properties[0];
            var keyName = keyProp.Name;              
            var keyClrType = keyProp.ClrType;         

            // Construyo x => x.<PK> == (cast)id
            var param = Expression.Parameter(typeof(T), "x");
            var propExpr = Expression.Property(param, keyName);

            object? typedId = id is IConvertible && keyClrType != id.GetType()
                ? Convert.ChangeType(id, keyClrType)
                : (keyClrType == typeof(Guid) && id is string s ? Guid.Parse(s) : id);

            var constant = Expression.Constant(typedId, keyClrType);
            var body = Expression.Equal(propExpr, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return await query.FirstOrDefaultAsync(lambda);
        }




    }

}