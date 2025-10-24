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
        Task<Guid> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);

        Task<ProductViewModel> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken);

        Task<PaginatedResult<ProductViewModel>> GetProductsPaginatedAsync(GetPagindatedProductsRequest request, CancellationToken cancellationToken);

        Task<Guid> UpdateProductAsync(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken);

        Task<ProductViewModel> ReserveProductAsync(Guid productId, int quantity, CancellationToken cancellationToken);
        Task<ProductViewModel> ReleaseProductAsync(Guid productId, int quantity, CancellationToken cancellationToken);

        Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken);

    }
}
