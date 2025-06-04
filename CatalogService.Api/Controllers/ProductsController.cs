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
        private IValidator<ProductCreateRequest> _createValidator;
        private IValidator<ProductUpdateRequest> _updateValidator;
        private IValidator<ProductUpdateQuantityRequest> _quantityValidator;

        public ProductsController(
            IProductService productService,
            IValidator<ProductCreateRequest> createValidator,
            IValidator<ProductUpdateRequest> updateValidator,
            IValidator<ProductUpdateQuantityRequest> quantityValidator
            )
        {
            _productService = productService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _quantityValidator = quantityValidator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateProductAsync(
            [FromBody] ProductCreateRequest request,
            CancellationToken cancellationToken
            )
        {
            await _createValidator.ValidateAndThrowAsync(request);
            var result = await _productService.CreateProductAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
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
            await _updateValidator.ValidateAndThrowAsync(request);
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
            await _quantityValidator.ValidateAndThrowAsync(request);
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
