using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess;
using OrderService.Domain.Entities;
using OrderService.Infrastructure;
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

        public IEFRepository<Order, Guid> OrderRepository { get; private set; }

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
            OrderRepository = new Repository<Order,Guid>(DbContext);
            await DbContext.Database.EnsureCreatedAsync();
        }

        public async Task ResetDataBase()
        {
            await DbContext.Database.EnsureDeletedAsync();
            DbContext.ChangeTracker.Clear();
        }
    }
}
