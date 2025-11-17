using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderNotificationService.Application.Contracts
{
    public interface INotificationService<T>
    {
        Task NotifyAsync(T request, CancellationToken ct);
    }
}
