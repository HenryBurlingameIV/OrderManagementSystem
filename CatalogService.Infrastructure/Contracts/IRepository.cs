using CatalogService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Infrastructure.Contracts
{
    public interface IRepository<T>
    {
        Task<Guid> CreateAsync(T item);

        Task<Guid> UpdateAsync(Guid id, T update);

        Task<T?> GetByIdAsync(Guid id);

        Task DeleteAsync(T item);

    }
}
