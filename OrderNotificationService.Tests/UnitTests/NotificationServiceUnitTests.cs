using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Application.DTO;
using OrderNotificationService.Application.Services;
using OrderNotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Tests.UnitTests
{
    public class NotificationServiceUnitTests
    {
        private readonly Mock<INotificationTemplatesRepository> _mockRepository;
        private readonly Mock<IEmailMessageSender> _mockEmailMessageSender;
        private readonly Mock<IMessageTemplateRenderer> _mockMessageTemplateRenderer;
        private readonly Mock<ILogger<NotificationService>> _mockLogger;
        private readonly NotificationService _notificationService;

        public NotificationServiceUnitTests() 
        {
            _mockRepository = new Mock<INotificationTemplatesRepository>();
            _mockEmailMessageSender = new Mock<IEmailMessageSender>();
            _mockMessageTemplateRenderer = new Mock<IMessageTemplateRenderer>();
            _mockLogger = new Mock<ILogger<NotificationService>>();
            _notificationService = new NotificationService(
                _mockRepository.Object,
                _mockEmailMessageSender.Object,
                _mockMessageTemplateRenderer.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Should_Notify_WhenTemplateExists()
        {
            //Arrange
            var orderId = Guid.NewGuid();
            var email = "test@gmail.com";
            var orderStatus = 1;
            var request = new NotificationRequest(orderId, orderStatus, email);
            var templateText = "Ваш заказ {OrderId} создан.";
            var expectedMessage = $"Ваш заказ {orderId} создан.";
            var template = new NotificationTemplate
            {
                Id = orderStatus,
                Name = "Создан",
                TemplateText = templateText
            };

            _mockRepository
                .Setup(repo => repo.GetNotificationTemplateByIdAsync(orderStatus, CancellationToken.None))
                .ReturnsAsync(template);
      

            _mockEmailMessageSender
                .Setup(sender => sender.SendAsync(expectedMessage, email, It.IsAny<CancellationToken>()));

            _mockMessageTemplateRenderer
                .Setup(renderer => renderer
                    .Render(
                        templateText, 
                        It.Is<Dictionary<string, string>>(d =>
                            d.ContainsKey("OrderId") && d["OrderId"] == orderId.ToString())))
                .Returns(expectedMessage);

            //Act
            await _notificationService.NotifyAsync(request, CancellationToken.None);

            //Assert
            _mockRepository.VerifyAll();
            _mockEmailMessageSender.VerifyAll();
            _mockMessageTemplateRenderer.VerifyAll();               
        }
    }
}
