using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTO
{
    public record class GetPaginatedOrdersRequest(int PageNumber, int PageSize, string? Search, string? SortBy);

}
