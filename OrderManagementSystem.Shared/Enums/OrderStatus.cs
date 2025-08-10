using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Enums
{
    public enum OrderStatus
    {
        New = 1,
        Cancelled = 2,
        Processing = 3,
        Ready = 4,
        Delivering = 5,
        Delivered = 6
    }
}
