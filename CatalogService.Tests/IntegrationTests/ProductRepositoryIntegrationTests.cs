using CatalogService.Application.Contracts;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Tests.IntegrationTests
{
    public  class ProductRepositoryIntegrationTests
    {
        private IRepository<Product> _productRepository;

        public ProductRepositoryIntegrationTests()
        {
            _productRepository = new ProductRepository(GetDBContext());
        }

        private CatalogDBContext GetDBContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase("testdb")
                .Options;
            var context =  new CatalogDBContext(options);
            context.Database.EnsureCreated();

            return context;
        }
       
    }
}
