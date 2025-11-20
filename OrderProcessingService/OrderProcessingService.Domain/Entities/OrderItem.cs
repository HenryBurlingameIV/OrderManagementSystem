namespace OrderProcessingService.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid ProcessingOrderId { get; set; }
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public ItemAssemblyStatus Status { get; set; }
    }

    public enum ItemAssemblyStatus
    {
        Pending,
        Ready
    }
}