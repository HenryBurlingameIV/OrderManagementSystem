using MediatR;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Queries.OrderQuery
{
    public record GetOrderByIdQuery(Guid Id) : IRequest<OrderViewModel>;

}
