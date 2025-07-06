using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Shared.Contracts;
using Serilog;
using ValidationException = FluentValidation.ValidationException;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private CatalogDBContext _dbContext;
        private IValidator<Product> _productValidator;

        public ProductRepository(CatalogDBContext dbContext, IValidator<Product> productValidator)
        {
            _dbContext = dbContext;
            _productValidator = productValidator;
        }

        public async Task<Guid> CreateAsync(Product product, CancellationToken cancellationToken)
        {
            var validationResult = await _productValidator.ValidateAsync(product, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            await _dbContext.AddAsync(product, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            Log.Information("Product with ID {@ProductId} successfully created and added in database", product.Id);
            return product.Id;
        }


        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Product? product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            return product;
        }

        public async Task<Guid> UpdateAsync(Product product, CancellationToken cancellationToken)
        {
            var validationResult = await _productValidator.ValidateAsync(product, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            _dbContext.Entry(product).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return product.Id;
        }

        public async Task DeleteAsync(Product product, CancellationToken cancellationToken)
        {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}