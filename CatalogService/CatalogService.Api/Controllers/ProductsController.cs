using Azure.Core;
using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderManagementSystem.Shared.Authorization;

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
        [Authorize(Policy = Permissions.Catalog.Create)]
        public async Task<ActionResult<Guid>> CreateProduct(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.CreateProductAsync(request, cancellationToken);
            return CreatedAtRoute("GetProduct", new { id = result}, result);
        }

        [HttpGet("{id:guid}", Name = "GetProduct")]
        public async Task<ActionResult<ProductViewModel>> GetProduct(
            [FromRoute] Guid id,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.GetProductByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ProductViewModel>> GetProducts(
            [FromQuery] GetPaginatedProductsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _productService.GetPaginatedProductsAsync(
                request,
                cancellationToken);
            return Ok(result);
        }
            

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Permissions.Catalog.Update)]
        public async Task<ActionResult<Guid>> UpdateProduct(
            [FromRoute] Guid id, 
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken
            )
        {
            
            await _productService.UpdateProductAsync(id, request, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/reserve")]
        [Authorize(Policy = Permissions.Catalog.Update)]
        public async Task<ActionResult<ProductViewModel>> ReserveProduct(
            [FromRoute] Guid id,
            [FromBody] ReserveProductRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.ReserveProductAsync(id, request.Quantity, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/release")]
        [Authorize(Policy = Permissions.Catalog.Update)]
        public async Task<ActionResult<ProductViewModel>> ReleaseProduct(
            [FromRoute] Guid id,
            [FromBody] ReserveProductRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await _productService.ReleaseProductAsync(id, request.Quantity, cancellationToken);
            return Ok(result);
        }


        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Permissions.Catalog.Delete)]
        public async Task<ActionResult> DeleteProduct(
            [FromRoute] Guid id, 
            CancellationToken cancellationToken
            )
        {
            await _productService.DeleteProductAsync(id, cancellationToken);
            return NoContent();
        }

    }
}
