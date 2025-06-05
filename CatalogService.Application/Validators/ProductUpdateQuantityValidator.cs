using CatalogService.Application.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Validators
{
    public class ProductUpdateQuantityValidator : AbstractValidator<ProductUpdateQuantityRequest>
    {
        public ProductUpdateQuantityValidator()
        {
            RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0);
        }
    }
}
