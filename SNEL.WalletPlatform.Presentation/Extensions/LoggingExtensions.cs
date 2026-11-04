using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using SNEL.WalletPlatform.Presentation.Middleware;
using System.Globalization;
using System.Text;

namespace SNEL.WalletPlatform.Presentation.Extensions
{
    public static class LoggingExtension
    {
        public static void RegisterLogging(this IServiceCollection services, WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();            
            ConfigureLogging(builder);
            builder.Host.UseSerilog();
            // Add HTTP logging middleware
            services.AddHttpContextAccessor();
            services.AddScoped<RequestBodyLogger>();
        }

        static void ConfigureLogging(WebApplicationBuilder builder)
        {
            var environment = builder.Environment.EnvironmentName;
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithEnvironmentName()                
                .Enrich.WithCallerInfo(includeFileInfo: true,assemblyPrefix:"SNEL.WalletPlatform")
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",LogEventLevel.Warning)
                .MinimumLevel.Override("Elastic.Apm",LogEventLevel.Warning)
                .WriteTo.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}]:[{SourceContext}]:[{Namespace}] [{Method}]:[{LineNumber}]:{Message:lj}{NewLine}{Exception}");
            if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging() || builder.Environment.IsProduction())
            {
                Log.Information($"Environment: {builder.Environment.EnvironmentName}, will apply elastic sink");
                loggerConfiguration =
                    loggerConfiguration.WriteTo.Elasticsearch(ConfigureElasticSink(builder.Configuration, environment));
            }
            else
            {
                Log.Information($"Environment: {builder.Environment.EnvironmentName}, will not apply elastic sink");
            }
            loggerConfiguration = loggerConfiguration.ReadFrom.Configuration(builder.Configuration);
            Log.Logger = loggerConfiguration.CreateLogger();
        }

        static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            var elasticUri = configuration.GetValue<string>("ElasticConfiguration:Uri");
            var username = configuration.GetValue<string>("ElasticConfiguration:Username");
            var password = configuration.GetValue<string>("ElasticConfiguration:Password");

            return new ElasticsearchSinkOptions(new Uri(elasticUri!))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{"SNEL-WALLET"}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                ModifyConnectionSettings = x => x.BasicAuthentication(username, password)

            };
        }
    }

    // Middleware for logging requests and responses
    

    // Service for structured body logging with correlation
    public class RequestBodyLogger
    {
        private readonly ILogger<RequestBodyLogger> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestBodyLogger(ILogger<RequestBodyLogger> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LogBody<T>(T body, string action)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            _logger.LogDebug("{Action} Body for {TraceId}: {@Body}",
                action,
                context.TraceIdentifier,
                body);
        }
    }
    
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Items["RequestStartTime"] = DateTime.UtcNow;
                await next();
            });

            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
