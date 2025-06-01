using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;


namespace CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private IRepository<Product> _repository;
        private IValidator<ProductCreateRequest> _createValidator;

        public ProductService(IRepository<Product> repo, IValidator<ProductCreateRequest> createValidator)
        {
            _repository = repo;
            _createValidator = createValidator;
        }

        public async Task<Guid> CreateProductAsync(ProductCreateRequest request)
        {
            var product = CreateProductFromRequest(request);
            return await _repository.CreateAsync(product!);
        }

        public Task<ProductViewModel> GetProductByIdAsync(Guid productId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> UpdateProductAsync(ProductUpdateRequest request)
        {
            throw new NotImplementedException();
        }

        public Product? CreateProductFromRequest(ProductCreateRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if(!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }    
            return new Product()
            {
                Name = request.Name,
                Description = request.Description,
                Quantity = request.Quantity,
                Price = request.Price,
                Category = request.Category,
                CreatedDateUtc = DateTime.UtcNow,
                UpdatedDateUtc = DateTime.UtcNow
            };
        }

        public ProductViewModel CreateProductViewModel(Product product)
        {
            return new ProductViewModel()
            {
                Name = product.Name,
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price,
                Category = product.Category,
                CreatedDateUtc = product.CreatedDateUtc,
                UpdatedDateUtc = product.UpdatedDateUtc
            };
        }

    }
}
