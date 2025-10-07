using MediatR;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Queries.OrdersPaginatedQuery
{
    public record GetPaginatedOrdersQuery(GetPaginatedOrdersRequest Request) : IRequest<PaginatedResult<OrderViewModel>>;

}
