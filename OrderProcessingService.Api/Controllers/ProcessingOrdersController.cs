using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;

namespace OrderProcessingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessingOrdersController(
        IOrderProcessor orderProcessor,
        IProcessingOrderQueryService queryService
        ): ControllerBase
    {
        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<ProcessingOrderViewModel>> GetProcessingOrderAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await queryService.GetProcesingOrderById(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ProcessingOrderViewModel>>> GetProcessingOrdersAsync(
            [FromQuery] GetPaginatedProcessingOrdersRequest query,
            CancellationToken cancellationToken)
        {
            var result = await queryService.GetProcessingOrdersPaginatedAsync(query, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:Guid}/begin-assembly")]
        public async Task<ActionResult> BeginAssembly(Guid id, CancellationToken cancellationToken)
        {
            await orderProcessor.BeginAssembly(id, cancellationToken);
            return Accepted();
        }

        [HttpPatch("begin-delivery")]
        public async Task<ActionResult> BeginDelivery([FromBody] DeliveryRequest request, CancellationToken cancellationToken)
        {
            if (!request.Ids.Any()) 
            {
                return BadRequest(new { Message = "Delivery request is empty" });
            }
            await orderProcessor.BeginDelivery(request.Ids, cancellationToken);
            return Accepted();
        }
    }
}
