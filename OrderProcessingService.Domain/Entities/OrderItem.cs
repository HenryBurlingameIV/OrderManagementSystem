namespace OrderProcessingService.Domain.Entities
{
    public class OrderItem
    {
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