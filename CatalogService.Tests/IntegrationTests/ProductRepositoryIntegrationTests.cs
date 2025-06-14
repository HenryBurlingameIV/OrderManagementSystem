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
        private CatalogDBContext? _context;
        private IRepository<Product>? _productRepository;

        public async Task InitializeAsync()
        {
            _context = await GetDBContextAsync();
            _productRepository = new ProductRepository(_context);
        }

        public Task DisposeAsync()
        {
            return _context!.DisposeAsync().AsTask();
        }
        private async Task<CatalogDBContext> GetDBContextAsync()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new CatalogDBContext(options);
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        private Product CreateTestProduct(int variantNumber)
        {
            return new Product()
            {
                Id = Guid.NewGuid(),
                Name =$"test{variantNumber}",
                Description = $"test{variantNumber}",
                Category = $"test{variantNumber}",
                Price = variantNumber*100,
                Quantity = variantNumber*1,
                UpdatedDateUtc = DateTime.UtcNow,
                CreatedDateUtc = DateTime.UtcNow
            };
        }

        private async Task<IEnumerable<Product>> AddTestProducts(int count)
        {
            List<Product> products = new List<Product>();
            for (int i = 1; i <= count; i++)
                products.Add(CreateTestProduct(i));

            await _context!.AddRangeAsync(products);
            await _context.SaveChangesAsync();
            return products;
        }

        private void AssertProductsEqual(Product expected, Product actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Category, actual.Category);
            Assert.Equal(expected.Price, actual.Price);
            Assert.Equal(expected.Quantity, actual.Quantity);
            Assert.Equal(expected.CreatedDateUtc, actual.CreatedDateUtc);
            Assert.Equal(expected.UpdatedDateUtc, actual.UpdatedDateUtc);
        }

        [Fact]
        public async Task Should_ReturnIdAndPersistProduct_WhenCreateNewProduct()
        {
            //Arrange
            var product = CreateTestProduct(1);

            //Act
            var actualId = await _productRepository!.CreateAsync(product, CancellationToken.None);

            //Assert
            Assert.Equal(product.Id, actualId);
            var savedProduct = await _context!.Products.FindAsync(product.Id);
            Assert.NotNull(savedProduct);
            AssertProductsEqual(product, savedProduct);

        }

        [Fact]
        public async Task Should_ReturnProduct_WhenProductExists()
        {
            //Arrange
            var addedProducts = await AddTestProducts(3);
            var expectedProduct = addedProducts.First();

            //Act
            var actualProduct = await _productRepository!.GetByIdAsync(expectedProduct.Id, CancellationToken.None);

            //Assert
            Assert.NotNull(actualProduct);
            AssertProductsEqual(expectedProduct, actualProduct);

        }

        [Fact]
        public async Task Should_ReturnNull_WhenProductNotFound()
        {
            //Arrange
            var nonExistingId = Guid.NewGuid();
            //Act
            var actualProduct = await _productRepository!.GetByIdAsync(nonExistingId, CancellationToken.None);

            //Assert
            Assert.Null(actualProduct);
        }

        [Fact]
        public async Task Should_SaveAllChanges_WhenProductIsUpdated()
        {
            //Arrange
            var product = CreateTestProduct(1);
            await _context!.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            product.Name = "Updated Name";
            product.UpdatedDateUtc = DateTime.UtcNow;


            //Act
            await _productRepository!.UpdateAsync(product, CancellationToken.None);

            //Assert
            var dbProduct = await _context.Products.FindAsync(product.Id);
            Assert.NotNull(dbProduct);
            Assert.Equal("Updated Name", dbProduct!.Name);
            Assert.NotEqual(dbProduct.CreatedDateUtc, dbProduct.UpdatedDateUtc);            

        }

        [Fact]
        public async Task Should_RemoveProduct_WhenProductExists()
        {
            //Arrange
            var initialProducts = await AddTestProducts(3);
            var productToDelete = initialProducts.First();
            var initialCount = await _context!.Products.CountAsync();

            //Act
            await _productRepository!.DeleteAsync(productToDelete, CancellationToken.None);

            //Assert
            var deletedProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productToDelete.Id);
            Assert.Null(deletedProduct);
            Assert.Equal(initialCount - 1, await _context.Products.CountAsync());

            var remainingIds = initialProducts.Skip(1).Select(p => p.Id).ToList();
            var remainingProducts = await _context.Products
                .Where(p => remainingIds.Contains(p.Id))
                .CountAsync();

            Assert.Equal(2, remainingProducts);
        }
    }    
}
