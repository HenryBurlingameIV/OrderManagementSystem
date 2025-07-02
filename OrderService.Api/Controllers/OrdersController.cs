using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
using OrderService.Application.DTO;
using OrderService.Application.Queries.OrderQuery;
using OrderService.Domain.Entities;

namespace OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateOrderAsync(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateOrderCommand() { OrderItems = request.Items };
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtRoute("GetOrder", new { id = result }, result);
        }

        [HttpGet("{id:Guid}", Name = "GetOrder")]
        public async Task<ActionResult<OrderViewModel>> GetOrderByIdAsync(
            [FromRoute]
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetOrderByIdQuery(id), 
                cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult> UpdateOrderStatusAsync(
            [FromRoute]
            Guid id,
            [FromBody]
            NewOrderStatusRequest
            request,
            CancellationToken cancellationToken)
        {
            await mediator.Send(
                new UpdateOrderStatusCommand(id, request.OrderStatus), 
                cancellationToken);
            return NoContent();
        }

    }
}
