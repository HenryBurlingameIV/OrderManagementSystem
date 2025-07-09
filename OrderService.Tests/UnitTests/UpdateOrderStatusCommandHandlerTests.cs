using FluentValidation;
using Moq;
using OrderManagementSystem.Shared.Contracts;
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
    public class UpdateOrderStatusCommandHandlerTests
    {
        private IValidator<OrderStatusValidationModel> _validator;
        private Mock<IRepository<Order>> _mockOrderRepository;


        public UpdateOrderStatusCommandHandlerTests()
        {
            _validator = new OrderStatusTransitionValidator();
            _mockOrderRepository = new Mock<IRepository<Order>>();
        }

        
    }
}
