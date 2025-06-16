using CatalogService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogService.Infrastructure.Contracts
{
    public interface IRepository<T>
    {
        Task<Guid> CreateAsync(T item, CancellationToken cancellationToken);

        Task<Guid> UpdateAsync(T update, CancellationToken cancellationToken);

        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task DeleteAsync(T item, CancellationToken cancellationToken);

    }
}
