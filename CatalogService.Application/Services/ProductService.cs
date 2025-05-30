using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Contracts;


namespace CatalogService.Application.Services
{
    public class ProductService
    {
        private IRepository<Product> _repository;

        public ProductService(IRepository<Product> repo)
        {
            _repository = repo;
        }


    }
}
