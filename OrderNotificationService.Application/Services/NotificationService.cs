using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Exceptions;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Application.Services
{
    public class NotificationService : INotificationService<NotificationRequest>
    {
        private readonly INotificationTemplatesRepository _repository;
        private readonly IEmailMessageSender _sender;
        private readonly IMessageTemplateRenderer _messageTemplateRenderer;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationTemplatesRepository repository,
            IEmailMessageSender sender, 
            IMessageTemplateRenderer renderer,
            ILogger<NotificationService> logger)
        {
            _repository = repository;
            _sender = sender;
            _messageTemplateRenderer = renderer;
            _logger = logger;
            
        }
        public async Task NotifyAsync(NotificationRequest request, CancellationToken ct)
        {
            var template = await _repository.GetNotificationTemplateByIdAsync(request.OrderStatus, ct);
            if (template == null)
                throw new NotFoundException($"Template with Id {request.OrderStatus} not found.");

            var text = template.TemplateText;
            var message = _messageTemplateRenderer.Render(text, request);


            await _sender.SendAsync(message, request.Email);
            _logger.LogInformation("Notification sent successfully for order {OrderId} to {Email}.", request.OrderId, request.Email);
        }
    }
}
