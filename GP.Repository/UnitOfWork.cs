using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GP.Core.Entities;
using GP.Core.Interfaces;
using GP.Core.Repositories.Contract;
using GP.Repository.Data;

namespace GP.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GPContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(GPContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
                _repositories[type] = new GenericRepository<T>(_dbContext);

            return (IGenericRepository<T>)_repositories[type];
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
