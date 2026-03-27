namespace OrderManagementApi.Models
{
    public enum PaymentStatus { Pending, Completed, Failed, Refunded }

    public class PaymentRecord
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
