using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.CreateOrderCommand;

namespace OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateOrderAsync(
    [FromBody] CreateOrderCommand command,
    CancellationToken cancellationToken)
        {
            var id = await mediator.Send(command, cancellationToken);
            return Ok(id);
        }

    }
}
