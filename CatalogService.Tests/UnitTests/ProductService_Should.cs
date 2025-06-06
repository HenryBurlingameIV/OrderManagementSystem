using CatalogService.Application.DTO;
using CatalogService.Application.Services;
using CatalogService.Domain;
using CatalogService.Infrastructure.Contracts;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.ComponentModel.DataAnnotations;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;
namespace CatalogService.Tests.UnitTests
{
    public class ProductService_Should
    {
        [Fact]
        public async Task CreateProductAsync_ValidRequest_CreatesProduct()
        {            
            //Arange
            var createProductRequest = new ProductCreateRequest()
            {
                Name = "test",
                Description = "test",
                Category = "test",
                Price = 100,
                Quantity = 1
            };
            var product = new Product()
            {
                Id = Guid.NewGuid(),
                Name = createProductRequest.Name,
                Description = createProductRequest.Description,
                Category = createProductRequest.Category,
                Price = createProductRequest.Price,
                Quantity = createProductRequest.Quantity,
                UpdatedDateUtc = DateTime.UtcNow,
                CreatedDateUtc = DateTime.UtcNow
            };
            var mockRepository = new Mock<IRepository<Product>>();
            mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product.Id);

            var mockProductCreateRequestValidator = new Mock<IValidator<ProductCreateRequest>>();
            mockProductCreateRequestValidator.Setup(validator => validator.ValidateAsync(It.IsAny<ProductCreateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var mockProductValidator = new Mock<IValidator<Product>>();
            mockProductValidator.Setup(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var productService = new ProductService(mockRepository.Object,
                mockProductCreateRequestValidator.Object,
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                mockProductValidator.Object);
            //Act
            var actual = await productService.CreateProductAsync(createProductRequest, CancellationToken.None);

            //Assert
            Assert.Equal(product.Id, actual);
            mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once());
            mockProductCreateRequestValidator.Verify(validator => validator.ValidateAsync(It.IsAny<ProductCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
            mockProductValidator.Verify(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once());

        }

        [Fact]
        public async Task CreateProductAsync_InvalidRequest_ThrowsValidationException()
        {
            //Arrange
            var mockProductCreateRequestValidator = new Mock<IValidator<ProductCreateRequest>>();
            mockProductCreateRequestValidator.Setup(validator => validator.ValidateAsync(It.IsAny<ProductCreateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new [] { new ValidationFailure() }));
            var createProductRequest = new ProductCreateRequest()
            {
                Name = "",
                Description = "test",
                Category = "test",
                Price = 100,
                Quantity = 1
            };
            var mockRepository = new Mock<IRepository<Product>>();


            var productService = new ProductService(mockRepository.Object,
                mockProductCreateRequestValidator.Object,
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                Mock.Of<IValidator<Product>>());
            //Act & Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await productService.CreateProductAsync(createProductRequest, CancellationToken.None));

        }

    }
}