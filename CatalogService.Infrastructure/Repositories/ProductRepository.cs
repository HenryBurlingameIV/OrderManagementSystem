using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain;
using CatalogService.Domain.Exceptions;
using CatalogService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ValidationException = FluentValidation.ValidationException;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private CatalogDBContext _dbContext;
        public ProductRepository(CatalogDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Guid> CreateAsync(Product product)
        {
            Log.Information("Checking if product name is unique: {ProductName}", product.Name);
            var isUnique = await _dbContext.Products.FirstOrDefaultAsync(p => p.Name == product.Name) == null;
            if(!isUnique)
            {
                Log.Error("Product {@name} is not unique", product.Name);
                throw new ValidationException("Name must be unique");
            }
            await _dbContext.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            Log.Information("Product with ID {@ProductId} successfully added in database", product.Id);
            return product.Id;

        }

        public async Task DeleteAsync(Product product)
        {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            Product? product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            return product;
        }

        public async Task<Guid> UpdateAsync(Guid id, Product product)
        {
            Log.Information("Checking if product name is unique: {ProductName}", product.Name);
            var isUnique = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Name == product.Name && p.Id != id) == null;

            if (!isUnique)
            {
                Log.Error("Product {@name} is not unique", product.Name);
                throw new ValidationException("Name must be unique");
            }

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
            return id;
        }

    }
}
