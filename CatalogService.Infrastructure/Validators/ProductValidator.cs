using CatalogService.Domain;
using CatalogService.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogService.Infrastructure.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        private CatalogDbContext _dbContext;
        public ProductValidator(CatalogDbContext dbContext) 
        {
           _dbContext = dbContext;
            RuleFor(product => product).MustAsync(async (product, token) =>
            {
                return !await _dbContext.Products
                    .AnyAsync(p => p.Name == product.Name && p.Id != product.Id, token);
            }).WithMessage("Product name must be unique");


        }
    }
}
