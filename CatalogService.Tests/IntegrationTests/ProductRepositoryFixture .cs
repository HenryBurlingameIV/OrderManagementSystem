using CatalogService.Domain;
using CatalogService.Infrastructure.Contracts;
using CatalogService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CatalogService.Infrastructure.Repositories;

namespace CatalogService.Tests.IntegrationTests
{
    public class ProductRepositoryFixture : IDisposable 
    {
        public CatalogDBContext Context { get; }
        public IRepository<Product> ProductRepository { get; }
        public ProductRepositoryFixture() 
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            Context = new CatalogDBContext(options);
            ProductRepository = new ProductRepository(Context);                          
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
