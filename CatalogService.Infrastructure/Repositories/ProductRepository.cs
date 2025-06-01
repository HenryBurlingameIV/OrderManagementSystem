using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain;
using CatalogService.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

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
            await _dbContext.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return product.Id;

        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            Product? product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            return product;
        }

        public Task<Guid> UpdateAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
