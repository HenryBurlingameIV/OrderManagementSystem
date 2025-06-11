using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Application.Services;
using CatalogService.Application.Validators;
using CatalogService.Domain;
using CatalogService.Domain.Exceptions;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;
namespace CatalogService.Tests.UnitTests
{
    public class ProductServiceUnitTests
    {
        private readonly List<Product> _products = new List<Product>()
        {
            {
                new Product()
                {
                    Id = Guid.NewGuid(),
                    Name = "test1",
                    Description = "test1",
                    Category = "test1",
                    Price = 100,
                    Quantity = 1,
                    UpdatedDateUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow
                }
            },
            {
                new Product()
                {
                    Name = "test2",
                    Description = "test2",
                    Category = "test2",
                    Price = 200,
                    Quantity = 2,
                    UpdatedDateUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow
                }

            },
            {
                new Product()
                {
                    Name = "test3",
                    Description = "test3",
                    Category = "test3",
                    Price = 300,
                    Quantity = 3,
                    UpdatedDateUtc = DateTime.UtcNow,
                    CreatedDateUtc = DateTime.UtcNow
                }
            }
        };

        private IValidator<ProductCreateRequest> _productCreateRequestValidator;
        private IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
        private IValidator<ProductUpdateQuantityRequest> _productUpdateQuantityRequestValidator;
        private Mock<IValidator<Product>> _mockProductValidator;
        private Mock<IRepository<Product>> _mockRepository;
        private IProductService _productService;

        public ProductServiceUnitTests() 
        {
            _productCreateRequestValidator = new ProductCreateRequestValidator();
            _productUpdateRequestValidator = new ProductUpdateRequestValidator();
            _productUpdateQuantityRequestValidator = new ProductUpdateQuantityValidator();
            _mockProductValidator = new Mock<IValidator<Product>>();
            _mockRepository = new Mock<IRepository<Product>>();
            _productService = new ProductService(
                _mockRepository.Object,
                _productCreateRequestValidator,
                _productUpdateRequestValidator,
                _productUpdateQuantityRequestValidator,
                _mockProductValidator.Object
                );

        }

        private void SetupMockProductNameUniquenessValidation(string expectedName)
        {
            _mockProductValidator.Setup(v => v.ValidateAsync(It.Is<Product>(p => 
                p.Name == expectedName), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product product, CancellationToken token) =>
            {
                if (_products.Any(p => p.Name == product.Name && p.Id != product.Id))
                    return new ValidationResult(new[] { new ValidationFailure("Name", "Product name must be unique") });
                else
                    return new ValidationResult();
            });
        } 
        

        [Fact]
        public async Task CreateProductAsync_ValidRequest_CreatesProduct()
        {
            //Arange
            var createRequest = new ProductCreateRequest()
            {
                Name = "test4",
                Description = "test4",
                Category = "test4",
                Price = 400,
                Quantity = 4
            };
            Product createdProduct = null;
            _mockRepository.Setup(repo => repo.CreateAsync(
                It.Is<Product>(p => 
                    p.Name == createRequest.Name &&
                    p.Description == createRequest.Description &&
                    p.Category == createRequest.Category &&
                    p.Price == createRequest.Price &&
                    p.Quantity == createRequest.Quantity), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product p, CancellationToken token) =>
                {
                    createdProduct = p;
                    _products.Add(p);
                    return p.Id;
                });
            SetupMockProductNameUniquenessValidation(createRequest.Name);

            //Act
            var actualId = await _productService.CreateProductAsync(createRequest, CancellationToken.None);

            //Assert
            Assert.NotEqual(Guid.Empty, actualId);
            _mockRepository.VerifyAll();
            _mockProductValidator.VerifyAll();
            Assert.Equal(createRequest.Name, createdProduct!.Name);
            Assert.Equal(createRequest.Description, createdProduct.Description);
            Assert.Equal(createRequest.Category, createdProduct.Category);
            Assert.Equal(createRequest.Price, createdProduct.Price);
            Assert.Equal(createRequest.Quantity, createdProduct.Quantity);
        }

        [Fact]
        public async Task CreateProductAsync_InvalidRequest_ThrowsValidationException()
        {
            //Arrange
            var createRequest = new ProductCreateRequest()
            {
                Name = "",
                Description = "test",
                Category = "test",
                Price = 100,
                Quantity = 1
            };

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _productService.CreateProductAsync(createRequest, CancellationToken.None));
            Assert.Contains("'Name' must not be empty", exception.Message);
            

        }

        [Fact]
        public async Task GetProductById_ValidRequest_ReturnsViewModel()
        {
            //Arrange
            var product = _products[0];
            var id = product.Id;
            var mockRepository = new Mock<IRepository<Product>>();
            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));
            var expectedViewModel = new ProductViewModel()
            {
                Id = id,
                Name = product.Name,
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price,
                Category = product.Category,
                CreatedDateUtc = product.CreatedDateUtc,
                UpdatedDateUtc = product.UpdatedDateUtc
            };

            //Act
            var actual = await _productService.GetProductByIdAsync(id, CancellationToken.None);

            //Assert
            Assert.IsType<ProductViewModel>(actual);
            Assert.NotNull(actual);
            Assert.Equal(expectedViewModel.Id, actual.Id);
            Assert.Equal(expectedViewModel.Name, actual.Name);
            Assert.Equal(expectedViewModel.Description, actual.Description);
            Assert.Equal(expectedViewModel.Category, actual.Category);
            Assert.Equal(expectedViewModel.Price, actual.Price);
            Assert.Equal(expectedViewModel.Quantity, actual.Quantity);
            Assert.Equal(expectedViewModel.CreatedDateUtc, actual.CreatedDateUtc);
            Assert.Equal(expectedViewModel.UpdatedDateUtc, actual.UpdatedDateUtc);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetProductById_InvalidRequest_ThrowsNotFoundException()
        {
            //Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.GetProductByIdAsync(id, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            _mockRepository.Verify(repo => repo.GetByIdAsync(id, CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task UpdateProduct_ValidRequestWithAllProperties_UpdatesProduct()
        {
            //Arrange
            var updateRequest = new ProductUpdateRequest()
            {
                Name = "updated",
                Description = "updated",
                Category = "updated",
                Price = 1000,
                Quantity = 10
            };

            var productToUpdate = _products.First();
            var initialUpdatedDate = productToUpdate.UpdatedDateUtc;
            var id = productToUpdate.Id;

            _mockRepository.Setup(repo => repo.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            _mockRepository.Setup(repo => repo.UpdateAsync(It.Is<Product>(p =>
                    p.Name == updateRequest.Name &&
                    p.Description == updateRequest.Description &&
                    p.Category == updateRequest.Category &&
                    p.Price == updateRequest.Price &&
                    p.Quantity == updateRequest.Quantity), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product p, CancellationToken token) => p.Id);
            SetupMockProductNameUniquenessValidation(updateRequest.Name);
 
            //Act
            var actual = await _productService.UpdateProductAsync(id, updateRequest, CancellationToken.None);
            var updatedProduct = _products.First(p => p.Id == id);

            //Assert
            Assert.Equal(id, actual);
            Assert.Equal(updatedProduct.Id, actual);
            Assert.Equal(updateRequest.Name, updatedProduct.Name);
            Assert.Equal(updateRequest.Description, updatedProduct.Description);
            Assert.Equal(updateRequest.Category, updatedProduct.Category);
            Assert.Equal(updateRequest.Price, updatedProduct.Price);
            Assert.Equal(updateRequest.Quantity, updatedProduct.Quantity);
            Assert.True(updatedProduct.UpdatedDateUtc > initialUpdatedDate);
            _mockProductValidator.VerifyAll();
            _mockProductValidator.VerifyAll();
        }


        [Fact]
        public async Task UpdateProduct_ValidRequestWithOneProperty_UpdatesProduct()
        {
            //Arrange
            var updateRequest = new ProductUpdateRequest()
            {
                Name = null,
                Description = "updatedProperty",
                Category = null,
                Price = null,
                Quantity = null
            };

            var productToUpdate = _products.First();
            var id = productToUpdate.Id;

            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));
            _mockRepository.Setup(repo => repo.UpdateAsync(
                It.Is<Product>(p =>
                    p.Name == productToUpdate.Name &&
                    p.Description == updateRequest.Description &&
                    p.Price == productToUpdate.Price), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product p, CancellationToken token) => p.Id);

            SetupMockProductNameUniquenessValidation(productToUpdate.Name);


            //Act
            var actual = await _productService.UpdateProductAsync(id, updateRequest, CancellationToken.None);
            var updatedProduct = _products.First(p => p.Id == id);

            //Assert
            Assert.Equal(id, actual);
            Assert.Equal(updatedProduct.Id, id);
            Assert.Equal("test1", updatedProduct.Name);
            Assert.Equal(updateRequest.Description, updatedProduct.Description);
            Assert.Equal("test1", updatedProduct.Category);
            Assert.Equal(100, updatedProduct.Price);
            Assert.Equal(1, updatedProduct.Quantity);
            
            _mockRepository.VerifyAll();       
            _mockProductValidator.VerifyAll();
        }

        [Fact]
        public async Task UpdateProduct_InvalidRequest_ThrowsValidationException()
        {
            //Arrange
            var updateRequest = new ProductUpdateRequest()
            {
                Name = null,
                Description = null,
                Category = null,
                Price = null,
                Quantity = -10
            };
            var productToUpdate = _products.First();
            var id = productToUpdate.Id;


            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _productService.UpdateProductAsync(id, updateRequest, CancellationToken.None));
            Assert.Contains("'Quantity' must be greater than or equal to '0'", exception.Message);
        }

        [Fact] 
        public async Task UpdateProduct_NotFoundId_ThrowsNotFoundException()
        {
            //Arrange
            var id = Guid.NewGuid();
            var updateRequest = new ProductUpdateRequest()
            {
                Name = "updated",
                Description = "updated",
                Category = "updated",
                Price = 1000,
                Quantity = 10
            };

            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));
            SetupMockProductNameUniquenessValidation(updateRequest.Name);


            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.UpdateProductAsync(id, updateRequest, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            _mockRepository.VerifyAll();
            _mockProductValidator.Verify(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateProductQuantity_ValidRequest_UpdateQuantity()
        {
            var updateRequest = new ProductUpdateQuantityRequest()
            {
                Quantity = 1
            };

            var productToUpdate = _products.First();
            var initialQuantity = productToUpdate.Quantity;
            var id = productToUpdate.Id;
            var expectedQuantity = initialQuantity - updateRequest.Quantity;

            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            _mockRepository.Setup(repo => repo.UpdateAsync(
                It.Is<Product>(p=> 
                    p.Name == productToUpdate.Name &&
                    p.Quantity == expectedQuantity), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product p, CancellationToken token) => p.Id);


            //Act
            await _productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(expectedQuantity, productToUpdate.Quantity);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task UpdateProductQuantity_InsufficientQuantity_ThrowsValidationException()
        {
            var updateRequest = new ProductUpdateQuantityRequest()
            {
                Quantity = 100
            };

            var productToUpdate = _products.First();
            var initialQuantity = productToUpdate.Quantity;
            var id = productToUpdate.Id;

            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async() => await _productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None));
            Assert.Contains($"Product '{productToUpdate.Name}' does not have enough quantity available. Requested: {updateRequest.Quantity}, Available: {productToUpdate.Quantity}.", exception.Message);
            _mockRepository.VerifyAll();
  
        }


        [Fact]
        public async Task DeleteProduct_ValidId_DeleteProduct()
        {
            //Arrange
            var productToDelete = _products[0];
            var id = productToDelete.Id;
            var mockRepository = new Mock<IRepository<Product>>();
            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));
            _mockRepository.Setup(repo => repo.DeleteAsync(productToDelete, It.IsAny<CancellationToken>()))
                .Callback((Product p, CancellationToken token) =>
                {
                    _products.Remove(p);
                })
                .Returns(Task.CompletedTask);

            //Act
            await _productService.DeleteProductAsync(id, CancellationToken.None);

            //Assert
            Assert.DoesNotContain(productToDelete, _products);
            _mockRepository.VerifyAll();

        }

        [Fact]
        public async Task DeleteProduct_NotFoundId_ThrowsNotFoundEcxeption()
        {
            //Arrange
            var id = Guid.NewGuid();
            var mockRepository = new Mock<IRepository<Product>>();
            _mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await _productService.DeleteProductAsync(id, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            _mockRepository.VerifyAll();

        }
    }

}