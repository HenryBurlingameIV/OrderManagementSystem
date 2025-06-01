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
        public async Task<ActionResult<Guid>> CreateProductAsync([FromBody]ProductCreateRequest request)
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
        public async Task<ActionResult<ProductViewModel>> GetProductByIdAsync([FromRoute]Guid id)
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

    }
}
