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
    public class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
    {
        public ProductUpdateRequestValidator() 
        {
            RuleFor(p => p.Name).NotEmpty().When(p => p.Name != null);
            RuleFor(p => p.Description).MaximumLength(200).When(p =>p.Description != null);
            RuleFor(p => p.Category).NotEmpty().When(p => p.Category != null);
            RuleFor(p => p.Category).MaximumLength(100).When(p => p.Category != null);
            RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0).When(p => p.Quantity != null);
            RuleFor(p => p.Price).GreaterThan(0).When(p => p.Price != null);

        }
    }
}
