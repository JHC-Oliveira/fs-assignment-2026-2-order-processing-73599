using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

namespace RabbitMQ
{
	public interface IBusPublisher
	{
		Task Publish<T>(T message, string? routingKey = null) where T : class;
	}

	public class RabbitMQBus : IBusPublisher
	{
		private readonly IRabbitMQConnection _connection;
		private readonly ILogger<RabbitMQBus> _logger;

		public RabbitMQBus(IRabbitMQConnection connection, ILogger<RabbitMQBus> logger)
		{
			_connection = connection;
			_logger = logger;
		}

		public Task Publish<T>(T message, string? routingKey = null) where T : class
		{
			using var channel = _connection.CreateModel();
			channel.ExchangeDeclare(exchange: "order_exchange", type: ExchangeType.Topic, durable: true);

			var messageName = routingKey ?? typeof(T).Name;
			var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

			channel.BasicPublish(
				exchange: "order_exchange",
				routingKey: messageName,
				basicProperties: null,
				body: body);

			_logger.LogInformation("Published {MessageName} to RabbitMQ", messageName);
			return Task.CompletedTask;
		}
	}
}
