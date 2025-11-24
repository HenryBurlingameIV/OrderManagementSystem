using FluentValidation;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OrderService.Application.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(r => r.Items)
                .NotNull()
                .DependentRules(() =>
                {
                    RuleFor(r => r.Items)
                        .NotEmpty()
                        .Must(r => r.Count < 100);                       
                    RuleForEach(r => r.Items)
                        .SetValidator(new OrderItemRequestValidator());
                });
        }
    }
}
