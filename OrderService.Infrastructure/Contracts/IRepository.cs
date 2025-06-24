using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Contracts
{
    public interface IRepository<T>
    {
        Task<Guid> CreateAsync(T item, CancellationToken cancellationToken);
        Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateAsync(T item, CancellationToken cancellationToken);       
    }
}
