namespace OrderManagementApi.Models
{
    public enum InventoryStatus { Reserved, Released, Failed }

    public class InventoryRecord
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public InventoryStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
