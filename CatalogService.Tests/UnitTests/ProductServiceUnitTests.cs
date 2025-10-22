using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Application.Services;
using CatalogService.Application.Validators;
using CatalogService.Domain;
using OrderManagementSystem.Shared.Exceptions;
using CatalogService.Infrastructure;
using OrderManagementSystem.Shared.Contracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;
using AutoFixture;
using CatalogService.Tests.ProductFixture;
using FluentAssertions;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.DataAccess.Pagination;
namespace CatalogService.Tests.UnitTests
{
    public class ProductServiceUnitTests
    {     
        private IValidator<CreateProductRequest> _productCreateRequestValidator;
        private IValidator<UpdateProductRequest> _productUpdateRequestValidator;
        private IValidator<ReserveProductRequest> _productUpdateQuantityRequestValidator;
        private IValidator<GetPagindatedProductsRequest> _paginationValidator;
        private Mock<IEFRepository<Product, Guid>> _mockRepository;
        private Mock<ILogger<ProductService>> _mockLogger;
        private IProductService _productService;

        public ProductServiceUnitTests() 
        {
            _productCreateRequestValidator = new CreateProductRequestValidator();
            _productUpdateRequestValidator = new UpdateProductRequestValidator();
            _productUpdateQuantityRequestValidator = new UpdateProductQuantityValidator();
            _paginationValidator = new GetPaginatedProductsRequestValidator();
            _mockRepository = new Mock<IEFRepository<Product, Guid>>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            _productService = new ProductService(
                _mockRepository.Object,
                _productCreateRequestValidator,
                _productUpdateRequestValidator,
                _productUpdateQuantityRequestValidator,
                _paginationValidator,
                _mockLogger.Object
                );

        }

        [Theory, AutoProductData]
        public async Task Should_CreateProduct_WhenRequestIsValid(CreateProductRequest request)
        {
            //Arange
            Product createdProduct = null;
            _mockRepository
                .Setup(repo => repo.InsertAsync(
                    It.Is<Product>(p => 
                        p.Name == request.Name), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product p, CancellationToken token) =>
                {
                    createdProduct = p;
                    return createdProduct;
                });

            _mockRepository
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository
                .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            //Act
            var actualId = await _productService.CreateProductAsync(request, CancellationToken.None);

            //Assert
            Assert.NotEqual(Guid.Empty, actualId);
            _mockRepository.VerifyAll();
            Assert.Equal(request.Name, createdProduct!.Name);
            Assert.Equal(request.Description, createdProduct.Description);
            Assert.Equal(request.Category, createdProduct.Category);
            Assert.Equal(request.Price, createdProduct.Price);
            Assert.Equal(request.Quantity, createdProduct.Quantity);
        }

        [Theory, AutoProductData]
        public async Task Should_ThrowValidationException_WhenCreateProductWithEmptyName(CreateProductRequest request)
        {
            //Arrange
            var requestWithEmptyName = request with { Name = "" };

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _productService.CreateProductAsync(requestWithEmptyName, CancellationToken.None));
            Assert.Contains("'Name' must not be empty", exception.Message);
        }

        [Theory, AutoProductData]
        public async Task Should_ThrowValidationException_WhenProductNameExists(CreateProductRequest request)
        {
            // Arrange
            _mockRepository
                .Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _productService.CreateProductAsync(request, CancellationToken.None));
            Assert.Contains("name", exception.Message);
            Assert.Contains($"{request.Name}", exception.Message);
            _mockLogger.VerifyAll();
        }

        [Theory, AutoProductData]
        public async Task Should_ReturnProductViewModel_WhenProductExists(Product product)
        {
            //Arrange
            var id = product.Id;
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(
                    It.Is<Guid>(key => key == id), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var expectedViewModel = new ProductViewModel(
                id, product.Name, product.Description, product.Category, product.Price, product.Quantity);


            //Act
            var actual = await _productService.GetProductByIdAsync(id, CancellationToken.None);

            //Assert
            Assert.IsType<ProductViewModel>(actual);
            Assert.NotNull(actual);
            actual.Should().BeEquivalentTo(expectedViewModel);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_WhenProductDoesNotExist()
        {
            //Arrange
            var id = Guid.NewGuid();

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.GetProductByIdAsync(id, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            _mockRepository
                .Verify(repo => repo.GetByIdAsync(
                    It.Is<Guid>(key => key == id), 
                    CancellationToken.None), 
                Times.Once());
        }

        [Theory, AutoProductData]
        public async Task Should_UpdateAllProperties_WhenRequestContainsAllProperties(Product product, UpdateProductRequest request)
        {
            //Arrange
            var initialUpdatedDate = product.UpdatedDateUtc;
            var id = product.Id;

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((Guid id, CancellationToken token) => product);

            _mockRepository
                .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            //Act
            var actual = await _productService.UpdateProductAsync(id, request, CancellationToken.None);

            //Assert
            Assert.Equal(id, actual);
            Assert.Equal(request.Name, product.Name);
            Assert.Equal(request.Description, product.Description);
            Assert.Equal(request.Category, product.Category);
            Assert.Equal(request.Price, product.Price);
            Assert.Equal(request.Quantity, product.Quantity);
            Assert.True(product.UpdatedDateUtc > initialUpdatedDate);
            _mockRepository.VerifyAll();
        }


        [Theory, AutoProductData]
        public async Task Should_UpdateOnlyNonNullProperties_WhenUpdateWithPartialRequest(UpdateProductRequest request, Product product)
        {
            //Arrange
            product.Name = "OriginalName";
            product.Description = "OriginalDescription";
            product.Price = 100;
            product.Quantity = 5;
            var id = product.Id;
            string newDescription = "UpdatedDescription";

            request = request with { Name = null, Description = newDescription, Price = null, Quantity = null, Category = null };

            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => product);

            _mockRepository
                .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            //Act
            var actual = await _productService.UpdateProductAsync(id, request, CancellationToken.None);

            //Assert
            Assert.Equal(id, actual);
            Assert.Equal(request.Description, product.Description);
            Assert.Equal(newDescription, product.Description);
            Assert.Equal("OriginalName", product.Name);
            Assert.Equal(100, product.Price);
            Assert.Equal(5, product.Quantity);
            _mockRepository.VerifyAll();
        }

        [Theory, AutoProductData]
        public async Task Should_ThrowValidationException_WhenQuantityIsNegative(UpdateProductRequest request, Product product)
        {
            //Arrange
            request = request with { Quantity = -10 };
            var id = product.Id;


            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _productService.UpdateProductAsync(id, request, CancellationToken.None));
            Assert.Contains("'Quantity' must be greater than or equal to '0'", exception.Message);
            _mockRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory, AutoProductData]
        public async Task Should_ThrowNotFoundException_WhenProductToUpdateDoesNotExist(UpdateProductRequest request)
        {
            //Arrange
            var id = Guid.NewGuid();
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.UpdateProductAsync(id, request, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            _mockRepository.VerifyAll();
            _mockRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory, AutoProductData]
        public async Task Should_UpdateQuantity_WhenRequestContainsValidData(Product product)
        {
            var updateRequest = new ReserveProductRequest(-1);


            var initialQuantity = product.Quantity;
            var id = product.Id;
            var expectedQuantity = initialQuantity + updateRequest.Quantity;

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => product);

            _mockRepository
                .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);



            //Act
            await _productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(expectedQuantity, product.Quantity);
            _mockRepository.VerifyAll();
        }

        [Theory, AutoProductData]
        public async Task Should_ThrowValidationException_WhenRequestedQuantityExceedsAvailable(Product product)
        {
            var updateRequest = new ReserveProductRequest(-(product.Quantity + 1));


            var initialQuantity = product.Quantity;
            var id = product.Id;

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => product);

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None));
            Assert.Contains($"Product '{product.Name}' does not have enough quantity available. Requested: {Math.Abs(updateRequest.Quantity)}, Available: {product.Quantity}.", exception.Message);
            _mockRepository.VerifyAll();
            _mockRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        }


        [Theory, AutoProductData]
        public async Task Should_RemoveProduct_WhenProductExists(Product product)
        {
            //Arrange
            var id = product.Id;
            Product deletedProduct = null;
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => product);
            _mockRepository
                .Setup(repo => repo.Delete(It.Is<Product>(p => p.Id == id)))
                .Callback<Product>((p) => deletedProduct = p);

            _mockRepository
                .Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);


            //Act
            await _productService.DeleteProductAsync(id, CancellationToken.None);

            //Assert
            Assert.Same(product, deletedProduct);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task Should_ThrowNotFoundException_WhenProductToDeleteNotFound()
        {
            //Arrange
            var id = Guid.NewGuid();
            _mockRepository
                .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => (Product)null);

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.DeleteProductAsync(id, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            _mockRepository.VerifyAll();

        }

        [Theory, AutoProductData]
        public async Task Should_GetValidPagedProductList_WhenRequested(List<Product> products)
        {
            //Arrange
            var pageNumber = 1;
            var pageSize = 3;
            var request = new GetPagindatedProductsRequest(
                pageNumber, pageSize, null, null);
            var expectedResult = new PaginatedResult<ProductViewModel>(
                products.Select(p => new ProductViewModel(p.Id, p.Name, p.Description, p.Category, p.Price, p.Quantity)),
                products.Count(),
                pageNumber,
                pageSize);

            _mockRepository
                .Setup(repo => repo.GetPaginated(
                    It.Is<PaginationRequest>(r => r.PageNumber == pageNumber && r.PageSize == pageSize),
                    It.IsAny<Expression<Func<Product, ProductViewModel>>>(),
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                    ))
                .ReturnsAsync(expectedResult);

            //Act
            var actualResult = await _productService.GetProductsPaginatedAsync(request, CancellationToken.None);

            //Assert
            Assert.NotNull(actualResult);
            Assert.NotNull(actualResult.Items);
            Assert.Equal(expectedResult.TotalCount, actualResult.TotalCount);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenPageNumberIsZero()
        {
            // Arrange
            var invalidRequest = new GetPagindatedProductsRequest(
                0, 2, null, null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _productService.GetProductsPaginatedAsync(invalidRequest, CancellationToken.None));
            Assert.Contains("PageNumber", exception.Message);
            Assert.Contains("must be greater than '0'", exception.Message);
        }

        [Theory]
        [InlineData("name", "Name")]
        [InlineData("price", "Price")]
        [InlineData("category", "Category")]
        [InlineData("created", "CreatedDateUtc")]
        public async Task Should_ApplyCorrectOrderBy_WhenValidSortByProvided(string sortBy, string expectedProperty)
        {
            // Arrange
            var request = new GetPagindatedProductsRequest(1, 10, null, sortBy);

            Func<IQueryable<Product>, IOrderedQueryable<Product>> capturedOrderBy = null;

            var returnsTask = new PaginatedResult<ProductViewModel>(
                new List<ProductViewModel>(), 0, 1, 10);

            _mockRepository
                .Setup(repo => repo.GetPaginated(
                    It.IsAny<PaginationRequest>(),
                    It.IsAny<Expression<Func<Product, ProductViewModel>>>(),
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Callback((PaginationRequest p,
                       Expression<Func<Product, ProductViewModel>> s,
                       Expression<Func<Product, bool>> f,
                       Func<IQueryable<Product>, IOrderedQueryable<Product>> o,
                       bool t, CancellationToken ct) =>
                {
                    capturedOrderBy = o;
                })
                .ReturnsAsync(returnsTask);

            // Act
            await _productService.GetProductsPaginatedAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedOrderBy);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task Should_ThrowValidationException_WhenInvalidSortByProvided()
        {
            // Arrange
            var request = new GetPagindatedProductsRequest(1, 10, null, "invalid");


            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
               _productService.GetProductsPaginatedAsync(request, CancellationToken.None));
            Assert.Contains("SortBy", exception.Message);
        }
    }

}