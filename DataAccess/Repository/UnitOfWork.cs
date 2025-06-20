using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using DataAccess.IRepository;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        protected readonly DbBolsaTrabajoContext _context;
        private IDbContextTransaction _transaction;



        //tengo que crear _transaction

        public UnitOfWork(DbBolsaTrabajoContext context)
        {
            _context = context;

  
        }

        
        public IGenericRepository<T> GenericRepository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }



        public async Task BeginTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    _transaction = await _context.Database.BeginTransactionAsync();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _transaction?.Dispose();
                _transaction = null;

            }


        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }


        public void Dispose()
        {
            //el dispose es para liberar recursos

            // Dispose of the context

            _context.Dispose();
        }

        public async Task RollbackAsync()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }



    }
}