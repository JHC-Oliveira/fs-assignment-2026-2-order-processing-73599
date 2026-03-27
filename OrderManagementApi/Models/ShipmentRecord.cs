namespace OrderManagementApi.Models
{
    public enum ShipmentStatus { Pending, Shipped, Delivered, Failed }

    public class ShipmentRecord
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public ShipmentStatus Status { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
