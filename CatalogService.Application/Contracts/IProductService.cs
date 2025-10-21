using CatalogService.Application.DTO;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Contracts
{
    public interface IProductService
    {
        Task<Guid> CreateProductAsync(ProductCreateRequest request, CancellationToken cancellationToken);

        Task<ProductViewModel> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken);

        Task<PaginatedResult<ProductViewModel>> GetProductsPaginatedAsync(GetPagindatedProductsRequest request, CancellationToken cancellationToken);

        Task<Guid> UpdateProductAsync(Guid productId, ProductUpdateRequest request, CancellationToken cancellationToken);

        Task<ProductViewModel> UpdateProductQuantityAsync(Guid productId, ProductUpdateQuantityRequest request, CancellationToken cancellationToken);

        Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken);

    }
}
