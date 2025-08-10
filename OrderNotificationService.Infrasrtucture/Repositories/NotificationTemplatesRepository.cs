using Microsoft.EntityFrameworkCore;
using OrderNotificationService.Application.Contracts;
using OrderNotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Infrasrtucture.Repositories
{
    public class NotificationTemplatesRepository : INotificationTemplatesRepository
    {
        private readonly OrderNotificationDbContext _orderNotificationDbContext;

        public NotificationTemplatesRepository(OrderNotificationDbContext orderNotificationContext)
        {
            _orderNotificationDbContext = orderNotificationContext;
        }
        public async Task<NotificationTemplate?> GetNotificationTemplateByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _orderNotificationDbContext.NotificationTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }
    }
}
