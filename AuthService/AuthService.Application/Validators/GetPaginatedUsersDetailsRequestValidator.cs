using AuthService.Application.Contracts;
using AuthService.Application.DTO;
using FluentValidation;
using OrderManagementSystem.Shared.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Validators
{
    public class GetPaginatedUsersDetailsRequestValidator : AbstractValidator<GetPaginatedUsersDetailsRequest>
    {
        public GetPaginatedUsersDetailsRequestValidator()
        {
            RuleFor(r => r.PageNumber).GreaterThan(0);
            RuleFor(r => r.PageSize).InclusiveBetween(1, 100);
            RuleFor(r => r.Search)
               .MaximumLength(50)
               .When(r => !string.IsNullOrEmpty(r.Search));

            RuleFor(r => r.SortBy)
               .Must(BeValidSortProperty)
               .WithMessage("SortBy must be one of: name, email.")
               .When(r => !string.IsNullOrEmpty(r.SortBy));

            RuleFor(r => r.Role)
              .Must(BeValidRole)
              .WithMessage($"Invalid role specified. Available roles: {Roles.Admin}, {Roles.Manager}, {Roles.Client}.")
              .When(r => !string.IsNullOrEmpty(r.Role));
        }

        private bool BeValidSortProperty(string? sortBy)
        {
            var validProperties = new string[] { "Name", "Email"};
            return validProperties.Contains(sortBy?.ToLower());
        }

        private bool BeValidRole(string? role)
        {
            var validRoles = new string[] { Roles.Admin, Roles.Manager, Roles.Client };
            return validRoles.Contains(role);
        }
    }
}
