using OrderService.Infrastructure.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Contracts
{
    public interface ICatalogServiceClient
    {
        Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateProductInventoryAsync(Guid id, int deltaQuantity, CancellationToken cancellationToken);


    }
}
