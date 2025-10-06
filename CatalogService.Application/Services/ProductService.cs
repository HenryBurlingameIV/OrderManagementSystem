using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using OrderManagementSystem.Shared.Exceptions;
using Serilog;
using ValidationException = FluentValidation.ValidationException;


namespace CatalogService.Application.Services
{
    public class ProductService : IProductService
    {
        private IEFRepository<Product, Guid> _productRepository;
        private IValidator<ProductCreateRequest> _createValidator;
        private IValidator<ProductUpdateRequest> _updateValidator;
        private IValidator<ProductUpdateQuantityRequest> _quantityValidator;
        private readonly IValidator<GetPagedProductsRequest> _paginationValidator;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IEFRepository<Product, Guid> productRepository,
            IValidator<ProductCreateRequest> createValidator,
            IValidator<ProductUpdateRequest> updateValidator,
            IValidator<ProductUpdateQuantityRequest> quantityValidator,
            IValidator<GetPagedProductsRequest> paginationValidator,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _quantityValidator = quantityValidator;
            _paginationValidator = paginationValidator;
            _logger = logger;
        }

        public async Task<Guid> CreateProductAsync(ProductCreateRequest request, CancellationToken cancellationToken)
        {
            await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var nameIsUnique = await IsProductNameUnique(request.Name, null, cancellationToken);
            if (!nameIsUnique)
            {
                throw new ValidationException($"Product with name {request.Name} already exists");
            }

            var product = CreateProductFromRequest(request);
          
            var id = (await _productRepository.InsertAsync(product!, cancellationToken)).Id;
            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product with ID {@ProductId} created and saved in database", product.Id);
            return id;
        }

        public async Task<ProductViewModel> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            _logger.LogInformation("Product with ID {@productId} successfully found", productId);
            return CreateProductViewModel(product);
        }

        public async Task<PaginatedResult<ProductViewModel>> GetProductsPaginatedAsync(GetPagedProductsRequest request, CancellationToken cancellationToken)
        {
            await _paginationValidator.ValidateAndThrowAsync(request, cancellationToken);
            var pagination = new PaginationRequest()
            {
                PageSize = request.PageSize,
                PageNumber = request.PageNumber,
            };

            Expression<Func<Product, bool>>? filter = string.IsNullOrEmpty(request.Search)
                ? null
                : (p) => p.Name.ToLower().Contains(request.Search.ToLower()) 
                || p.Description.ToLower().Contains(request.Search.ToLower())
                || p.Category.ToLower().Contains(request.Search.ToLower());

            Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = request.SortBy?.ToLower() switch
            {
                "name" => q => q.OrderBy(p => p.Name),
                "category" => q => q.OrderBy(p => p.Category),
                "price" => q => q.OrderBy(p => p.Price),
                "created" => q => q.OrderBy(p => p.CreatedDateUtc),
                _ => null
            };

            return await _productRepository
                .GetPaginated(
                    request: pagination,
                    orderBy: orderBy,
                    filter: filter,
                    selector: (p) => new ProductViewModel(
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Category,
                        p.Price,
                        p.Quantity),                       
                    ct: cancellationToken
                );
        }

        public async Task<Guid> UpdateProductAsync(Guid productId, ProductUpdateRequest request, CancellationToken cancellationToken)
        {
            await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            if(request.Name != product.Name && request.Name != null)
            {
                var nameIsUnique = await IsProductNameUnique(request.Name!, productId, cancellationToken);
                if (!nameIsUnique)
                {
                    throw new ValidationException($"Product with name {request.Name} already exists");
                }
            }

            UpdateProductFromRequest(request, product);
            await _productRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {@ProductId} successfully updated", productId);
            return productId;
        }
        public async Task<ProductViewModel> UpdateProductQuantityAsync(Guid productId, ProductUpdateQuantityRequest request, CancellationToken cancellationToken)
        {
            await _quantityValidator.ValidateAndThrowAsync(request);

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            _logger.LogInformation("Product with ID {@ProductId} successfully found", productId);

            if (product.Quantity + request.DeltaQuantity < 0)
            {
                throw new ValidationException($"Product '{product.Name}' does not have enough quantity available. Requested: {Math.Abs(request.DeltaQuantity)}, Available: {product.Quantity}.");
            }
            product.Quantity += request.DeltaQuantity;
            product.UpdatedDateUtc = DateTime.UtcNow;
            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{@Product} quantity successfully updated", product);
            return CreateProductViewModel(product);

        }
        public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }
            _logger.LogInformation("Product with ID {@ProductId} successfully found", productId);
            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync(cancellationToken);
        }

        public Product CreateProductFromRequest(ProductCreateRequest request)
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
