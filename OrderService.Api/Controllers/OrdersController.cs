using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.CreateOrderCommand;

namespace OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IMediator mediator) : ControllerBase
    {            


    }
}
