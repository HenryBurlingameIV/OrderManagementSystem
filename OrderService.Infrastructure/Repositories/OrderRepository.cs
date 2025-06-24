using OrderService.Domain.Entities;
using OrderService.Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        private OrderDbContext _orderDbContext;

        public OrderRepository(OrderDbContext orderDbContext) 
        {
            _orderDbContext = orderDbContext;
        }
        public async Task<Guid> CreateAsync(Order item, CancellationToken cancellationToken)
        {
            await _orderDbContext.Orders.AddAsync(item, cancellationToken);
            await _orderDbContext.SaveChangesAsync();
            return item.Id;
        }

        public async Task<Order> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(Order item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
