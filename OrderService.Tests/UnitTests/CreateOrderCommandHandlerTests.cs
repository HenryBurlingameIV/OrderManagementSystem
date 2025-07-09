using FluentValidation;
using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Contracts;
using OrderService.Application.DTO;
using OrderService.Application.Validators;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.UnitTests
{
    public class CreateOrderCommandHandlerTests
    {
        private CreateOrderCommandHandler _handler;
        private IValidator<CreateOrderCommand> _validator;
        private Mock<IRepository<Order>> _mockRepository;
        private Mock<IKafkaProducer<OrderEvent>> _mockKafkaProducer;
        private Mock<ICatalogServiceApi> _mockCatalogServiceApi;
        public CreateOrderCommandHandlerTests() 
        {
            _validator = new CreateOrderCommandValidator();
            _mockRepository = new Mock<IRepository<Order>>();
            _mockKafkaProducer = new Mock<IKafkaProducer<OrderEvent>>();
            _mockCatalogServiceApi = new Mock<ICatalogServiceApi>();
            _handler = new CreateOrderCommandHandler(
                _mockRepository.Object, 
                _mockCatalogServiceApi.Object, 
                _validator, 
                _mockKafkaProducer.Object);
        }

        private List<OrderItemRequest> GenerateOrderItemRequests(int count)
        {
            var random = new Random();
            return Enumerable
                .Range(0, count)
                .Select(i =>
                    new OrderItemRequest(Guid.NewGuid(), random.Next(0, 10)))   
                .ToList();
        }

    }
}
