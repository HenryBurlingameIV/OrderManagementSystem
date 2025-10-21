using CatalogService.Domain;
using OrderManagementSystem.Shared.Contracts;
using CatalogService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using OrderManagementSystem.Shared.DataAccess;

namespace CatalogService.Tests.IntegrationTests
{
    public class ProductRepositoryFixture : IAsyncLifetime
    {
        public CatalogDbContext Context { get; private set; }
        public IEFRepository<Product, Guid> ProductRepository { get; private set; }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Context = new CatalogDbContext(options);
            ProductRepository = new Repository<Product, Guid>(Context); 
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
