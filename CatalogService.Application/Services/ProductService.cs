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
using Microsoft.EntityFrameworkCore;
using Serilog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ValidationException = FluentValidation.ValidationException;


namespace CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private IRepository<Product> _productRepository;
        private IValidator<Product> _productValidator;


        public ProductService(IRepository<Product> productRepository,
            IValidator<Product> productValidator)
        {
            _productRepository = productRepository;
            _productValidator = productValidator;

        }

        public async Task<Guid> CreateProductAsync(ProductCreateRequest request, CancellationToken cancellationToken)
        {
            var product = CreateProductFromRequest(request);
            await _productValidator.ValidateAndThrowAsync(product);
            Log.Information("Product with ID {@ProductId} successfully created", product!.Id);
            return await _productRepository.CreateAsync(product!, cancellationToken);
        }

        public async Task<ProductViewModel> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);
            return CreateProductViewModel(product);
        }

        public async Task<Guid> UpdateProductAsync(Guid productId, ProductUpdateRequest request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            UpdateProductFromRequest(request, product);
            await _productValidator.ValidateAndThrowAsync(product);
            await _productRepository.UpdateAsync(productId, product, cancellationToken);
            Log.Information("Product with ID {@productId} successfully updated", productId);
            return productId;
        }
        public async Task UpdateProductQuantityAsync(Guid productId, ProductUpdateQuantityRequest request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);

            if (product.Quantity < request.Quantity)
            {
                throw new ValidationException($"Product '{product.Name}' does not have enough quantity available. Requested: {request.Quantity}, Available: {product.Quantity}.");
            }
            product.Quantity -= request.Quantity;
            product.UpdatedDateUtc = DateTime.Now;
            await _productRepository.UpdateAsync(productId, product, cancellationToken);
            Log.Information("{@Product} quantity successfully updated", product);

        }
        public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);
            await _productRepository.DeleteAsync(product, cancellationToken);
        }

        public Product CreateProductFromRequest(ProductCreateRequest request)
        {  
            return new Product()
            {
                Id = Guid.NewGuid(),
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
