using AutoFixture;
using CatalogService.Application.DTO;
using CatalogService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Tests.ProductFixture
{
    public class ProductFixtureCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var date = DateTime.UtcNow;
            fixture
                .Customize<Product>(composer => composer
                    .With(p => p.Id, () =>Guid.NewGuid())
                    .With(p => p.Name, () => Guid.NewGuid().ToString())
                    .With(p => p.Price, () => Math.Round((decimal)new Random().NextDouble() * 100, 2))
                    .With(p => p.Quantity, () => new Random().Next(1, 101))
                    .With(p => p.Category, "Electronics")
                    .With(p => p.CreatedDateUtc, date)
                    .With(p => p.UpdatedDateUtc, date)
                    );

            fixture
                .Customize<ProductCreateRequest>(composer => composer
                    .With(p => p.Name, () => Guid.NewGuid().ToString())
                    .With(p => p.Price, () => Math.Round((decimal)new Random().NextDouble() * 100, 2))
                    .With(p => p.Quantity, () => new Random().Next(1, 101))
                    .With(p => p.Category, "Electronics")
                    );

            fixture
                .Customize<ProductUpdateRequest>(composer => composer
                    .With(p => p.Name, () => Guid.NewGuid().ToString())
                    .With(p => p.Price, () => Math.Round((decimal)new Random().NextDouble() * 100, 2))
                    .With(p => p.Quantity, () => new Random().Next(1, 101))
                    .With(p => p.Category, "Groceries")
                );


        }
    }
}
