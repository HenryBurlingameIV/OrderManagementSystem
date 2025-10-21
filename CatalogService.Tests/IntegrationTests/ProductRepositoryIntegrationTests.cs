using CatalogService.Application.Contracts;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using OrderManagementSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ValidationException = FluentValidation.ValidationException;
using CatalogService.Tests.ProductFixture;
using OrderManagementSystem.Shared.DataAccess.Pagination;

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
        public async Task Should_ReturnTrue_WhenProductFound(Product product)
        {
            //Arrange
            await _fixture.Context.Products.AddAsync(product);
            await _fixture.Context.SaveChangesAsync();

            //Act
            var exists = await _fixture.ProductRepository.ExistsAsync(
                predicate: (p) => p.Id == product.Id,
                ct: CancellationToken.None);

            //Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task Should_ReturnFalse_WhenProductNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var exists = await _fixture.ProductRepository.ExistsAsync(
                predicate: p => p.Id == nonExistentId,
                ct: CancellationToken.None);

            // Assert
            Assert.False(exists);
        }

        [Theory, AutoProductData]
        public async Task Should_ReturnInsertedProduct_WhenInsertingNewProduct(Product product)
        {
            //Arrange
            var expectedId = product.Id;

            //Act
            var insertedProduct = await _fixture.ProductRepository.InsertAsync(product, CancellationToken.None);

            //Assert
            Assert.Equal(expectedId, insertedProduct.Id);
            Assert.Equivalent(product, insertedProduct);
        }

        [Theory, AutoProductData]
        public async Task Should_FindInsertedProduct_WhenProductWasInserted(Product product)
        {
            //Arrange
            var expectedId = product.Id;

            //Act
            await _fixture.ProductRepository.InsertAsync(product, CancellationToken.None);
            await _fixture.ProductRepository.SaveChangesAsync(CancellationToken.None);

            //Assert
            var insertedProduct = await _fixture.Context.Products.FirstOrDefaultAsync(p => p.Id == expectedId);
            Assert.NotNull(insertedProduct);
            Assert.Equivalent(product, insertedProduct);
        }

        [Theory, AutoProductData]
        public async Task Should_ReturnProduct_WhenProductExists(Product product)
        {
            //Arrange
            await _fixture.Context.Products.AddAsync(product);
            await _fixture.Context.SaveChangesAsync();


            //Act
            var foundProduct = await _fixture.ProductRepository!.FindAsync(new object[] { product.Id }, CancellationToken.None);

            //Assert
            Assert.NotNull(foundProduct);
            Assert.Equivalent(product, foundProduct);
        }

        [Fact]
        public async Task Should_ReturnNull_WhenProductNotFound()
        {
            //Arrange
            var nonExistingId = Guid.NewGuid();

            //Act
            var foundProduct = await _fixture.ProductRepository!.FindAsync(new object[] { nonExistingId }, CancellationToken.None);

            //Assert
            Assert.Null(foundProduct);
        }

        [Theory, AutoProductData]
        public async Task Should_GetProductById_WhenExists(Product product)
        {
            //Arrange
            await _fixture.Context.Products.AddAsync(product);
            await _fixture.Context.SaveChangesAsync();


            //Act
            var foundProduct = await _fixture.ProductRepository!.GetByIdAsync(product.Id, CancellationToken.None);

            //Assert
            Assert.NotNull(foundProduct);
            Assert.Equivalent(product, foundProduct);
        }

        [Theory, AutoProductData]
        public async Task Should_PersistChanges_WhenProductIsModifiedAndSaved(Product product)
        {
            //Arrange
            await _fixture.Context.Products.AddAsync(product);
            await _fixture.Context.SaveChangesAsync();
            string newProductName = "Updated";
            decimal newProductPrice = 99.9m;


            //Act
            var productToUpdate = await _fixture.ProductRepository.FindAsync(new object[] { product.Id }, CancellationToken.None);
            productToUpdate!.Name = newProductName;
            productToUpdate!.Price = newProductPrice;
            productToUpdate.UpdatedDateUtc = DateTime.UtcNow;
            var affectedRows = await _fixture.ProductRepository!.SaveChangesAsync(CancellationToken.None);

            //Assert
            Assert.Equal(1, affectedRows);
            var updatedProduct = await _fixture.Context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == product.Id);

            Assert.NotNull(updatedProduct);
            Assert.Equal(newProductName, updatedProduct!.Name);
            Assert.Equal(newProductPrice, updatedProduct!.Price);
            Assert.NotEqual(updatedProduct.CreatedDateUtc, updatedProduct.UpdatedDateUtc);

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
            _fixture.ProductRepository.Delete(productToDelete);
            await _fixture.ProductRepository.SaveChangesAsync(CancellationToken.None);

            //Assert
            var deletedProduct = await _fixture.Context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productToDelete.Id);
            Assert.Null(deletedProduct);
            Assert.Equal(initialCount - 1, await _fixture.Context.Products.CountAsync());
        }

        [Theory, AutoProductData]
        public async Task Should_GetValidPagedProductList_WhenRequested(List<Product> products)
        {
            //Arrange
            await _fixture.Context.Products.AddRangeAsync(products);
            await _fixture.Context.SaveChangesAsync();
            var totalCount = products.Count();
            var pageSize = totalCount - 1;
            var request = new PaginationRequest()
            {
                PageNumber = 1,
                PageSize = pageSize
            };
            var expectedTotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var expectedIds = products.Take(pageSize).Select(p => p.Id).ToList();

            //Act
            var pagedList = await _fixture.ProductRepository.GetPaginated(
                request: request,
                ct: CancellationToken.None
                );

            //Assert
            Assert.NotNull(pagedList);
            Assert.NotNull(pagedList.Items);
            Assert.Equal(pageSize, pagedList.Items.Count);
            Assert.Equal(pageSize, pagedList.PageSize);
            Assert.Equal(totalCount, pagedList.TotalCount);
            Assert.Equal(expectedTotalPages, pagedList.TotalPages);
            var actualIds = pagedList.Items.Select(p => p.Id).ToList();
            Assert.Equal(expectedIds, actualIds);
        }


        [Theory, AutoProductData]
        public async Task Should_GetEmptyPagedProductList_WhenRequestingPageBeyondTotalPages(List<Product> products)
        {
            //Arrange
            await _fixture.Context.Products.AddRangeAsync(products);
            await _fixture.Context.SaveChangesAsync();
            var totalCount = products.Count();
            var pageSize = 2;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pageBeyondTotal = totalPages + 1;
            var request = new PaginationRequest()
            {
                PageNumber = pageBeyondTotal,
                PageSize = pageSize
            };

            //Act
            var pagedList = await _fixture.ProductRepository.GetPaginated(
                request: request,
                ct: CancellationToken.None
                );

            //Assert
            Assert.NotNull(pagedList);
            Assert.NotNull(pagedList.Items);
            Assert.Empty(pagedList.Items);
            Assert.Equal(pageSize, pagedList.PageSize);
            Assert.Equal(totalCount, pagedList.TotalCount);
            Assert.Equal(totalPages, pagedList.TotalPages);
        }

        [Theory, AutoProductData]
        public async Task Should_GetValidFilteredPagedProductList_WhenRequested(List<Product> products)
        {
            //Arrange
            var searchName = Guid.NewGuid().ToString();
            var productWithSearchName = products.Last();
            productWithSearchName.Name = searchName;
            await _fixture.Context.Products.AddRangeAsync(products);
            await _fixture.Context.SaveChangesAsync();
            var totalCount = products.Count();
            var pageSize = totalCount;
            var request = new PaginationRequest()
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            //Act
            var pagedList = await _fixture.ProductRepository.GetPaginated(
                request: request,
                filter: (p) => p.Name == searchName,
                ct: CancellationToken.None);

            //Assert
            Assert.NotNull(pagedList.Items);
            Assert.Single(pagedList.Items, (p) => p.Name == searchName);
            Assert.Equal(productWithSearchName.Id, pagedList.Items[0].Id);
        }


        [Theory, AutoProductData]
        public async Task Should_ReturnPagedSortedNames_WhenUsingProjectionWithOrderBy(List<Product> products)
        {
            //Arrange
            await _fixture.Context.Products.AddRangeAsync(products);
            await _fixture.Context.SaveChangesAsync();
            var expectedNames = products.Select(p => p.Name).OrderBy(n => n).ToList();
            var request = new PaginationRequest()
            {
                PageNumber = 1,
                PageSize = products.Count()
            };

            //Act
            var pagedList = await _fixture.ProductRepository.GetPaginated<string>(
                selector: (p) => p.Name,
                orderBy: q => q.OrderBy(p => p.Name),
                request: request,
                ct: CancellationToken.None);

            //Assert
            Assert.NotNull(pagedList.Items);
            Assert.All(pagedList.Items, (n) => Assert.IsType<string>(n));
            Assert.Equal(expectedNames, pagedList.Items);
        }

        [Theory, AutoProductData]
        public async Task Should_ReturnProduct_WhenFilteringById(Product product)
        {
            //Arrange
            await _fixture.Context.Products.AddAsync(product);
            await _fixture.Context.SaveChangesAsync();

            //Act
            var first = await _fixture.ProductRepository.GetFirstOrDefaultAsync(
                filter: (p) => p.Id == product.Id,
                ct: CancellationToken.None
                );

            //
            Assert.NotNull(first);
            Assert.Equal(product.Id, first.Id);
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
