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
        private IValidator<ProductUpdateRequest> _updateValidator;
        private IValidator<ProductUpdateQuantityRequest> _quantityValidator;

        public ProductService(IRepository<Product> productRepository, 
            IValidator<ProductCreateRequest> createValidator, 
            IValidator<ProductUpdateRequest> updateValidator,
            IValidator<ProductUpdateQuantityRequest> quantityValidator)
        {
            _productRepository = productRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _quantityValidator = quantityValidator;
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

            UpdateProductFromRequest(request, product);
            await _productRepository.UpdateAsync(productId, product);
            return productId;
        }
        public async Task UpdateProductQuantityAsync(Guid productId, ProductUpdateQuantityRequest request)
        {
            var validationResult = _quantityValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            if (product.Quantity < request.Quantity)
            {
                throw new ValidationException($"Product '{product.Name}' does not have enough quantity available. Requested: {request.Quantity}, Available: {product.Quantity}.");
            }
            product.Quantity -= request.Quantity;
            product.UpdatedDateUtc = DateTime.Now;
            await _productRepository.UpdateAsync(productId, product);
           
        }
        public async Task DeleteProductAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException("$Product with ID {productId} not found.");
            }
            await _productRepository.DeleteAsync(product);
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
                CreatedDateUtc = DateTime.Now,
                UpdatedDateUtc = DateTime.Now
            };
        }

        public void UpdateProductFromRequest(ProductUpdateRequest request, Product productToUpdate)
        {
            var validationResult = _updateValidator.Validate(request);
            if(!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            productToUpdate.Name = request.Name ?? productToUpdate.Name;
            productToUpdate.Description = request.Description ?? productToUpdate.Description;
            productToUpdate.Category = request.Category ?? productToUpdate.Category;
            productToUpdate.Price = request.Price ?? productToUpdate.Price;
            productToUpdate.Quantity = request.Quantity ?? productToUpdate.Quantity;
            productToUpdate.UpdatedDateUtc = DateTime.Now;

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
