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
        void Delete(TEntity entity, CancellationToken ct);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
        Task<TEntity?> FindAsync(object[] keyValues, CancellationToken ct);
        Task<TEntity> InsertAsync(TEntity entity, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);

    }
}
