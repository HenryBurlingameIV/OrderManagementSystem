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
        private readonly Mock<IValidator<ProductCreateRequest>> _validProductCreateRequestValidator;
        private readonly Mock<IValidator<ProductUpdateRequest>> _validProductUpdateRequestValidator;
        private readonly Mock<IValidator<ProductUpdateQuantityRequest>> _validQuantityUpdateRequestValidator;
        private readonly Mock<IValidator<Product>> _validProductValidator;


        public ProductService_Should()
        {
            _validProductCreateRequestValidator = new Mock<IValidator<ProductCreateRequest>>();
            _validProductCreateRequestValidator.Setup(validator => validator.ValidateAsync(It.IsAny<ProductCreateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _validProductUpdateRequestValidator = new Mock<IValidator<ProductUpdateRequest>>();
            _validProductUpdateRequestValidator.Setup(validator => validator.ValidateAsync(It.IsAny<ProductUpdateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _validQuantityUpdateRequestValidator = new Mock<IValidator<ProductUpdateQuantityRequest>>();
            _validQuantityUpdateRequestValidator.Setup(validator => validator.ValidateAsync(It.IsAny<ProductUpdateQuantityRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _validProductValidator = new Mock<IValidator<Product>>();
            _validProductValidator.Setup(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());           

        }

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


            var productService = new ProductService(mockRepository.Object,
                _validProductCreateRequestValidator.Object,
                _validProductUpdateRequestValidator.Object,
                _validQuantityUpdateRequestValidator.Object,
                _validProductValidator.Object);
            //Act
            var actual = await productService.CreateProductAsync(createProductRequest, CancellationToken.None);

            //Assert
            Assert.Equal(product.Id, actual);
            mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once());
            _validProductCreateRequestValidator.Verify(validator => validator.ValidateAsync(It.IsAny<ProductCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once());
            _validProductValidator.Verify(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once());

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
                _validProductUpdateRequestValidator.Object,
                _validQuantityUpdateRequestValidator.Object,
                _validProductValidator.Object);
            //Act & Assert
            await Assert.ThrowsAsync<ValidationException>(async () => await productService.CreateProductAsync(createProductRequest, CancellationToken.None));

        }

    }
}