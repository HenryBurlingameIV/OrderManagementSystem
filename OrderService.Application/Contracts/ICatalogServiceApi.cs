using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Contracts
{
    public interface ICatalogServiceApi
    {
        Task<ProductDto?> ReserveProductAsync(Guid id, int quantity, CancellationToken cancellationToken);
        Task ReleaseProductAsync(Guid id, int quantity, CancellationToken cancellationToken);
    }
}
