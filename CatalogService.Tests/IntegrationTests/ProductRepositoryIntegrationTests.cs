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
    public class ProductRepositoryIntegrationTests : IAsyncLifetime
    {
        private CatalogDBContext _context;
        private IRepository<Product> _productRepository;

        public async Task InitializeAsync()
        {
            _context = await GetDBContext();
            _productRepository = new ProductRepository(_context);
        }

        public Task DisposeAsync()
        {
            return _context.DisposeAsync().AsTask();
        }
        private async Task<CatalogDBContext> GetDBContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new CatalogDBContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        private Product CreateTestProduct()
        {
            return new Product()
            {
                Id = Guid.NewGuid(),
                Name = "test1",
                Description = "test1",
                Category = "test1",
                Price = 100,
                Quantity = 1,
                UpdatedDateUtc = DateTime.UtcNow,
                CreatedDateUtc = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnIdAndSaveProduct()
        {
            //Arrange
            Product product = CreateTestProduct();

            //Act
            var actualId = await _productRepository.CreateAsync(product, CancellationToken.None);

            //Assert
            Assert.Equal(product.Id, actualId);
            var savedProduct = await _context.Products.FindAsync(product.Id);
            Assert.NotNull(savedProduct);
            Assert.Equivalent(product, savedProduct);
        }

        
    }
    


}
