using MediatR;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OrderService.Application.Commands.CreateOrderCommand
{
    public record CreateOrderCommand(CreateOrderRequest Request) : IRequest<Guid>;

}
