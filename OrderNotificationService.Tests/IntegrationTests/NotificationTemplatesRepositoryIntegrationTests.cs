using OrderManagementSystem.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Tests.IntegrationTests
{
    public class NotificationTemplatesRepositoryIntegrationTests : IClassFixture<NotificationTemplatesRepositoryFixture>
    {
        private readonly NotificationTemplatesRepositoryFixture _fixture;

        public NotificationTemplatesRepositoryIntegrationTests(NotificationTemplatesRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(OrderStatus.New, "Ваш заказ {OrderId} создан.")]
        [InlineData(OrderStatus.Cancelled, "Ваш заказ {OrderId} отменен.")]
        [InlineData(OrderStatus.Processing, "Ваш заказ {OrderId} находится в обработке.")]
        [InlineData(OrderStatus.Ready, "Обработка вашего заказа {OrderId} завершена. Заказ готов к доставке.")]
        [InlineData(OrderStatus.Delivering, "Ваш заказ {OrderId} находится в процессе доставки.")]
        [InlineData(OrderStatus.Delivered, "Ваш заказ {OrderId} доставлен по указанному адресу.")]
        public async Task Should_ReturnNotificationTemplate_WhenExists(OrderStatus status, string expectedTemplateText)
        {
            //Act
            var actualTemplate = await _fixture.NotificationTemplatesRepository.GetNotificationTemplateByIdAsync((int)status, CancellationToken.None);

            //Assert
            Assert.NotNull(actualTemplate);
            Assert.Equal(expectedTemplateText, actualTemplate?.TemplateText);
        }

        [Fact]
        public async Task Should_ReturnNull_WhenTemplateNotFound()
        {
            //Act
            var actualTemplate = await _fixture.NotificationTemplatesRepository.GetNotificationTemplateByIdAsync(10, CancellationToken.None);

            //Assert
            Assert.Null(actualTemplate);
        }
    }

}

