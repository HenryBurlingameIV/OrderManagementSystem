using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Commands.CreateOrderCommand
{
    public class CreateOrderCommandHandler() : IRequestHandler<CreateOrderCommand, Guid>
    {
        public Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
