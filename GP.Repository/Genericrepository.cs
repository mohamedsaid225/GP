using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GP.Core.Entities;
using GP.Core.Repositories.Contract;
using GP.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace GP.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly GPContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(GPContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        // ─── Get ────────────────────────────────────────────
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            // بنلف على كل الـ Includes اللي مبعوتة ونضيفها للـ Query
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // بنفلتر بالـ Id في الآخر
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // ─── Check ──────────────────────────────────────────
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate is null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);
        }

        // ─── Write ──────────────────────────────────────────
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(predicate);
        }

        //public Task<IEnumerable<T>> GetAllAsync(int id, params Expression<Func<T, object>>[] includes)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<T?> IGenericRepository<T>.GetAllAsync(int id, params Expression<Func<T, object>>[] includes)
        //{
        //    throw new NotImplementedException();
        //}
    }
}