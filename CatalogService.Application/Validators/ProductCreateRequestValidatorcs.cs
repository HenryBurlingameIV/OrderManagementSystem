using CatalogService.Application.DTO;
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
        public ProductCreateRequestValidator() 
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(p => p.Quantity).Must(q => q >= 0).WithMessage("Quantity must be non-negative.");
            RuleFor(p => p.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
            RuleFor(p => p.Category).NotEmpty().WithMessage("Category is required.");
            RuleFor(p => p.Category).MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
            RuleFor(p => p.Description).MaximumLength(200).WithMessage("Description name must not exceed 200 characters.");
        }
    }
}
