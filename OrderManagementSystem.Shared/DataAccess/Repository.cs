using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.DataAccess
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public async Task<TEntity?> FindAsync(object[] keyValues, CancellationToken ct)
        {
            return await _dbSet.FindAsync(keyValues, ct);
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken ct)
        {
            return (await _dbSet.AddAsync(entity, ct)).Entity;
        }

        public void Delete(TEntity entity, CancellationToken ct) =>
            _dbSet.Remove(entity);

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct) =>
             predicate is null
                ? await _dbSet.AnyAsync()
                : await _dbSet.AnyAsync(predicate, ct);

        public async Task<int> SaveChangesAsync() =>
            await _dbContext.SaveChangesAsync();

    }
}
