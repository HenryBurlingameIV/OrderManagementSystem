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
        Task<Guid> CreateAsync(Guid id);

        Task<Guid> UpdateAsync(Guid id);

        Task<T> GetByIdAsync(Guid id);

    }
}
