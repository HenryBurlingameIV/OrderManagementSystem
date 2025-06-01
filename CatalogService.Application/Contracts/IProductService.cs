using CatalogService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Contracts
{
    public interface IProductService
    {
        Task<Guid> CreateProductAsync(ProductCreateRequest request);

        Task<ProductViewModel> GetProductByIdAsync(Guid productId);

        Task<Guid> UpdateProductAsync(Guid productId, ProductUpdateRequest request);

    }
}
