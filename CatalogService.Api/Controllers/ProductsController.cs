using Azure.Core;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;  
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateProductAsync(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.CreateProductAsync(request, cancellationToken);
            return CreatedAtRoute("GetProduct", new { id = result}, result);
        }

        [HttpGet("{id:guid}", Name = "GetProduct")]
        public async Task<ActionResult<ProductViewModel>> GetProductAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.GetProductByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ProductViewModel>> GetProductsAsync(
            [FromQuery] GetPagindatedProductsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _productService.GetProductsPaginatedAsync(
                request,
                cancellationToken);
            return Ok(result);
        }
            

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateProductAsync(
            [FromRoute] Guid id, 
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken
            )
        {
            
            var result = await _productService.UpdateProductAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/reserve")]
        public async Task<ActionResult<ProductViewModel>> ReserveProductAsync(
            [FromRoute] Guid id,
            [FromBody] ReserveProductRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.ReserveProductAsync(id, request.Quantity, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/release")]
        public async Task<ActionResult<ProductViewModel>> ReleaseProductAsync(
            [FromRoute] Guid id,
            [FromBody] ReserveProductRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.ReleaseProductAsync(id, request.Quantity, cancellationToken);
            return Ok(result);
        }


        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteProductAsync(
            [FromRoute] Guid id, 
            CancellationToken cancellationToken
            )
        {
            await _productService.DeleteProductAsync(id, cancellationToken);
            return NoContent();
        }

    }
}
