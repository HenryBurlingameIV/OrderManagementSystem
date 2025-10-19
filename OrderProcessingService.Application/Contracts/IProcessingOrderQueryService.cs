using OrderManagementSystem.Shared.DataAccess.Pagination;
using OrderProcessingService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Contracts
{
    public interface IProcessingOrderQueryService
    {
        Task<ProcessingOrderViewModel> GetProcesingOrderById(Guid id, CancellationToken ct);

        Task<PaginatedResult<ProcessingOrderViewModel>>GetProcessingOrdersPaginatedAsync(GetPaginatedProcessingOrdersRequest query, CancellationToken ct);
    }
}
