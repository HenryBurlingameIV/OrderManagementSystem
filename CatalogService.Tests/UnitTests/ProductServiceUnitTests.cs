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
namespace CatalogService.Tests.UnitTests
{
    public class ProductServiceUnitTests
    {     
        private IValidator<ProductCreateRequest> _productCreateRequestValidator;
        private IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
        private IValidator<ProductUpdateQuantityRequest> _productUpdateQuantityRequestValidator;
        private IValidator<GetPagedProductsRequest> _paginationValidator;
        private Mock<IRepository<Product>> _mockRepository;
        private Mock<ILogger<ProductService>> _mockLogger;
        private IProductService _productService;

        public ProductServiceUnitTests() 
        {
            _productCreateRequestValidator = new ProductCreateRequestValidator();
            _productUpdateRequestValidator = new ProductUpdateRequestValidator();
            _productUpdateQuantityRequestValidator = new ProductUpdateQuantityValidator();
            _paginationValidator = new GetPagedProductsRequestValidator();
            _mockRepository = new Mock<IRepository<Product>>();
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
        public async Task Should_CreateProduct_WhenRequestIsValid(ProductCreateRequest request)
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
        public async Task Should_ThrowValidationException_WhenCreateProductWithEmptyName(ProductCreateRequest request)
        {
            //Arrange
            var requestWithEmptyName = request with { Name = "" };

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _productService.CreateProductAsync(requestWithEmptyName, CancellationToken.None));
            Assert.Contains("'Name' must not be empty", exception.Message);
        }

        [Theory, AutoProductData]
        public async Task Should_ReturnProductViewModel_WhenProductExists(Product product)
        {
            //Arrange
            var id = product.Id;
            _mockRepository
                .Setup(repo => repo.FindAsync(
                    It.Is<object[]>(keys => keys.Length == 1 && (Guid)keys[0] == id), 
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
                .Verify(repo => repo.FindAsync(
                    It.Is<object[]>(keys => keys.Length == 1 && (Guid)keys[0] == id), 
                    CancellationToken.None), 
                Times.Once());
        }

        //[Theory, AutoProductData]
        //public async Task Should_UpdateAllProperties_WhenRequestContainsAllProperties(Product product, ProductUpdateRequest request)
        //{
        //    //Arrange
        //    var initialUpdatedDate = product.UpdatedDateUtc;
        //    var id = product.Id;

        //    _mockRepository
        //        .Setup(repo => repo.GetByIdAsync(id, CancellationToken.None))
        //        .ReturnsAsync((Guid id, CancellationToken token) => product);

        //    _mockRepository
        //        .Setup(repo => repo.UpdateAsync(
        //            It.Is<Product>(p =>
        //                p.Name == request.Name &&
        //                p.Description == request.Description &&
        //                p.Category == request.Category &&
        //                p.Price == request.Price &&
        //                p.Quantity == request.Quantity), 
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Product p, CancellationToken token) => p.Id);

        //    //Act
        //    var actual = await _productService.UpdateProductAsync(id, request, CancellationToken.None);

        //    //Assert
        //    Assert.Equal(id, actual);
        //    Assert.Equal(request.Name, product.Name);
        //    Assert.Equal(request.Description, product.Description);
        //    Assert.Equal(request.Category, product.Category);
        //    Assert.Equal(request.Price, product.Price);
        //    Assert.Equal(request.Quantity, product.Quantity);
        //    Assert.True(product.UpdatedDateUtc > initialUpdatedDate);
        //    _mockRepository.VerifyAll();
        //}


        //[Theory, AutoProductData]
        //public async Task Should_UpdateOnlyNonNullProperties_WhenUpdateWithPartialRequest(ProductUpdateRequest request, Product product)
        //{
        //    //Arrange
        //    product.Name = "OriginalName";
        //    product.Description = "OriginalDescription";
        //    product.Price = 100;
        //    product.Quantity = 5;

        //    request.Name = null;
        //    request.Category = null;
        //    request.Price = null;
        //    request.Quantity = null;
        //    request.Description = "UpdatedDescription"; // Единственное изменяемое поле
        //    var id = product.Id;

        //    _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Guid id, CancellationToken token) => product);
        //    _mockRepository.Setup(repo => repo.UpdateAsync(
        //        It.Is<Product>(p =>
        //            p.Name == product.Name &&
        //            p.Description == request.Description &&
        //            p.Price == product.Price), 
        //        It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Product p, CancellationToken token) => p.Id);


        //    //Act
        //    var actual = await _productService.UpdateProductAsync(id, request, CancellationToken.None);

        //    //Assert
        //    Assert.Equal(id, actual);
        //    Assert.Equal(request.Description, product.Description);
        //    Assert.Equal("UpdatedDescription", product.Description);
        //    Assert.Equal("OriginalName", product.Name); 
        //    Assert.Equal(100, product.Price);
        //    Assert.Equal(5, product.Quantity);
        //    _mockRepository.VerifyAll();       
        //}

        //[Theory, AutoProductData]
        //public async Task Should_ThrowValidationException_WhenQuantityIsNegative(ProductUpdateRequest request, Product product) 
        //{
        //    //Arrange
        //    request.Quantity = -10;
        //    var id = product.Id;


        //    //Act & Assert
        //    var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _productService.UpdateProductAsync(id, request, CancellationToken.None));
        //    Assert.Contains("'Quantity' must be greater than or equal to '0'", exception.Message);
        //    _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        //}

        //[Theory, AutoProductData] 
        //public async Task Should_ThrowNotFoundException_WhenProductToUpdateDoesNotExist(ProductUpdateRequest request)
        //{
        //    //Arrange
        //    var id = Guid.NewGuid();
        //    _mockRepository
        //        .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Product)null);

        //    //Act & Assert
        //    var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.UpdateProductAsync(id, request, CancellationToken.None));
        //    Assert.Contains($"Product with ID {id} not found.", exception.Message);
        //    _mockRepository.VerifyAll();
        //    _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        //}

        //[Theory, AutoProductData]
        //public async Task Should_UpdateQuantity_WhenRequestContainsValidData(Product product)
        //{
        //    var updateRequest = new ProductUpdateQuantityRequest()
        //    {
        //        DeltaQuantity = -1
        //    };

        //    var initialQuantity = product.Quantity;
        //    var id = product.Id;
        //    var expectedQuantity = initialQuantity + updateRequest.DeltaQuantity;

        //    _mockRepository
        //        .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Guid id, CancellationToken token) => product);

        //    _mockRepository
        //        .Setup(repo => repo.UpdateAsync(
        //            It.Is<Product>(p=> 
        //                p.Name == product.Name &&
        //                p.Quantity == expectedQuantity), 
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Product p, CancellationToken token) => p.Id);


        //    //Act
        //    await _productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None);

        //    //Assert
        //    Assert.Equal(expectedQuantity, product.Quantity);
        //    _mockRepository.VerifyAll();
        //}

        //[Theory, AutoProductData]
        //public async Task Should_ThrowValidationException_WhenRequestedQuantityExceedsAvailable(Product product)
        //{
        //    var updateRequest = new ProductUpdateQuantityRequest()
        //    {
        //        DeltaQuantity = -(product.Quantity + 1)
        //    };

        //    var initialQuantity = product.Quantity;
        //    var id = product.Id;

        //    _mockRepository
        //        .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Guid id, CancellationToken token) => product);

        //    //Act & Assert
        //    var exception = await Assert.ThrowsAsync<ValidationException>(async() => await _productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None));
        //    Assert.Contains($"Product '{product.Name}' does not have enough quantity available. Requested: {Math.Abs(updateRequest.DeltaQuantity)}, Available: {product.Quantity}.", exception.Message);
        //    _mockRepository.VerifyAll();
        //    _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);

        //}


        //[Theory, AutoProductData]
        //public async Task Should_RemoveProduct_WhenProductExists(Product product)
        //{
        //    //Arrange
        //    var id = product.Id;
        //    Product deletedProduct = null;
        //    _mockRepository
        //        .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Guid id, CancellationToken token) => product);
        //    _mockRepository
        //        .Setup(repo => repo.DeleteAsync(It.Is<Product>(p => p.Id == id), It.IsAny<CancellationToken>()))
        //        .Callback<Product, CancellationToken>((p, _) => deletedProduct = p)
        //        .Returns(Task.CompletedTask);

        //    //Act
        //    await _productService.DeleteProductAsync(id, CancellationToken.None);

        //    //Assert
        //    Assert.Same(product, deletedProduct);
        //    _mockRepository.VerifyAll();
        //}

        //[Fact]
        //public async Task Should_ThrowNotFoundException_WhenProductToDeleteNotFound()
        //{
        //    //Arrange
        //    var id = Guid.NewGuid();
        //    _mockRepository
        //        .Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Guid id, CancellationToken token) => (Product)null);

        //    //Act & Assert
        //    var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.DeleteProductAsync(id, CancellationToken.None));
        //    Assert.Contains($"Product with ID {id} not found.", exception.Message);
        //    _mockRepository.VerifyAll();

        //}
    }

}