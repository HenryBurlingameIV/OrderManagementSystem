using Azure.Core;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using CatalogService.Domain.Exceptions;
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
            [FromBody] ProductCreateRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.CreateProductAsync(request, cancellationToken);
            return CreatedAtRoute("GetProduct", new { id = result}, result);
        }

        [HttpGet("{id:guid}", Name = "GetProduct")]
        public async Task<ActionResult<ProductViewModel>> GetProductByIdAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.GetProductByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateProductAsync(
            [FromRoute] Guid id, 
            [FromBody] ProductUpdateRequest request,
            CancellationToken cancellationToken
            )
        {
            
            var result = await _productService.UpdateProductAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/quantity")]
        public async Task<ActionResult> UpdateProductQuantityAsync(
            [FromRoute] Guid id, 
            [FromBody] ProductUpdateQuantityRequest request,
            CancellationToken cancellationToken
            )
        {
            
            await _productService.UpdateProductQuantityAsync(id, request, cancellationToken);
            return NoContent();
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
