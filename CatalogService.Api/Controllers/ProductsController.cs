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
            try
            {
                var result = await _productService.CreateProductAsync(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return ValidationProblem(ex.Message);
            }

        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductViewModel>> GetProductByIdAsync([FromRoute] Guid id)
        {
            try
            {
                var result = await _productService.GetProductByIdAsync(id);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateProductAsync([FromRoute] Guid id, [FromBody] ProductUpdateRequest request)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, request);
                return Ok(result);
            }
            catch(NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(ValidationException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }

        [HttpPatch("{id:guid}/quantity")]
        public async Task<ActionResult> UpdateProductQuantityAsync([FromRoute] Guid id, [FromBody] ProductUpdateQuantityRequest request)
        {
            try
            {
                await _productService.UpdateProductQuantityAsync(id, request);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteProductAsync([FromRoute] Guid id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch(NotFoundException ex) 
            {
                return NotFound(ex.Message);
            }
        }

    }
}
