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
        private readonly IRepository<Product> _repository;

        public ProductCreateRequestValidator(IRepository<Product> repository) 
        {
            _repository = repository;
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(100)
                .MustAsync(BeUniqueName)
                .WithMessage("Product with name {ProperyValue} already exists");

                

            RuleFor(p => p.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(p => p.Price).GreaterThan(0);
            RuleFor(p => p.Category).NotEmpty();
            RuleFor(p => p.Category).MaximumLength(100);
            RuleFor(p => p.Description).MaximumLength(200);

        }

        private async Task<bool> BeUniqueName(string name, CancellationToken ct)
        {
            var normalizeName = name.ToLower().Trim();
            return !await _repository.ExistsAsync(p =>  normalizeName == p.Name.ToLower(), ct);
        }
    }
}
