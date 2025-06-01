using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Domain;
using CatalogService.Domain.Exceptions;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using FluentValidation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ValidationException = FluentValidation.ValidationException;


namespace CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private IRepository<Product> _productRepository;
        private IValidator<ProductCreateRequest> _createValidator;

        public ProductService(IRepository<Product> productRepository, IValidator<ProductCreateRequest> createValidator)
        {
            _productRepository = productRepository;
            _createValidator = createValidator;
        }

        public async Task<Guid> CreateProductAsync(ProductCreateRequest request)
        {
            var product = CreateProductFromRequest(request);
            return await _productRepository.CreateAsync(product!);
        }

        public async Task<ProductViewModel> GetProductByIdAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            return CreateProductViewModel(product);
        }

        public async Task<Guid> UpdateProductAsync(Guid productId, ProductUpdateRequest request)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            product.Name = request!.Name!;
            product.Description = request!.Description!;
            product.Category = request!.Category!;
            product.Price = request!.Price.Value;
            product.UpdatedDateUtc = DateTime.UtcNow;
            product.Quantity = request!.Quantity.Value;
            product.Category = request!.Category!;

            await _productRepository.UpdateAsync(productId, product);
            return productId;
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
                Id = product.Id,
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
