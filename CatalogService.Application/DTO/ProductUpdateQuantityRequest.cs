using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.DTO
{
    public class ProductUpdateQuantityRequest
    {
        public int DeltaQuantity { get; set; }
    }
}
