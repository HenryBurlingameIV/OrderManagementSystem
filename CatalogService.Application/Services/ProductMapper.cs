using CatalogService.Application.DTO;
using CatalogService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Services
{
    public static class ProductMapper
    {
        public static Product ToEntity(this ProductCreateRequest request)
        {
            var createdAt = DateTime.UtcNow;
            return new Product()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Quantity = request.Quantity,
                Price = request.Price,
                Category = request.Category,
                CreatedDateUtc = createdAt,
                UpdatedDateUtc = createdAt
            };
        }

        public static ProductViewModel ToViewModel(this Product product) =>
            new ProductViewModel(
                product.Id,
                product.Name,
                product.Description,
                product.Category,
                product.Price,
                product.Quantity);

        public static void UpdateFromRequest(this Product product, ProductUpdateRequest request)
        {
            product.Name = request.Name ?? product.Name;
            product.Description = request.Description ?? product.Description;
            product.Category = request.Category ?? product.Category;
            product.Price = request.Price ?? product.Price;
            product.Quantity = request.Quantity ?? product.Quantity;
            product.UpdatedDateUtc = DateTime.UtcNow;
        }
    }
}
