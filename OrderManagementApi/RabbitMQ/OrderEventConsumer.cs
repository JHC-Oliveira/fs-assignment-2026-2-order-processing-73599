using MediatR;
using MessageContracts;
using OrderManagementApi.Features.Orders;
using OrderManagementApi.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ;
using System.Text;
using System.Text.Json;

namespace OrderManagementApi.RabbitMQ
{
	public class OrderEventConsumer : BackgroundService
	{
		private readonly IChannel _channel;
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<OrderEventConsumer> _logger;

		public OrderEventConsumer(
			IRabbitMQConnection rabbitMQConnection,
			IServiceProvider serviceProvider,
			ILogger<OrderEventConsumer> logger)
		{
			_channel = rabbitMQConnection.CreateChannelAsync();
			_serviceProvider = serviceProvider;
			_logger = logger;

			_channel.ExchangeDeclareAsync("order_exchange", ExchangeType.Topic, durable: true)
				.GetAwaiter().GetResult();
			_channel.QueueDeclareAsync("order_management_queue", durable: true, exclusive: false, autoDelete: false)
				.GetAwaiter().GetResult();
			_channel.QueueBindAsync("order_management_queue", "order_exchange", nameof(InventoryCheckCompleted))
				.GetAwaiter().GetResult();
			_channel.QueueBindAsync("order_management_queue", "order_exchange", nameof(PaymentProcessed))
				.GetAwaiter().GetResult();
			_channel.QueueBindAsync("order_management_queue", "order_exchange", nameof(ShippingCreated))
				.GetAwaiter().GetResult();
			_channel.QueueBindAsync("order_management_queue", "order_exchange", nameof(OrderFailed))
				.GetAwaiter().GetResult();
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var consumer = new AsyncEventingBasicConsumer(_channel);

			consumer.ReceivedAsync += async (model, ea) =>
			{
				var eventName = ea.RoutingKey;
				var message = Encoding.UTF8.GetString(ea.Body.ToArray());

				using var scope = _serviceProvider.CreateScope();
				var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

				try
				{
					switch (eventName)
					{
						case nameof(InventoryCheckCompleted):
							var inv = JsonSerializer.Deserialize<InventoryCheckCompleted>(message)!;
							await mediator.Send(new UpdateOrderStatusCommand(
								inv.OrderId,
								inv.IsSuccess ? OrderStatus.PaymentPending : OrderStatus.InventoryFailed,
								inv.Reason));
							break;

						case nameof(PaymentProcessed):
							var pay = JsonSerializer.Deserialize<PaymentProcessed>(message)!;
							await mediator.Send(new UpdateOrderStatusCommand(
								pay.OrderId,
								pay.IsSuccess ? OrderStatus.ShippingPending : OrderStatus.PaymentFailed,
								pay.Reason));
							break;

						case nameof(ShippingCreated):
							var ship = JsonSerializer.Deserialize<ShippingCreated>(message)!;
							await mediator.Send(new UpdateOrderStatusCommand(
								ship.OrderId,
								OrderStatus.Completed));
							break;

						case nameof(OrderFailed):
							var fail = JsonSerializer.Deserialize<OrderFailed>(message)!;
							await mediator.Send(new UpdateOrderStatusCommand(
								fail.OrderId,
								OrderStatus.Failed,
								fail.Reason));
							break;
					}

					await _channel.BasicAckAsync(ea.DeliveryTag, false);
					_logger.LogInformation("Processed event {EventName}", eventName);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error processing event {EventName}", eventName);
					await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
				}
			};

			_channel.BasicConsumeAsync(
				queue: "order_management_queue",
				autoAck: false,
				consumer: consumer)
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
