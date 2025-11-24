using FluentValidation;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Validators
{
    public class GetPaginatedOrdersRequestValidator : AbstractValidator<GetPaginatedOrdersRequest>
    {
        public GetPaginatedOrdersRequestValidator()
        {
            RuleFor(r => r.PageNumber).GreaterThan(0);
            RuleFor(r => r.PageSize).InclusiveBetween(1, 100);

            RuleFor(r => r.Search)
                .MaximumLength(50)
                .When(r => !string.IsNullOrEmpty(r.Search));

            RuleFor(r => r.SortBy)
                .Must(BeValidSortProperty)
                .WithMessage("SortBy must be one of: email, status, price, created.")
                .When(r => !string.IsNullOrEmpty(r.SortBy));
        }
        private bool BeValidSortProperty(string? sortBy)
        {
            var validProperties = new string[] { "email", "status", "price", "created" };
            return validProperties.Contains(sortBy?.ToLower());
        }
    }
}
