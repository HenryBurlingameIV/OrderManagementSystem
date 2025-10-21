using OrderProcessingService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Contracts
{
    public interface IOrderBackgroundWorker<TCommand>
    {
        Task ScheduleAsync(TCommand command, CancellationToken cancellationToken);
        Task ProcessAsync(TCommand command, CancellationToken cancellationToken);
    }

}
