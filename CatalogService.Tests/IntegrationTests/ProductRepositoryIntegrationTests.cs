using CatalogService.Application.Contracts;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using OrderManagementSystem.Shared.Contracts;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ValidationException = FluentValidation.ValidationException;
using CatalogService.Tests.ProductFixture;

namespace CatalogService.Tests.IntegrationTests
{
    public class ProductRepositoryIntegrationTests : IClassFixture<ProductRepositoryFixture>, IAsyncLifetime
    {
        private readonly ProductRepositoryFixture _fixture;

        public ProductRepositoryIntegrationTests(ProductRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, AutoProductData]
        public async Task Should_ReturnProductIdAndSaveToDataBase_WhenCreateNewProduct(Product product)
        {
            //Arrange
            var expectedId = product.Id;

            //Act
            var actualId = await _fixture.ProductRepository.CreateAsync(product, CancellationToken.None);

            //Assert
            Assert.Equal(expectedId, actualId);
            var savedProduct = await _fixture.Context.Products.FindAsync(product.Id);
            Assert.Equivalent(product, savedProduct);
        }

        [Theory, AutoProductData]
        public async Task Should_ThrowValidationException_WhenCreatingProductWithDuplicateName(Product initialProduct, Product duplicateProduct)
        {
            //Arrange
            duplicateProduct.Name = initialProduct.Name;

            //Act && Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await _fixture.ProductRepository.CreateAsync(initialProduct, CancellationToken.None);
                await _fixture.ProductRepository.CreateAsync(duplicateProduct, CancellationToken.None);

            });
            Assert.Contains("Product name must be unique", exception.Message);            
        }

        [Theory, AutoProductData]
        public async Task Should_ReturnProduct_WhenProductExists(List<Product> products)
        {
            //Arrange
            await _fixture.Context.Products.AddRangeAsync(products);
            await _fixture.Context.SaveChangesAsync();
            var expectedProduct = products.First();

            //Act
            var actualProduct = await _fixture.ProductRepository!.GetByIdAsync(expectedProduct.Id, CancellationToken.None);

            //Assert
            Assert.Equivalent(expectedProduct, actualProduct);

        }

        [Fact]
        public async Task Should_ReturnNull_WhenProductNotFound()
        {
            //Arrange
            var nonExistingId = Guid.NewGuid();
            //Act
            var actualProduct = await _fixture.ProductRepository!.GetByIdAsync(nonExistingId, CancellationToken.None);

            //Assert
            Assert.Null(actualProduct);
        }

        [Theory, AutoProductData]
        public async Task Should_SaveAllChanges_WhenProductIsUpdated(Product product)
        {
            //Arrange
            await _fixture.Context.Products.AddAsync(product);
            await _fixture.Context.SaveChangesAsync();
            string newProductName = "Updated";
            product.Name = newProductName;
            product.UpdatedDateUtc = DateTime.UtcNow;


            //Act
            await _fixture.ProductRepository!.UpdateAsync(product, CancellationToken.None);

            //Assert
            var dbProduct = await _fixture.Context.Products.FindAsync(product.Id);
            Assert.NotNull(dbProduct);
            Assert.Equal(newProductName, dbProduct!.Name);
            Assert.NotEqual(dbProduct.CreatedDateUtc, dbProduct.UpdatedDateUtc);            

        }

        [Theory, AutoProductData]
        public async Task Should_RemoveProduct_WhenProductExists(List<Product> initialProducts)
        {
            //Arrange
            await _fixture.Context.Products.AddRangeAsync(initialProducts);
            await _fixture.Context.SaveChangesAsync();
            var productToDelete = initialProducts.First();
            var initialCount = await _fixture.Context.Products.CountAsync();

            //Act
            await _fixture.ProductRepository!.DeleteAsync(productToDelete, CancellationToken.None);

            //Assert
            var deletedProduct = await _fixture.Context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productToDelete.Id);
            Assert.Null(deletedProduct);
            Assert.Equal(initialCount - 1, await _fixture.Context.Products.CountAsync());

            var remainingIds = initialProducts.Skip(1).Select(p => p.Id).ToList();
            var remainingProducts = await _fixture.Context.Products
                .Where(p => remainingIds.Contains(p.Id))
                .CountAsync();

            Assert.Equal(2, remainingProducts);
        }

        public async Task InitializeAsync()
        {
            await _fixture.ResetDatabase();
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }    
}
