using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.DataAccess
{
    public class Repository<TEntity, TKey> : IEFRepository<TEntity, TKey> where TEntity : class
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct) =>
            await _dbSet.FindAsync(new object[]{id}, ct);


        public void Update(TEntity item) => 
            _dbSet.Update(item);

        public void Delete(TEntity entity) =>
            _dbSet.Remove(entity);

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken ct)
        {
            return (await _dbSet.AddAsync(entity, ct)).Entity;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct) =>
            await _dbContext.SaveChangesAsync(ct);


        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct) =>
             predicate is null
                ? await _dbSet.AnyAsync()
                : await _dbSet.AnyAsync(predicate, ct);
        public async Task<TEntity?> FindAsync(object[] keyValues, CancellationToken ct)
        {
            return await _dbSet.FindAsync(keyValues, ct);
        }

        public async Task<PaginatedResult<TResult>> GetPaginated<TResult>(
            PaginationRequest request,
            Expression<Func<TEntity, TResult>> selector,
            Expression <Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            CancellationToken ct = default) where TResult : class
        {
            var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();

            if(filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync(ct);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(selector)
                .ToListAsync(ct);

            return new PaginatedResult<TResult>(
                items, totalCount, request.PageNumber, request.PageSize);           
        }

        public async Task<PaginatedResult<TEntity>> GetPaginated(
            PaginationRequest request,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync(ct);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            return new PaginatedResult<TEntity>(
                items, totalCount, request.PageNumber, request.PageSize);
        }

        public async Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTraсking = true,
            CancellationToken ct = default)
        {
            var query = asNoTraсking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await query.FirstOrDefaultAsync(ct);

        }

        public async Task<TResult?> GetFirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTraсking = true,
            CancellationToken ct = default)
        {
            var query = asNoTraсking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await query.Select(selector).FirstOrDefaultAsync(ct);
        }



    }
}
