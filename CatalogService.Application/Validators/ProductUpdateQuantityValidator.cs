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
            RuleFor(p => p.Quantity).NotEmpty().WithMessage("Quiantity is required");
            RuleFor(p => p.Quantity).Must(q => q > 0).WithMessage("Quantity must be greater than zero");
        }
    }
}
