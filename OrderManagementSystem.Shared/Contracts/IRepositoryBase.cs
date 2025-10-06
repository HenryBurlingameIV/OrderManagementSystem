using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Contracts
{
    public interface IRepositoryBase<TEntity, in TKey> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);
        Task<TEntity> InsertAsync(TEntity item, CancellationToken cancellationToken);

        void Update(TEntity item);
        void Delete(TEntity item);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
