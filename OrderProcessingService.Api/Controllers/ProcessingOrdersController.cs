using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;

namespace OrderProcessingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessingOrdersController(IOrderProcessor orderProcessor): ControllerBase
    {
        [HttpPatch("{id:Guid}/begin-assembly")]
        public async Task<ActionResult> BeginAssembly(Guid id, CancellationToken cancellationToken)
        {
            await orderProcessor.BeginAssembly(id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("begin-delivery")]
        public async Task<ActionResult> BeginDelivery([FromBody] DeliveryRequest request, CancellationToken cancellationToken)
        {
            if (!request.Ids.Any()) 
            {
                return BadRequest(new { Message = "Delivery request is empty" });
            }
            await orderProcessor.BeginDelivery(request.Ids, cancellationToken);
            return NoContent();
        }
    }
}
