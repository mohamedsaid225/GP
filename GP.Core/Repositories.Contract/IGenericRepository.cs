using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GP.Core.Entities;

namespace GP.Core.Repositories.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // ─── Get ────────────────────────────────────────────
        Task<T?> GetByIdAsync(int id);

        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

        Task<IEnumerable<T>> GetAllAsync();

        // ✅ ضيف دي
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // ✅ وعدّل دي
        Task<T?> FindOneAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        // ─── Check ──────────────────────────────────────────
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        // ─── Write ──────────────────────────────────────────
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}