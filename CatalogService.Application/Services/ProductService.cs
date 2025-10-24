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
        private IValidator<CreateProductRequest> _createValidator;
        private IValidator<UpdateProductRequest> _updateValidator;
        private readonly IValidator<GetPagindatedProductsRequest> _paginationValidator;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IEFRepository<Product, Guid> productRepository,
            IValidator<CreateProductRequest> createValidator,
            IValidator<UpdateProductRequest> updateValidator,
            IValidator<GetPagindatedProductsRequest> paginationValidator,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _paginationValidator = paginationValidator;
            _logger = logger;
        }

        public async Task<Guid> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
        {
            await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var nameIsUnique = await IsProductNameUnique(request.Name, null, cancellationToken);
            if (!nameIsUnique)
            {
                throw new ValidationException($"Product with name {request.Name} already exists");
            }

            var product = request.ToEntity();         
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
            return product.ToViewModel();
        }

        public async Task<PaginatedResult<ProductViewModel>> GetProductsPaginatedAsync(GetPagindatedProductsRequest request, CancellationToken cancellationToken)
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


            Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = BuildOrderByExpression(request.SortBy, request.Descending);

            return await _productRepository
                .GetPaginated(
                    request: pagination,
                    orderBy: orderBy,
                    filter: filter,
                    selector: (p) => p.ToViewModel(),                       
                    ct: cancellationToken
                );
        }

        private Func<IQueryable<Product>, IOrderedQueryable<Product>> BuildOrderByExpression(string? sortBy, bool descending)
        {
            if(string.IsNullOrEmpty(sortBy))
            {
                return (q) => q.OrderByDescending(p => p.CreatedDateUtc);
            }
            var normalizedSortBy = sortBy?.ToLower().Trim();

            return normalizedSortBy switch
            {
                "name" => descending
                    ? q => q.OrderByDescending(p => p.Name)
                    : q => q.OrderBy(p => p.Name),
                "category" => descending
                    ? q => q.OrderByDescending(p => p.Category)
                    : q => q.OrderBy(p => p.Category),
                "price" => descending
                    ? q => q.OrderByDescending(p => p.Price)
                    : q => q.OrderBy(p => p.Price),
                "created" => descending
                    ? q => q.OrderByDescending(p => p.CreatedDateUtc)
                    : q => q.OrderBy(p => p.CreatedDateUtc),
                _ => (q) => q.OrderByDescending(p => p.CreatedDateUtc)
            };
        }

        public async Task<Guid> UpdateProductAsync(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken)
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

            product.UpdateFromRequest(request);
            await _productRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product with ID {@ProductId} successfully updated", productId);
            return productId;
        }

        public async Task<ProductViewModel> ReserveProductAsync(Guid productId, int quantity, CancellationToken cancellationToken)
        {
            if (quantity <= 0)
                throw new ValidationException("Quantity must be positive");

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new NotFoundException($"Product with ID {productId} not found.");

            if (product.Quantity < quantity)
            {
                throw new ValidationException($"Not enough quantity. Available: {product.Quantity}");
            }

            product.Quantity -= quantity;
            product.UpdatedDateUtc = DateTime.UtcNow;

            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Reserved {Quantity} of product {@ProductId}", quantity, productId);

            return product.ToViewModel();
        }

        public async Task<ProductViewModel> ReleaseProductAsync(Guid productId, int quantity, CancellationToken cancellationToken)
        {
            if (quantity <= 0)
                throw new ValidationException("Quantity must be positive");

            var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new NotFoundException($"Product with ID {productId} not found.");

            product.Quantity += quantity;
            product.UpdatedDateUtc = DateTime.UtcNow;

            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Released {Quantity} of product {@ProductId}", quantity, productId);

            return product.ToViewModel();
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

        public async Task<bool> IsProductNameUnique(string name, Guid? excludedProduct = null, CancellationToken ct = default)
        {
            var normailizedName = name.ToLower().Trim();
            return !await _productRepository.ExistsAsync(p => 
                p.Name.ToLower().Trim() == normailizedName && (excludedProduct == null || p.Id != excludedProduct.Value), 
                ct);
        }

    }
}
