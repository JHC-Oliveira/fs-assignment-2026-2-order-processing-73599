namespace MessageContracts;

public record OrderSubmitted(Guid OrderId, Guid CustomerId, List<OrderItemDto> Items, decimal TotalAmount);
public record InventoryCheckRequested(Guid OrderId, List<OrderItemDto> Items);
public record InventoryCheckCompleted(Guid OrderId, bool IsSuccess, string? Reason);
public record PaymentProcessingRequested(Guid OrderId, decimal Amount, string CardNumber);
public record PaymentProcessed(Guid OrderId, bool IsSuccess, string? TransactionId, string? Reason);
public record ShippingRequested(Guid OrderId, string CustomerName, string Address);
public record ShippingCreated(Guid OrderId, string TrackingNumber, DateTime EstimatedDelivery);
public record OrderCompleted(Guid OrderId);
public record OrderFailed(Guid OrderId, string Reason);

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal Price);