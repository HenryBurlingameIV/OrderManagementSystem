using FluentValidation;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Validators
{
    public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> StatusTransitionRules = new()
        {
            [OrderStatus.New] = new() { OrderStatus.Cancelled, OrderStatus.Processing },
            [OrderStatus.Cancelled] = new(),
            [OrderStatus.Processing] = new() { OrderStatus.Cancelled, OrderStatus.Ready },
            [OrderStatus.Ready] = new() { OrderStatus.Delivering, OrderStatus.Cancelled },
            [OrderStatus.Delivering] = new() { OrderStatus.Delivered, OrderStatus.Cancelled },
            [OrderStatus.Delivered] = new()
        };


        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(c => c.NewOrderStatus).NotEmpty();
            RuleFor(c => c)
                .Must(c => BeValidStatus(c.NewOrderStatus, c.CurrentOrderStatus))
                .WithMessage(c => 
                    $"Cannot change order status from '{c.CurrentOrderStatus}' to '{c.NewOrderStatus}'. ");
        }

        private bool BeValidStatus(string newStatus, OrderStatus currentStatus)
        {
            return Enum.TryParse<OrderStatus>(newStatus, true, out var status)
               && StatusTransitionRules[currentStatus].Contains(status);

        }
    }
}
