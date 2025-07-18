using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Contracts
{
    public interface IMessageHandler<in TMessage>
    {
        Task HandleAsync(TMessage message, CancellationToken cancellationToken);
    }
}
