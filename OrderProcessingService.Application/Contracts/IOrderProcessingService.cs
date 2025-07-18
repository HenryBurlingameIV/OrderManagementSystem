using OrderProcessingService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Contracts
{
    public interface IOrderProcessingInitializer
    {
        Task InitializeProcessingAsync(OrderDto dto);
    }
}
