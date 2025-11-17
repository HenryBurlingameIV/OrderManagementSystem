using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Contracts
{
    public interface IOrderProcessor
    {
        Task BeginAssembly(Guid id, CancellationToken cancellationToken);

        Task BeginDelivery(List<Guid> ids, CancellationToken cancellationToken);
    }
}
