using OrderNotificationService.Application.Services;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Tests.UnitTests
{
    public class MessageTemplateRendererTests
    {
        private readonly MessageTemplateRenderer _messageTemapleteRenderer;

        public MessageTemplateRendererTests()
        {
            _messageTemapleteRenderer = new MessageTemplateRenderer();
        }

        [Theory]
        [InlineData("Ваш заказ {OrderId} создан.", "123", "Ваш заказ 123 создан.")]
        [InlineData("Ваш заказ {OrderId} отменен.", "123", "Ваш заказ 123 отменен.")]
        [InlineData("Ваш заказ {OrderId} находится в процессе доставки.", "123", "Ваш заказ 123 находится в процессе доставки.")]
        public void Should_ReturnTemplateWithOneFilledPlaceholder(string template, string orderId, string expectedTemplate)
        {
            //Arrange
            var values = new Dictionary<string, string>()
            {
                {"OrderId", orderId},
            };

            //Act
            var actualTemplate = _messageTemapleteRenderer.Render(template, values);

            //Assert
            Assert.Equal(expectedTemplate, actualTemplate);
        }

        [Theory]
        [InlineData("Заказ {OrderId} {Status}.", "123", "готов", "Заказ 123 готов.")]
        [InlineData("{Status}. Заказ: {OrderId}.", "123", "Доставка завершена", "Доставка завершена. Заказ: 123.")]
        public void Should_ReplaceMultiplePlaceholders(string template, string orderId, string status, string expectedTemplate)
        {
            //Arrange
            var values = new Dictionary<string, string>()
            {
                {"OrderId", orderId},
                {"Status", status}
            };

            //Act
            var actualTemplate = _messageTemapleteRenderer.Render(template, values);

            //Assert
            Assert.Equal(expectedTemplate, actualTemplate);
        }

        [Fact]
        public void Should_KeepPlaceholders_WhenValuesNotFound()
        {
            //Arrange
            var template = "Заказ {OrderId} {Status}.";
            var values = new Dictionary<string, string>()
            {
                { "Status", "доставлен" }
            };

            var expectedTemplate = "Заказ {OrderId} доставлен.";

            //Act
            var actualTemplate = _messageTemapleteRenderer.Render(template, values);

            //Assert
            Assert.Equal(expectedTemplate, actualTemplate);
        }
    }
}
