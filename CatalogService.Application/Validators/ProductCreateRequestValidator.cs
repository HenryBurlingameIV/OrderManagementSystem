using CatalogService.Application.DTO;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using FluentValidation;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Validators
{
    public class ProductCreateRequestValidator : AbstractValidator<ProductCreateRequest>
    {

        public ProductCreateRequestValidator() 
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(100);
              
            RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(p => p.Price).GreaterThan(0);
            RuleFor(p => p.Category).NotEmpty();
            RuleFor(p => p.Category).MaximumLength(100);
            RuleFor(p => p.Description).MaximumLength(200);

        }
    }
}
