using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderProcessingService.Application.Contracts;

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
        public async Task<ActionResult> BeginDelivery(Guid id, CancellationToken cancellationToken)
        {
            return NoContent();
        }
    }
}
