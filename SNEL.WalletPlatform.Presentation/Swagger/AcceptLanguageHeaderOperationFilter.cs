using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SNEL.WalletPlatform.Presentation.Swagger;

public sealed class AcceptLanguageHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        var hasHeader = operation.Parameters.Any(p =>
            p.In == ParameterLocation.Header &&
            string.Equals(p.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase));

        if (hasHeader)
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Description = "Preferred language (for example: en, fr, ar or en-US,en;q=0.9).",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("en")
            }
        });
    }
}
