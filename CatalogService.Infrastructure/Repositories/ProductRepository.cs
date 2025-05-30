using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain;
using CatalogService.Infrastructure.Contracts;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private CatalogDBContext _dbContext;
        public ProductRepository(CatalogDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<Guid> CreateAsync(Product product)
        {
            throw new NotImplementedException();
        }

        public Task<Product> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> UpdateAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
