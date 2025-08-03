using Microsoft.EntityFrameworkCore;
using OrderProcessingService.Infrastructure;
using OrderProcessingService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.IntegrationTests
{
    public class ProcessingOrderRepositoryFixture : IAsyncLifetime
    {
        public OrderProcessingDbContext DbContext { get; private set; }
        public ProcessingOrderRepository ProcessingOrderRepository { get; private set; }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder()
                .UseNpgsql("Host=localhost;Database=order_processing_test_db;Port=5432;Username=postgres;Password=0807")
                .Options;

            DbContext = new OrderProcessingDbContext(options);

            ProcessingOrderRepository = new ProcessingOrderRepository(DbContext);
            await DbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }

        public async Task ResetDataBase()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            DbContext.ChangeTracker.Clear();
        }

    }
}
