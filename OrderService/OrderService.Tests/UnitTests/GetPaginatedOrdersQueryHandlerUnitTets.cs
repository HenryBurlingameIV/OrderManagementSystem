using FluentValidation;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using OrderService.Application.DTO;
using OrderService.Application.Queries.OrderQuery;
using OrderService.Application.Queries.OrdersPaginatedQuery;
using OrderService.Application.Validators;
using OrderService.Domain.Entities;
using OrderService.Tests.OrderFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.UnitTests
{
    public class GetPaginatedOrdersQueryHandlerUnitTests
    {
        private Mock<IEFRepository<Order, Guid>> _mockOrderRepository;
        private Mock<ILogger<GetPaginatedOrdersQueryHandler>> _mockLogger;
        private GetPaginatedOrdersRequestValidator _validator;
        private GetPaginatedOrdersQueryHandler _handler;

        public GetPaginatedOrdersQueryHandlerUnitTests()
        {
            _validator = new GetPaginatedOrdersRequestValidator();
            _mockOrderRepository = new Mock<IEFRepository<Order, Guid>>();
            _mockLogger = new Mock<ILogger<GetPaginatedOrdersQueryHandler>>();
            _handler = new GetPaginatedOrdersQueryHandler(
                _mockOrderRepository.Object, _validator, _mockLogger.Object);
        }

        [Theory, AutoOrderData]
        public async Task Should_GetValidPaginatedOrders_WhenRequested(List<Order> orders)
        {
            //Arrange
            var pageNumber = 1;
            var pageSize = 3;
            var query = new GetPaginatedOrdersQuery(
                new GetPaginatedOrdersRequest(
                    pageNumber, pageSize, null, null));

            var expectedResult = new PaginatedResult<OrderViewModel>(
                orders.Select(o => new OrderViewModel(
                    o.Id,
                    o.Items.Select(i => new ProductDto(i.ProductId, i.Price, i.Quantity)).ToList(),
                    o.Status.ToString(),
                    o.TotalPrice,
                    o.CreatedAtUtc,
                    o.UpdatedAtUtc,
                    o.Email)),
                orders.Count(),
                pageNumber,
                pageSize);

            _mockOrderRepository
                .Setup(repo => repo.GetPaginated<OrderViewModel>(
                    It.Is<PaginationRequest>(r => r.PageNumber == pageNumber && r.PageSize == pageSize),
                    It.IsAny<Expression<Func<Order, OrderViewModel>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                    ))
                .ReturnsAsync(expectedResult);

            //Act
            var actualResult = await _handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.NotNull(actualResult);
            Assert.NotNull(actualResult.Items);
            Assert.Equal(expectedResult.TotalCount, actualResult.TotalCount);
            _mockOrderRepository.VerifyAll();
        }


        [Fact]
        public async Task Should_ThrowValidationException_WhenInvalidSortByProvided()
        {
            // Arrange
            var query = new GetPaginatedOrdersQuery(
                new GetPaginatedOrdersRequest(
                    1, 10, null, "invalid"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
               _handler.Handle(query, CancellationToken.None));
            Assert.Contains("SortBy", exception.Message);
        }


        [Fact]
        public async Task Should_ThrowValidationException_WhenPageNumberIsZero()
        {
            // Arrange
            var query = new GetPaginatedOrdersQuery(
                new GetPaginatedOrdersRequest(
                    0, 10, null, "invalid"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None));
            Assert.Contains("PageNumber", exception.Message);
            Assert.Contains("must be greater than '0'", exception.Message);
        }
    }
}
