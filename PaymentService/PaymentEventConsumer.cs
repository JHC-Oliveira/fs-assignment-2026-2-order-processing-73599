using MessageContracts;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PaymentService
{
	public class PaymentEventConsumer : BackgroundService
	{
		private readonly IChannel _channel;
		private readonly IBusPublisher _publisher;
		private readonly ILogger<PaymentEventConsumer> _logger;
		private readonly Random _random = new();

		public PaymentEventConsumer(
			IRabbitMQConnection connection,
			IBusPublisher publisher,
			ILogger<PaymentEventConsumer> logger)
		{
			_channel = connection.CreateChannelAsync();
			_publisher = publisher;
			_logger = logger;

			_channel.ExchangeDeclareAsync("order_exchange", ExchangeType.Topic, durable: true)
				.GetAwaiter().GetResult();
			_channel.QueueDeclareAsync("payment_queue", durable: true, exclusive: false, autoDelete: false)
				.GetAwaiter().GetResult();
			_channel.QueueBindAsync("payment_queue", "order_exchange", nameof(InventoryCheckCompleted))
				.GetAwaiter().GetResult();
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var consumer = new AsyncEventingBasicConsumer(_channel);

			consumer.ReceivedAsync += async (model, ea) =>
			{
				var message = Encoding.UTF8.GetString(ea.Body.ToArray());
				var inventoryResult = JsonSerializer.Deserialize<InventoryCheckCompleted>(message)!;

				// Only process payment if inventory succeeded
				if (inventoryResult.IsSuccess)
				{
					// Simulate payment: 90% success, 10% random failure
					bool isSuccess = _random.Next(1, 11) != 1;
					string? transactionId = isSuccess ? Guid.NewGuid().ToString() : null;
					string? reason = isSuccess ? null : "Payment declined by bank";

					if (isSuccess)
						_logger.LogInformation("Payment successful for Order {OrderId}", inventoryResult.OrderId);
					else
						_logger.LogWarning("Payment failed for Order {OrderId}: {Reason}", inventoryResult.OrderId, reason);

					await _publisher.Publish(new PaymentProcessed(
						inventoryResult.OrderId, isSuccess, transactionId, reason));
				}
				else
				{
					_logger.LogInformation("Skipping payment for Order {OrderId} — inventory failed", inventoryResult.OrderId);
				}

				await _channel.BasicAckAsync(ea.DeliveryTag, false);
			};

			_channel.BasicConsumeAsync("payment_queue", autoAck: false, consumer: consumer)
				.GetAwaiter().GetResult();

			return Task.CompletedTask;
		}

		public override void Dispose()
		{
			_channel.CloseAsync().GetAwaiter().GetResult();
			_channel.Dispose();
			base.Dispose();
		}
	}
}
