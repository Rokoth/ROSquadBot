using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ROSquadBot
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        private const string AuthorizationName = "Authorization";
        private const string DefaultDescription = "access token";
        private const string StringType = "string";
        private const string BearerDefaultApiString = "Bearer ";
        private const ParameterLocation DefaultParameterLocation = ParameterLocation.Header;

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            CheckOperation(operation);
            operation.Parameters.Add(CreateOpenApiParameter());
        }

        private static OpenApiParameter CreateOpenApiParameter() =>
            new()
            {
                Name = AuthorizationName,
                In = DefaultParameterLocation,
                Description = DefaultDescription,
                Required = true,
                Schema = CreateOpenApiSchema()
            };

        private static OpenApiSchema CreateOpenApiSchema() =>
            new()
            {
                Type = StringType,
                Default = new OpenApiString(BearerDefaultApiString)
            };

        private static void CheckOperation(OpenApiOperation operation)
        {
            operation.Parameters ??= [];
        }
    }
}
