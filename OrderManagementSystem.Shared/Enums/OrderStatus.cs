using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Enums
{
    public enum OrderStatus
    {
        New,
        Cancelled,
        Processing,
        Ready,
        Delivering,
        Delivered
    }
}
