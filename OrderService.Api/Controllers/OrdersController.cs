using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem.Shared.Enums;
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
            var command = new CreateOrderCommand() 
            { 
                OrderItems = request.Items, 
                Email = request.Email 
            };
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtRoute("GetOrder", new { id = result }, result);
        }

        [HttpGet("{id:Guid}", Name = "GetOrder")]
        public async Task<ActionResult<OrderViewModel>> GetOrderAsync(
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
            if(!Enum.TryParse<OrderStatus>(request.OrderStatus, true, out var newStatus))
                return BadRequest(new {Message = "Invalid status value" });

            await mediator.Send(
                new UpdateOrderStatusCommand(id, newStatus), 
                cancellationToken);
            return NoContent();
        }

    }
}
