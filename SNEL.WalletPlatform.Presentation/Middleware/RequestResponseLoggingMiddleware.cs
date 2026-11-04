using System.Text;

namespace SNEL.WalletPlatform.Presentation.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var skipPaths = new[] { "/health", "/metrics", "/swagger" };
            if (skipPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await _next(context);
                return;
            }

            await LogRequest(context);

            // Capture response body
            var originalBodyStream = context.Response.Body;
            using var responseBodyMemoryStream = new MemoryStream();
            context.Response.Body = responseBodyMemoryStream;

            try
            {
                await _next(context);

                // Log response
                await LogResponse(context, responseBodyMemoryStream);

                // Copy back to original stream
                await responseBodyMemoryStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                var requestBody = await ReadBodyFromRequest(context.Request);

                var requestLog = new
                {
                    TraceId = context.TraceIdentifier,
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    Body = requestBody,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString()
                };

                _logger.LogInformation("HTTP Request: {@RequestLog}", requestLog);
                context.Request.Body.Position = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging request body");
            }
        }

        private async Task LogResponse(HttpContext context, MemoryStream responseBodyMemoryStream)
        {
            try
            {
                responseBodyMemoryStream.Position = 0;
                var responseBody = await new StreamReader(responseBodyMemoryStream).ReadToEndAsync();
                responseBodyMemoryStream.Position = 0;

                var responseLog = new
                {
                    TraceId = context.TraceIdentifier,
                    StatusCode = context.Response.StatusCode,
                    Body = TruncateIfNeeded(responseBody, 5000), // Limit body size
                    Timestamp = DateTime.UtcNow,
                    Duration = GetRequestDuration(context)
                };

                var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error :
                              context.Response.StatusCode >= 400 ? LogLevel.Warning :
                              LogLevel.Information;

                _logger.Log(logLevel, "HTTP Response: {@ResponseLog}", responseLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging response body");
            }
        }

        private async Task<string> ReadBodyFromRequest(HttpRequest request)
        {
            if (!request.Body.CanRead) return string.Empty;

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return TruncateIfNeeded(body, 5000);
        }

        private string TruncateIfNeeded(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "... [TRUNCATED]";
        }

        private long GetRequestDuration(HttpContext context)
        {
            if (context.Items.TryGetValue("RequestStartTime", out var startTime))
            {
                return (long)(DateTime.UtcNow - (DateTime)startTime).TotalMilliseconds;
            }
            return 0;
        }
    }
}
