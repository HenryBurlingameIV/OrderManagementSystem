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
            RuleFor(command => command.Items)
                .NotNull()
                .DependentRules(() =>
                {
                    RuleFor(command => command.Items)
                        .NotEmpty()
                        .Must(items => items.Count < 100);                       
                    RuleForEach(command => command.Items)
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
