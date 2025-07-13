using FluentValidation;
using OrderService.Application.Commands.CreateOrderCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Validators
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(command => command.OrderItems)
                .NotNull()
                .DependentRules(() =>
                {
                    RuleFor(command => command.OrderItems)
                        .NotEmpty()
                        .Must(items => items.Count < 100);                       
                    RuleForEach(command => command.OrderItems)
                        .SetValidator(new OrderItemRequestValidator());
                });

        }
    }
}
