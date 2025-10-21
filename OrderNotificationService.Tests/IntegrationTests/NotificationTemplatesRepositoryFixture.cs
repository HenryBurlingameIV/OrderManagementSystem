using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess;
using OrderManagementSystem.Shared.Enums;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Domain.Entities;
using OrderNotificationService.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Tests.IntegrationTests
{
    public class NotificationTemplatesRepositoryFixture : IDisposable
    {
        public OrderNotificationDbContext OrderNotificationDbContext { get ; private set; }
        public IRepositoryBase<NotificationTemplate, int> NotificationTemplatesRepository { get; private set; }

        public NotificationTemplatesRepositoryFixture()
        {
            var options =  new DbContextOptionsBuilder<OrderNotificationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            OrderNotificationDbContext = new OrderNotificationDbContext(options);
            NotificationTemplatesRepository = new Repository<NotificationTemplate, int>(OrderNotificationDbContext);
            var templates = new List<NotificationTemplate>()
            {
                 new NotificationTemplate()
                {
                    Id = (int)OrderStatus.New,
                    Name = "Создан",
                    TemplateText = "Ваш заказ {OrderId} создан."
                },
                new NotificationTemplate()
                {
                    Id = (int)OrderStatus.Cancelled,
                    Name = "Отменен",
                    TemplateText = "Ваш заказ {OrderId} отменен."
                },
                new NotificationTemplate()
                {
                    Id = (int)OrderStatus.Processing,
                    Name = "В обработке",
                    TemplateText = "Ваш заказ {OrderId} находится в обработке."
                },
                new NotificationTemplate()
                {
                    Id = (int)OrderStatus.Ready,
                    Name = "Готов к доставке",
                    TemplateText = "Обработка вашего заказа {OrderId} завершена. Заказ готов к доставке."
                },
                new NotificationTemplate()
                {
                    Id = (int)OrderStatus.Delivering,
                    Name = "В доставке",
                    TemplateText = "Ваш заказ {OrderId} находится в процессе доставки."
                },
                new NotificationTemplate()
                {
                    Id = (int)OrderStatus.Delivered,
                    Name = "Доставлен",
                    TemplateText = "Ваш заказ {OrderId} доставлен по указанному адресу."
                }
            };
            OrderNotificationDbContext.NotificationTemplates.AddRange(templates);
            OrderNotificationDbContext.SaveChanges();
        }

        public void Dispose()
        {
            OrderNotificationDbContext.Database.EnsureDeleted();
            OrderNotificationDbContext.Dispose();
        }
    }
}
