using FluentValidation;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Validators
{
    public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
    {
        public OrderItemRequestValidator() 
        {
            RuleFor(request => request.Id)
                .NotEmpty();
            RuleFor(request => request.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(1000);           
        }
    }
}
