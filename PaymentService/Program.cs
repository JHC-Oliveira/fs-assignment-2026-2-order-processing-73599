using PaymentService;
using RabbitMQ;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateLogger();
builder.Services.AddSerilog();

builder.Services.AddSingleton<IRabbitMQConnection>(sp =>
{
	var config = sp.GetRequiredService<IConfiguration>();
	return new RabbitMQConnection(
		config["RabbitMQ:HostName"]!,
		int.Parse(config["RabbitMQ:Port"]!),
		config["RabbitMQ:UserName"]!,
		config["RabbitMQ:Password"]!);
});
builder.Services.AddSingleton<IBusPublisher, RabbitMQBus>();
builder.Services.AddHostedService<PaymentEventConsumer>();

var host = builder.Build();
host.Run();
