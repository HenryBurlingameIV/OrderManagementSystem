using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Application.Services;
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
    }
}
