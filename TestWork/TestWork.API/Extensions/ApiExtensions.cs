using Microsoft.OpenApi.Models;

namespace TestWork.API.Extensions;

public static class ApiExtensions
{
    public static void AddApiDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TestWork Price Tracker API",
                Version = "v1",
                Description = "API для отслеживания изменения цен квартир на сайте prinzip.su"
            });
        });
    }
}
