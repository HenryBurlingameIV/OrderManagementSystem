using CatalogService.Domain;
using OrderManagementSystem.Shared.Contracts;
using CatalogService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Validators;

namespace CatalogService.Tests.IntegrationTests
{
    public class ProductRepositoryFixture : IDisposable
    {
        public CatalogDBContext Context { get; set; }
        public IRepository<Product> ProductRepository { get; set; }

        public ProductRepositoryFixture()
        {
            ResetDatabase().Wait();
        }

        public async Task ResetDatabase()
        {
            if (Context != null)
            {
                await Context.DisposeAsync();
            }

            var options = new DbContextOptionsBuilder<CatalogDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new CatalogDBContext(options);
            ProductRepository = new ProductRepository(Context, new ProductValidator(Context));
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
