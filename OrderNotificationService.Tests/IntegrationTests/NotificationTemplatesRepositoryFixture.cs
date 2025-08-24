using Microsoft.EntityFrameworkCore;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Infrastructure;
using OrderNotificationService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Tests.IntegrationTests
{
    public class NotificationTemplatesRepositoryFixture : IDisposable
    {
        private readonly OrderNotificationDbContext _orderNotificationDbContext;
        private readonly INotificationTemplatesRepository _notificationTemplatesRepository;

        public NotificationTemplatesRepositoryFixture()
        {
            var options =  new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _orderNotificationDbContext = new OrderNotificationDbContext(options);
            _notificationTemplatesRepository = new NotificationTemplatesRepository(_orderNotificationDbContext);
        }

        public void Dispose()
        {
            _orderNotificationDbContext.Dispose();
        }
    }
}
