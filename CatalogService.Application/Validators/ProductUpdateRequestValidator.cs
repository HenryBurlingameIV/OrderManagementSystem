using CatalogService.Application.DTO;
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
            RuleFor(p => p.Name).NotEmpty().When(p => p.Name != null).WithMessage("Name must not be empty");
            RuleFor(p => p.Description).MaximumLength(200).When(p =>p.Description != null).WithMessage("Description must not exceed 200 characters.");
            RuleFor(p => p.Category).NotEmpty().When(p => p.Category != null).WithMessage("Category must not be empty");
            RuleFor(p => p.Category).MaximumLength(100).When(p => p.Category != null).WithMessage("Category name must not exceed 100 characters.");
            RuleFor(p => p.Quantity).Must(q => q.Value >= 0).When(p => p.Quantity != null).WithMessage("Quantity must be non-negative.");
            RuleFor(p => p.Price).GreaterThan(0).When(p => p.Price != null).WithMessage("Price must be greater than zero.");
        }
    }
}
