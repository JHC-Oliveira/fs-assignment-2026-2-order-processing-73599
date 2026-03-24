using RabbitMQ.Client;

namespace RabbitMQ
{
	public class RabbitMQConnection : IRabbitMQConnection
	{
		private readonly IConnection _connection;

		public RabbitMQConnection(string hostname, int port, string username, string password)
		{
			var factory = new ConnectionFactory
			{
				HostName = hostname,
				Port = port,
				UserName = username,
				Password = password,
				DispatchConsumersAsync = true
			};
			_connection = factory.CreateConnection();
		}

		public IModel CreateModel() => _connection.CreateModel();

		public void Dispose() => _connection?.Dispose();
	}
}
