using CatalogService.Application.DTO;
using CatalogService.Application.Services;
using CatalogService.Domain;
using CatalogService.Domain.Exceptions;
using CatalogService.Infrastructure.Contracts;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;
namespace CatalogService.Tests.UnitTests
{
    public class ProductService_Should
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
            var mockRepository = new Mock<IRepository<Product>>();
            mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product p, CancellationToken token) =>
                {
                    _products.Add(p);
                    return p.Id;
                });


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
            var actualId = await productService.CreateProductAsync(createRequest, CancellationToken.None);

            //Assert
            var createdProduct = _products.Last();
            Assert.Equal(actualId, createdProduct.Id);
            Assert.Equal(createRequest.Name, createdProduct.Name);
            Assert.Equal(createRequest.Description, createdProduct.Description);
            Assert.Equal(createRequest.Price, createdProduct.Price);
            mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once());
            mockProductCreateRequestValidator.Verify(validator => validator.ValidateAsync(createRequest, It.IsAny<CancellationToken>()), Times.Once());
            mockProductValidator.Verify(validator => validator.ValidateAsync(createdProduct, It.IsAny<CancellationToken>()), Times.Once());

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
            var mockProductCreateRequestValidator = new Mock<IValidator<ProductCreateRequest>>();
            mockProductCreateRequestValidator.Setup(validator => validator.ValidateAsync(createRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

            var mockRepository = new Mock<IRepository<Product>>();
            var productService = new ProductService(
                mockRepository.Object,
                mockProductCreateRequestValidator.Object,
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                Mock.Of<IValidator<Product>>());
            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await productService.CreateProductAsync(createRequest, CancellationToken.None));
            Assert.Contains("Name is required", exception.Message);
            mockProductCreateRequestValidator.Verify(validator => validator.ValidateAsync(createRequest, CancellationToken.None), Times.Once());

        }

        [Fact]
        public async Task GetProductById_ValidRequest_ReturnsViewModel()
        {
            //Arrange
            var product = _products[0];
            var id = product.Id;
            var mockRepository = new Mock<IRepository<Product>>();
            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
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
            var productService = new ProductService(mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                Mock.Of<IValidator<Product>>());

            //Act
            var actual = await productService.GetProductByIdAsync(id, CancellationToken.None);

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
            mockRepository.Verify(repo => repo.GetByIdAsync(id, CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task GetProductById_InvalidRequest_ThrowsNotFoundException()
        {
            //Arrange
            var mockRepository = new Mock<IRepository<Product>>();
            var id = Guid.NewGuid();
            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as Product);
            var productService = new ProductService(mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                Mock.Of<IValidator<Product>>());

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await productService.GetProductByIdAsync(id, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, CancellationToken.None), Times.Once());
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
            var id = productToUpdate.Id;

            var mockRepository = new Mock<IRepository<Product>>();

            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            mockRepository.Setup(repo => repo.UpdateAsync(id, It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, Product p, CancellationToken token) => p.Id);
 

            var mockProductUpdateRequestValidator = new Mock<IValidator<ProductUpdateRequest>>();
            mockProductUpdateRequestValidator.Setup(validator => validator.ValidateAsync(It.IsAny<ProductUpdateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var mockProductValidator = new Mock<IValidator<Product>>();
            mockProductValidator.Setup(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var productService = new ProductService(mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                mockProductUpdateRequestValidator.Object,
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                mockProductValidator.Object);

            //Act
            var actual = await productService.UpdateProductAsync(id, updateRequest, CancellationToken.None);

            //Assert
            var updatedProduct = _products.First(p => p.Id == id);
            Assert.Equal(id, actual);
            Assert.Equal(updatedProduct.Id, actual);
            Assert.Equal(updateRequest.Name, updatedProduct.Name);
            Assert.Equal(updateRequest.Description, updatedProduct.Description);
            Assert.Equal(updateRequest.Category, updatedProduct.Category);
            Assert.Equal(updateRequest.Price, updatedProduct.Price);
            Assert.Equal(updateRequest.Quantity, updatedProduct.Quantity);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once());
            mockRepository.Verify(repo => repo.UpdateAsync(id, It.Is<Product>(p => p.Name == updateRequest.Name && p.Price == updateRequest.Price), It.IsAny<CancellationToken>()), Times.Once());
            mockProductUpdateRequestValidator.Verify(validator => validator.ValidateAsync(It.Is<ProductUpdateRequest>(req => req.Name == updateRequest.Name && req.Price == updateRequest.Price), It.IsAny<CancellationToken>()), Times.Once());
            mockProductValidator.Verify(validator => validator.ValidateAsync(It.Is<Product>(p => p.Name == updateRequest.Name && p.Price == updateRequest.Price), It.IsAny<CancellationToken>()), Times.Once());
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

            var mockRepository = new Mock<IRepository<Product>>();

            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            mockRepository.Setup(repo => repo.UpdateAsync(id, It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, Product p, CancellationToken token) => p.Id);


            var mockProductUpdateRequestValidator = new Mock<IValidator<ProductUpdateRequest>>();
            mockProductUpdateRequestValidator.Setup(validator => validator.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var mockProductValidator = new Mock<IValidator<Product>>();
            mockProductValidator.Setup(validator => validator.ValidateAsync(productToUpdate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var productService = new ProductService(mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                mockProductUpdateRequestValidator.Object,
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                mockProductValidator.Object);

            //Act
            var actual = await productService.UpdateProductAsync(id, updateRequest, CancellationToken.None);

            //Assert
            var updatedProduct = _products.First(p => p.Id == id);
            Assert.Equal(id, actual);
            Assert.Equal(updatedProduct.Id, id);
            Assert.Equal("test1", updatedProduct.Name);
            Assert.Equal(updateRequest.Description, updatedProduct.Description);
            Assert.Equal("test1", updatedProduct.Category);
            Assert.Equal(100, updatedProduct.Price);
            Assert.Equal(1, updatedProduct.Quantity);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once());
            mockRepository.Verify(repo => repo.UpdateAsync(id, It.Is<Product>(p => p.Name == "test1" && p.Description == updateRequest.Description), It.IsAny<CancellationToken>()), Times.Once());
            mockProductUpdateRequestValidator.Verify(validator => validator.ValidateAsync(It.Is<ProductUpdateRequest>(req => req.Name == updateRequest.Name && req.Price == updateRequest.Price), It.IsAny<CancellationToken>()), Times.Once());
            mockProductValidator.Verify(validator => validator.ValidateAsync(It.Is<Product>(p => p.Name == "test1" && p.Description == updateRequest.Description), It.IsAny<CancellationToken>()), Times.Once());
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
            var mockRepository = new Mock<IRepository<Product>>();
            var mockProductUpdateRequestValidator = new Mock<IValidator<ProductUpdateRequest>>();
            mockProductUpdateRequestValidator.Setup(validator => validator.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Quantity", "Quantity must be greater than 0") }));
            var mockProductValidator = new Mock<IValidator<Product>>();
            var productService = new ProductService(
                mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                mockProductUpdateRequestValidator.Object,
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                mockProductValidator.Object
                );

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await productService.UpdateProductAsync(id, updateRequest, CancellationToken.None));
            Assert.Contains("Quantity must be greater than 0", exception.Message);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Never());
            mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());
            mockProductValidator.Verify(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact] 
        public async Task UpdateProduct_NotFoundId_ThrowsNotFoundException()
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
            var id = productToUpdate.Id;
            var mockRepository = new Mock<IRepository<Product>>();
            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as Product);

            var mockProductUpdateRequestValidator = new Mock<IValidator<ProductUpdateRequest>>();
            mockProductUpdateRequestValidator.Setup(validator => validator.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            var mockProductValidator = new Mock<IValidator<Product>>();

            var productService = new ProductService(mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                mockProductUpdateRequestValidator.Object,
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                mockProductValidator.Object);

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await productService.UpdateProductAsync(id, updateRequest, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            mockProductUpdateRequestValidator.Verify(validator => validator.ValidateAsync(updateRequest, CancellationToken.None), Times.Once());
            mockRepository.Verify(repo => repo.GetByIdAsync(id, CancellationToken.None), Times.Once());
            mockProductValidator.Verify(validator => validator.ValidateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());
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


            var mockRepository = new Mock<IRepository<Product>>();

            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            mockRepository.Setup(repo => repo.UpdateAsync(id, It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, Product p, CancellationToken token) => p.Id);

            var mockProductUpdateQuantityRequestValidator = new Mock<IValidator<ProductUpdateQuantityRequest>>();
            mockProductUpdateQuantityRequestValidator.Setup(validator => validator.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var productService = new ProductService(
                mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                mockProductUpdateQuantityRequestValidator.Object,
                Mock.Of<IValidator<Product>>());

            var b = (productToUpdate == (_products.Find(p => p.Id == id)));

            //Act
            await productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None);

            //Assert
            var expected = initialQuantity - updateRequest.Quantity;
            Assert.Equal(expected, productToUpdate.Quantity);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once());
            mockProductUpdateQuantityRequestValidator.Verify(validator => validator.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()), Times.Once());     
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


            var mockRepository = new Mock<IRepository<Product>>();

            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            var mockProductUpdateQuantityRequestValidator = new Mock<IValidator<ProductUpdateQuantityRequest>>();
            mockProductUpdateQuantityRequestValidator.Setup(validator => validator.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var productService = new ProductService(
                mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                mockProductUpdateQuantityRequestValidator.Object,
                Mock.Of<IValidator<Product>>());

            //Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(async() => await productService.UpdateProductQuantityAsync(id, updateRequest, CancellationToken.None));
            Assert.Contains($"Product '{productToUpdate.Name}' does not have enough quantity available. Requested: {updateRequest.Quantity}, Available: {productToUpdate.Quantity}.", exception.Message);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, CancellationToken.None), Times.Once());
            mockRepository.Verify(repo => repo.UpdateAsync(id, It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never());
            mockProductUpdateQuantityRequestValidator.Verify(validator => validator.ValidateAsync(updateRequest, It.IsAny<CancellationToken>()), Times.Once());           
        }


        [Fact]
        public async Task DeleteProduct_ValidId_DeleteProduct()
        {
            //Arrange
            var productToDelete = _products[0];
            var id = productToDelete.Id;
            var mockRepository = new Mock<IRepository<Product>>();
            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_products.First(p => p.Id == id));
            mockRepository.Setup(repo => repo.DeleteAsync(productToDelete, It.IsAny<CancellationToken>()))
                .Callback((Product p, CancellationToken token) =>
                {
                    _products.Remove(p);
                })
                .Returns(Task.CompletedTask);

            var productService = new ProductService(
                mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                Mock.Of<IValidator<Product>>());

            //Act
            await productService.DeleteProductAsync(id, CancellationToken.None);

            //Assert
            Assert.DoesNotContain(productToDelete, _products);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once());
            mockRepository.Verify(repo => repo.DeleteAsync(It.Is<Product>(p => p.Id == id), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteProduct_NotFoundId_ThrowsNotFoundEcxeption()
        {
            //Arrange
            var id = Guid.NewGuid();
            var mockRepository = new Mock<IRepository<Product>>();
            mockRepository.Setup(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken token) => _products.Find(p => p.Id == id));

            var productService = new ProductService(
                mockRepository.Object,
                Mock.Of<IValidator<ProductCreateRequest>>(),
                Mock.Of<IValidator<ProductUpdateRequest>>(),
                Mock.Of<IValidator<ProductUpdateQuantityRequest>>(),
                Mock.Of<IValidator<Product>>());

            //Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(async () => await productService.DeleteProductAsync(id, CancellationToken.None));
            Assert.Contains($"Product with ID {id} not found.", exception.Message);
            mockRepository.Verify(repo => repo.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once());
            mockRepository.Verify(repo => repo.DeleteAsync(It.Is<Product>(p => p.Id == id), It.IsAny<CancellationToken>()), Times.Never());
        }
    }

}