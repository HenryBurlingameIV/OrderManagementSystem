using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Contracts
{
    public interface IKafkaProducer <in TMessage> : IDisposable
    {
        Task ProduceAsync(Guid key, TMessage message, CancellationToken cancellationToken);
    }
}
