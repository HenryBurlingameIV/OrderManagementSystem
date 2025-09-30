using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Domain;
using FluentValidation;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Exceptions;
using Serilog;
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
            var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            var nameIsUnique = await IsProductNameUnique(request.Name, null, cancellationToken);
            if (!nameIsUnique)
            {
                throw new ValidationException($"Product with name {request.Name} already exists");
            }

            var product = CreateProductFromRequest(request);
          
            var id = (await _productRepository.InsertAsync(product!, cancellationToken)).Id;
            await _productRepository.SaveChangesAsync(cancellationToken);
            return id;
        }

        public async Task<ProductViewModel> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productRepository.FindAsync(new object[] {productId }, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);
            return CreateProductViewModel(product);
        }

        public async Task<Guid> UpdateProductAsync(Guid productId, ProductUpdateRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var product = await _productRepository.FindAsync(new object []{ productId }, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            if(request.Name != product.Name)
            {
                var nameIsUnique = await IsProductNameUnique(request.Name!, productId, cancellationToken);
                if (!nameIsUnique)
                {
                    throw new ValidationException($"Product with name {request.Name} already exists");
                }
            }

            UpdateProductFromRequest(request, product);
            await _productRepository.SaveChangesAsync(cancellationToken);

            Log.Information("Product with ID {@productId} successfully updated", productId);
            return productId;
        }
        public async Task<ProductViewModel> UpdateProductQuantityAsync(Guid productId, ProductUpdateQuantityRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _quantityValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var product = await _productRepository.FindAsync(new object[] { productId }, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);

            if (product.Quantity + request.DeltaQuantity < 0)
            {
                throw new ValidationException($"Product '{product.Name}' does not have enough quantity available. Requested: {Math.Abs(request.DeltaQuantity)}, Available: {product.Quantity}.");
            }
            product.Quantity += request.DeltaQuantity;
            product.UpdatedDateUtc = DateTime.UtcNow;
            await _productRepository.SaveChangesAsync(cancellationToken);
            Log.Information("{@Product} quantity successfully updated", product);
            return CreateProductViewModel(product);

        }
        public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productRepository.FindAsync(new object[] {productId}, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            Log.Information("Product with ID {@productId} successfully found", productId);
            _productRepository.Delete(product, cancellationToken);
            await _productRepository.SaveChangesAsync(cancellationToken);
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
                CreatedDateUtc = DateTime.UtcNow,
                UpdatedDateUtc = DateTime.UtcNow
            };
        }

        public void UpdateProductFromRequest(ProductUpdateRequest request, Product productToUpdate)
        {
            productToUpdate.Name = request.Name ?? productToUpdate.Name;
            productToUpdate.Description = request.Description ?? productToUpdate.Description;
            productToUpdate.Category = request.Category ?? productToUpdate.Category;
            productToUpdate.Price = request.Price ?? productToUpdate.Price;
            productToUpdate.Quantity = request.Quantity ?? productToUpdate.Quantity;
            productToUpdate.UpdatedDateUtc = DateTime.UtcNow;

        }

        public ProductViewModel CreateProductViewModel(Product product)
        {
            return new ProductViewModel(
                product.Id, 
                product.Name, 
                product.Description, 
                product.Category, 
                product.Price, 
                product.Quantity);

        }

        public async Task<bool> IsProductNameUnique(string name, Guid? excludedProduct = null, CancellationToken ct = default)
        {
            var normailizedName = name.ToLower().Trim();
            return !await _productRepository.ExistsAsync(p => 
                p.Name.ToLower().Trim() == normailizedName && (excludedProduct == null || p.Id != excludedProduct.Value), 
                ct);
        }

    }
}
