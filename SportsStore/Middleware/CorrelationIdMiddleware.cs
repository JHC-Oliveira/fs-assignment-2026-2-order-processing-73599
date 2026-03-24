using Serilog.Context;

/*
 * CorrelationIdMiddleware.cs
 *
 * Middleware that ensures every HTTP request has a correlation identifier.
 * - Reads an incoming `X-Correlation-ID` header or generates a new GUID when absent.
 * - Appends the correlation id to the response headers so clients can see it.
 * - Pushes the correlation id into Serilog's `LogContext` so all logs for the request
 *   include a `CorrelationId` property (useful for tracing requests across components).
 *
 * Usage: register with `app.UseCorrelationId()` before request-logging middleware.
 */

namespace SportsStore.Middleware
{
	public class CorrelationIdMiddleware
	{
		private readonly RequestDelegate _next;
		private const string CorrelationIdHeader = "X-Correlation-ID";

		public CorrelationIdMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			//GET OR CREATE CORRELATION ID 
			// Check if the request already has a correlation ID (from upstream service)
			var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
				?? Guid.NewGuid().ToString(); // Create new ID if none exists

			//ADD TO RESPONSE 
			// Client can read this header to know the correlation ID
			context.Response.Headers.Append(CorrelationIdHeader, correlationId);

			//ADD TO LOG CONTEXT
			// All logs within this request will automatically include CorrelationId property
			using (LogContext.PushProperty("CorrelationId", correlationId))
			{
				await _next(context); // Continue to next middleware
			}
			// LogContext is automatically cleaned up when 'using' block exits
		}
	}

	// Extension method for clean registration
	public static class CorrelationIdMiddlewareExtensions
	{
		public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<CorrelationIdMiddleware>();
		}
	}
}