using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
using OrderService.Application.Queries.OrderQuery;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.UnitTests
{
    public class GetOrderByIdQueryHandlerTests
    {
        private Mock<IRepository<Order>> _mockOrderRepository;
        private GetOrderByIdQueryHandler _handler;

        public GetOrderByIdQueryHandlerTests()
        {
            _mockOrderRepository = new Mock<IRepository<Order>>();
            _handler = new GetOrderByIdQueryHandler(_mockOrderRepository.Object);
        }


    }
}
