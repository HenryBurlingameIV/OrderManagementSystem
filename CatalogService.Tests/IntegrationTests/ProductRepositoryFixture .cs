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
    public class ProductRepositoryFixture : IAsyncLifetime
    {
        public CatalogDBContext Context { get; private set; }
        public IRepository<Product> ProductRepository { get; private set; }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<CatalogDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new CatalogDBContext(options);
            ProductRepository = new ProductRepository(Context, new ProductValidator(Context)); 
            await Context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await Context.DisposeAsync();
        }
        public async Task ResetDatabase()
        {
            await Context.Database.EnsureDeletedAsync();
            Context.ChangeTracker.Clear();
        }
    }
}
