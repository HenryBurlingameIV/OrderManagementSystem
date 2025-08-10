using FluentValidation;
using OrderService.Application.Commands.CreateOrderCommand;
using System.Text.RegularExpressions;
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

            RuleFor(command => command.Email)
                .NotNull()
                .DependentRules(() =>
                {
                    RuleFor(command => command.Email)
                        .NotEmpty()
                        .MaximumLength(254)
                        .Must(email =>
                        {
                            string pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
                            return Regex.IsMatch(email, pattern);
                        })
                        .WithMessage("Invalid email format.");
                });
        }
    }
}
