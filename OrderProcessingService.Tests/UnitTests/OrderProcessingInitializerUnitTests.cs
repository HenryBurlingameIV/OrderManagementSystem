using Moq;
using OrderManagementSystem.Shared.Contracts;
using OrderProcessingService.Application.Services;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class OrderProcessingInitializerUnitTests
    {
        private readonly Mock<IRepository<ProcessingOrder>> _mockRepository;
        private readonly OrderProcessingInitializer _orderProcessingInitializer;

        public OrderProcessingInitializerUnitTests() 
        {
            _mockRepository = new Mock<IRepository<ProcessingOrder>>();
            _orderProcessingInitializer = new OrderProcessingInitializer(_mockRepository.Object);
        }
    }
}
