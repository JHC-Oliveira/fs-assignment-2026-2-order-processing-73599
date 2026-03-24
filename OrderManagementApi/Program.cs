using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.RabbitMQ;
using RabbitMQ;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateLogger();
builder.Host.UseSerilog();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core
builder.Services.AddDbContext<OrderDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
	cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// RabbitMQ
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

// Background consumer
builder.Services.AddHostedService<OrderEventConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Auto-run migrations on startup
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
	db.Database.Migrate();
}

app.Run();
