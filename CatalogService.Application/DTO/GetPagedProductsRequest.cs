using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.DTO
{
    public record GetPagedProductsRequest(int PageNumber, int PageSize, string? Search, string? SortBy);

}
