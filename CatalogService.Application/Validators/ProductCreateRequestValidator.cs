using CatalogService.Application.DTO;
using CatalogService.Infrastructure;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Validators
{
    public class ProductCreateRequestValidator : AbstractValidator<ProductCreateRequest>
    {
        public ProductCreateRequestValidator(CatalogDBContext catalogDBContext) 
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(p => p.Price).GreaterThan(0);
            RuleFor(p => p.Category).NotEmpty();
            RuleFor(p => p.Category).MaximumLength(100);
            RuleFor(p => p.Description).MaximumLength(200);

        }
    }
}
