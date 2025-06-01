using CatalogService.Application.Contracts;
using CatalogService.Application.DTO;
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
        public async Task<ActionResult<Guid>> CreateProductAsync(ProductCreateRequest request)
        {
            var result =  await _productService.CreateProductAsync(request);
            return Ok(result);
        }

    }
}
