using FluentValidation;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
using OrderService.Application.DTO;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Validators
{
    public class OrderStatusValidator : AbstractValidator<OrderStatusValidationModel>
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


        public OrderStatusValidator()
        {
            RuleFor(s => s.NewStatus).NotEmpty();
            RuleFor(s => s)
                .Must(s => BeValidStatus(s.NewStatus, s.CurrentStatus))
                .WithMessage(s => 
                    $"Cannot change order status from '{s.CurrentStatus}' to '{s.NewStatus}'.");
        }

        private bool BeValidStatus(string newStatus, OrderStatus currentStatus)
        {
            return Enum.TryParse<OrderStatus>(newStatus, true, out var status)
               && StatusTransitionRules[currentStatus].Contains(status);

        }
    }
}
