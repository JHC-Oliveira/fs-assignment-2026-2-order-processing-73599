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
				Password = password
			};
			_connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
		}

		public IChannel CreateChannelAsync() =>
			_connection.CreateChannelAsync().GetAwaiter().GetResult();

		public void Dispose() => _connection?.Dispose();
	}
}
