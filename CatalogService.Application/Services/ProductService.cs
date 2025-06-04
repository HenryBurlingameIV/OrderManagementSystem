using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Domain;
using CatalogService.Domain.Exceptions;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using FluentValidation;
using Serilog;
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

        public async Task<Guid> CreateProductAsync(ProductCreateRequest request, CancellationToken cancellationToken)
        {
            Log.Information("Starting to create product from {@Request}", request);
            var product = CreateProductFromRequest(request);
            Log.Information("Product with ID {@ProductId} successfully created", product!.Id);
            return await _productRepository.CreateAsync(product!, cancellationToken);
        }

        public async Task<ProductViewModel> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            Log.Information("Trying to get product with ID {@productId}", productId);
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                Log.Warning("Product with ID {@productId} not found");
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);
            return CreateProductViewModel(product);
        }

        public async Task<Guid> UpdateProductAsync(Guid productId, ProductUpdateRequest request, CancellationToken cancellationToken)
        {
            Log.Information("Trying to get product with ID {@productId}", productId);
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                Log.Warning("Product with ID {@productId} not found", productId);
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Starting to update product from {@Request}", request);
            UpdateProductFromRequest(request, product);
            await _productRepository.UpdateAsync(productId, product, cancellationToken);
            return productId;
        }
        public async Task UpdateProductQuantityAsync(Guid productId, ProductUpdateQuantityRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _quantityValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                Log.Error("Validation on updating product quantity failed with {@Errors}", validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }
            Log.Information("Trying to get product with ID {@productId}", productId);
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                Log.Warning("Product with ID {@productId} not found");
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);

            if (product.Quantity < request.Quantity)
            {
                Log.Error("Product {@product.Name} does not have enough quantity available. Requested: {@request.Quantity}, Available: {product.Quantity}.", product.Name, request.Quantity, product.Quantity);
                throw new ValidationException($"Product '{product.Name}' does not have enough quantity available. Requested: {request.Quantity}, Available: {product.Quantity}.");
            }
            product.Quantity -= request.Quantity;
            product.UpdatedDateUtc = DateTime.Now;
            await _productRepository.UpdateAsync(productId, product, cancellationToken);
           
        }
        public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken)
        {
            Log.Information("Trying to get product with ID {@productId}", productId);
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                Log.Warning("Product with ID {@productId} not found");
                throw new NotFoundException("$Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);
            await _productRepository.DeleteAsync(product, cancellationToken);
        }

        public Product? CreateProductFromRequest(ProductCreateRequest request)
        {
            var validationResult = _createValidator.Validate(request);
            if(!validationResult.IsValid)
            {
                Log.Error("Validation on creating product failed with {@Errors}", validationResult.Errors);
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
                Log.Error("Validation on updating product failed with {@Errors}", validationResult.Errors);
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
