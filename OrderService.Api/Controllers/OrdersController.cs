using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem.Shared.Authorization;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.Enums;
using OrderService.Application.Commands.CreateOrderCommand;
using OrderService.Application.Commands.UpdateOrderStatusCommand;
using OrderService.Application.DTO;
using OrderService.Application.Queries.OrderQuery;
using OrderService.Application.Queries.OrdersPaginatedQuery;
using OrderService.Domain.Entities;

namespace OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IMediator mediator,
        ICurrentUser currentUser
        ) : ControllerBase
    {
        [HttpPost]
        [Authorize(Policy = Permissions.Orders.Create)]
        public async Task<ActionResult<Guid>> CreateOrder(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateOrderCommand(
                request, currentUser.Email, currentUser.UserId);
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtRoute("GetOrder", new { id = result }, result);
        }

        [HttpGet("{id:Guid}", Name = "GetOrder")]
        [Authorize(Policy = Permissions.Orders.ReadAll)]
        public async Task<ActionResult<OrderViewModel>> GetOrder(
            [FromRoute]
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetOrderByIdQuery(id, null), 
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:Guid}/my", Name = "GetMyOrder")]
        [Authorize(Policy = Permissions.Orders.ReadOwn)]
        public async Task<ActionResult<OrderViewModel>> GetMyOrder(
            [FromRoute]
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetOrderByIdQuery(id, currentUser.UserId),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy =  Permissions.Orders.ReadAll)]
        public async Task<ActionResult<OrderViewModel>> GetOrders(
            [FromQuery]GetPaginatedOrdersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPaginatedOrdersQuery(request, null),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("my")]
        [Authorize(Policy = Permissions.Orders.ReadOwn)]
        public async Task<ActionResult<OrderViewModel>> GetMyOrders(
            [FromQuery] GetPaginatedOrdersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPaginatedOrdersQuery(request, currentUser.UserId), 
                cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Policy = Permissions.Orders.UpdateStatusAll)]
        public async Task<ActionResult> UpdateOrderStatus(
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
