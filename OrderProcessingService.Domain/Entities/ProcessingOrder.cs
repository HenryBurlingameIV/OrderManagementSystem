using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Domain.Entities
{
    public class ProcessingOrder
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public List<OrderItem> Items { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Stage Stage { get; set; }

        public ProcessingStatus Status { get; set; }

        public string? TrackingNumber { get; set; }
    }

    public enum Stage
    {
        Assembly,
        Delivery
    }

    public enum ProcessingStatus
    {
        New,
        Processing,
        Completed
    }
}
