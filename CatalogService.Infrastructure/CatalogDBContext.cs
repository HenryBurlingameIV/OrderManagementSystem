using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain;

namespace CatalogService.Infrastructure
{
    public class CatalogDBContext : DbContext
    {
        public CatalogDBContext(DbContextOptions options) : base (options){ }
        DbSet<Product> Products { get; set; }

    }
}

    
