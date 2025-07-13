using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Tests.IntegrationTests
{
    public class OrderRepositoryFixture : IAsyncLifetime
    {
        public OrderDbContext DbContext { get; private set; }

        public OrderRepository OrderRepository { get; private set; }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            DbContext = new OrderDbContext(options);
            OrderRepository = new OrderRepository(DbContext);
            await DbContext.Database.EnsureCreatedAsync();
        }

        public async Task ResetDataBase()
        {
            await DbContext.Database.EnsureDeletedAsync();
            DbContext.ChangeTracker.Clear();
        }
    }
}
