using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;


namespace CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private IRepository<Product> _repository;

        public ProductService(IRepository<Product> repo)
        {
            _repository = repo;
        }

        public async Task<Guid> CreateProductAsync(ProductCreateRequest request)
        {
            var product = new Product()
            {
                Name = request.Name,
                Description = request.Description,
                Quantity = request.Quantity,
                Price = request.Price,
                Category = request.Category,
                CreatedDateUtc = DateTime.UtcNow,
                UpdatedDateUtc = DateTime.UtcNow
            };
            return await _repository.CreateAsync(product);
        }

        public Task<ProductViewModel> GetProductByIdAsync(Guid productId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> UpdateProductAsync(ProductUpdateRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
