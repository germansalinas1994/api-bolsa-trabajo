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
            // Validamos que la entidad tenga PK y que sea int
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
            var propertyInfo = entity.GetType().GetProperty("FechaBaja");
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

        public async Task<T?> GetByIdIncludingRelations(
            int id)
        {
            // 1) Validación por metadatos: entidad mapeada y PK int simple
            var entityType = _context.Model.FindEntityType(typeof(T))
                             ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no está mapeado en el DbContext.");

            var pk = entityType.FindPrimaryKey()
                     ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no tiene clave primaria definida.");

            if (pk.Properties.Count != 1 || pk.Properties[0].ClrType != typeof(int) || pk.Properties[0].Name != "Id")
                throw new InvalidOperationException($"Se esperaba una PK simple 'Id' de tipo int para {typeof(T).Name}.");

            // 2) Query base
            IQueryable<T> query = _context.Set<T>();

            // Includes de primer nivel (mismo estilo que tu repo)
            foreach (var navName in entityType.GetNavigations().Select(n => n.Name))
                query = query.Include(navName);


            // 3) Filtro por Id (int)
            var param = Expression.Parameter(typeof(T), "x");
            var prop = Expression.Property(param, "Id");             // PK = "Id"
            var body = Expression.Equal(prop, Expression.Constant(id));
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);

            return await query.FirstOrDefaultAsync(lambda);
        }


        // public async Task<T?> GetByIdIncludingRelations(int id)
        // {
        //     try
        //     {
        //         var query = _context.Set<T>().AsQueryable();

        //         // Obtener las propiedades de navegación de la entidad T
        //         var entityType = _context.Model.FindEntityType(typeof(T));
        //         var navigationProperties = entityType.GetNavigations().Select(nav => nav.Name);

        //         // Incluir cada propiedad de navegación
        //         foreach (var navigationProperty in navigationProperties)
        //         {
        //             query = query.Include(navigationProperty);
        //         }

        //         // Construir el nombre de la clave primaria siguiendo la convención 'Id' + Nombre de la Clase
        //         // var primaryKey = "Id" + typeof(T).Name;
        //         var primaryKey = "Id";

        //         // Crear una expresión lambda para filtrar por la clave primaria
        //         var parameter = Expression.Parameter(typeof(T), "x");
        //         var property = Expression.Property(parameter, primaryKey);
        //         var constant = Expression.Constant(id);
        //         var equals = Expression.Equal(property, constant);
        //         var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

        //         return await query.FirstOrDefaultAsync(lambda);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Manejo de la excepción
        //         // Aquí puedes registrar el error o manejarlo según tu política de manejo de errores.
        //         throw;
        //     }
        // }

        public async Task<IList<T>> GetAllIncludingSpecificRelations(
           Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
           bool asNoTracking = true)
        {
            IQueryable<T> query = _context.Set<T>();

            // Aplico los Include/ThenInclude que me pasen desde el caso de uso
            if (include != null)
                query = include(query);

            // Para solo lectura conviene NO trackear (mejor rendimiento)
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        /// <summary>
        /// Trae T incluyendo TODAS las relaciones de referencia (no colecciones) de forma recursiva,
        /// siguiendo la misma lógica de tus otros métodos (metadatos + Include(string)).
        /// </summary>
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

        /// <summary>
        /// Construye rutas de Include "A.B.C" SOLO para navegaciones de referencia (no colecciones),
        /// recursivamente hasta 'maxDepth'. Como mapeaste sin inversas, no deberías tener ciclos,
        /// igual se agrega una protección ligera por tipo.
        /// </summary>
        private IEnumerable<string> BuildReferenceIncludePaths(Type rootClrType, int maxDepth)
        {
            var paths = new HashSet<string>();
            var rootEntity = _context.Model.FindEntityType(rootClrType);
            if (rootEntity is null) return paths;

            void Recurse(IEntityType current, string basePath, int depth, HashSet<IEntityType> stack)
            {
                if (depth == 0) return;

                // Tomar SOLO referencias (no colecciones)
                var refNavs = current.GetNavigations()
                                     .Where(n => !n.IsCollection);

                foreach (var nav in refNavs)
                {
                    var nextPath = string.IsNullOrEmpty(basePath) ? nav.Name : $"{basePath}.{nav.Name}";
                    // agregamos la ruta actual
                    paths.Add(nextPath);

                    var target = nav.TargetEntityType;

                    // Protección mínima por tipo (no debería dispararse en tu mapeo unidireccional)
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
                // Si necesitás identidad sin trackeo para grafos grandes:
                // query = query.AsNoTrackingWithIdentityResolution();
                query = query.AsNoTracking();

            if (asSplitQuery)
                query = query.AsSplitQuery();  // evita cartesian explosion con muchos JOINs

            // Detectar PK por metadatos (soporta nombre distinto de "Id")
            var et = _context.Model.FindEntityType(typeof(T))
                     ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no está mapeado.");
            var pk = et.FindPrimaryKey()
                     ?? throw new InvalidOperationException($"Tipo {typeof(T).Name} no tiene PK definida.");

            if (pk.Properties.Count != 1)
                throw new NotSupportedException("Este helper soporta solo clave primaria simple.");

            var keyProp = pk.Properties[0];
            var keyName = keyProp.Name;                // p.ej. "Id", "OfertaId", etc.
            var keyClrType = keyProp.ClrType;          // p.ej. typeof(int), typeof(Guid)

            // Construyo x => x.<PK> == (cast)id
            var param = Expression.Parameter(typeof(T), "x");
            var propExpr = Expression.Property(param, keyName);

            // Convert id al tipo correcto (maneja int, long, Guid, etc.)
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