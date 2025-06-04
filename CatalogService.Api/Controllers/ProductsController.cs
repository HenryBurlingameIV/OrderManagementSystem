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
        public async Task<ActionResult<Guid>> CreateProductAsync([FromBody] ProductCreateRequest request)
        {
            var result = await _productService.CreateProductAsync(request);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductViewModel>> GetProductByIdAsync([FromRoute] Guid id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateProductAsync([FromRoute] Guid id, [FromBody] ProductUpdateRequest request)
        {
            var result = await _productService.UpdateProductAsync(id, request);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/quantity")]
        public async Task<ActionResult> UpdateProductQuantityAsync([FromRoute] Guid id, [FromBody] ProductUpdateQuantityRequest request)
        {
            await _productService.UpdateProductQuantityAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteProductAsync([FromRoute] Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }

    }
}
