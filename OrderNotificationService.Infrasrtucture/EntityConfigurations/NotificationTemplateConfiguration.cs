using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementSystem.Shared.Enums;
using OrderNotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Infrasrtucture.EntityConfigurations
{
    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
        {
            builder
                .HasKey(n => n.Id);

            builder
                .Property(n => n.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder
                .Property(n => n.TemplateText)
                .IsRequired()
                .HasMaxLength(200);

            builder.
                HasData(
                new NotificationTemplate()
                {
                    Id = (int)OrderStatus.New,
                    Name = "Создан",
                    TemplateText = "Ваш заказ {OrderId} создан."
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
                },
                new NotificationTemplate()
                {
                    Id = (int)OrderStatus.Cancelled,
                    Name = "Отменен",
                    TemplateText = "Ваш заказ {OrderId} отменен."
                }
                );
        }
    }
}
