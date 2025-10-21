using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.DTO
{
    public record ProductCreateRequest(string Name, string Description, string Category, decimal Price, int Quantity);

}
