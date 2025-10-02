using Microsoft.EntityFrameworkCore.Query;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Contracts
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Delete(TEntity entity);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
        Task<TEntity?> FindAsync(object[] keyValues, CancellationToken ct);
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);

        Task<PaginatedResult<TResult>> GetPaginated<TResult>(
            PaginationRequest request,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool asNoTracking = true,
            CancellationToken ct = default) where TResult : class;

        Task<PaginatedResult<TEntity>> GetPaginated(
            PaginationRequest request,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default);

    }
}
