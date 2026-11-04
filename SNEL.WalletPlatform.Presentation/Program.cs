using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Localization;
using Microsoft.OpenApi.Models;
using Serilog;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;
using SNEL.WalletPlatform.Application.DependencyInjection;
using SNEL.WalletPlatform.Infrastructure.DependencyInjection;
using SNEL.WalletPlatform.Infrastructure.Reporting;
using SNEL.WalletPlatform.Infrastructure.Reporting.PDF;
using SNEL.WalletPlatform.Infrastructure.Reporting.Templating;
using SNEL.WalletPlatform.Presentation.Extensions;
using SNEL.WalletPlatform.Presentation.Middleware;
using SNEL.WalletPlatform.Presentation.Swagger;

var builder = WebApplication.CreateBuilder(args);
await builder.AddHashICorp(); 

builder.Services.RegisterLogging(builder);
if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging() || builder.Environment.IsProduction())
{
    builder.Services.AddAllElasticApm();
}

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SNEL Wallet Platform API",
        Version = "v1"
    });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste Keycloak access token as: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [bearerScheme] = Array.Empty<string>()
    });

    options.OperationFilter<AcceptLanguageHeaderOperationFilter>();
});
builder.Services.AddSnelAuthentication(builder.Configuration);
builder.Services.AddTemplateRendering(builder.Configuration);
builder.Services.AddHtmlToPdf(builder.Configuration);
builder.Services.AddCors();

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

builder.Services.RegisterQuartzServices(builder.Configuration);

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("fr")
};

var requestLocalizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

await app.Services.InitializeInfrastructureAsync();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SNEL Wallet Platform API v1");
});

app.UseRequestLocalization(requestLocalizationOptions);
app.UseGlobalExceptionHandling();

app.UseCors(opt =>
{
    opt.AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("Content-Disposition");

    var allowedHosts = builder.Configuration.GetValue<string>("AllowedHosts") ?? "*";
    var origins = allowedHosts.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (origins.Length == 0 || origins.Any(o => o == "*" || o.Equals("all", StringComparison.OrdinalIgnoreCase)))
    {
        opt.AllowAnyOrigin();
    }
    else
    {
        opt.WithOrigins(origins);
    }
});
app.UseRequestResponseLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

Log.CloseAndFlush();
