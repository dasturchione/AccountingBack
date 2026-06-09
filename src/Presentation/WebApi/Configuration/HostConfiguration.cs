namespace WebApi.Configuration
{
    public static partial class HostConfiguration
    {
        public static ValueTask<WebApplicationBuilder> ConfigureAsync(this WebApplicationBuilder builder)
        {
            builder
                .AddLogger()
                .AddDevTools()
                .AddPersistence()
                .AddInfrastructure()
                .AddApplication()
                .AddJwtToken()
                .AddQuartz()
                .AddExposers()
                .AddSwagger();

            return new ValueTask<WebApplicationBuilder>(builder);
        }

        public static ValueTask<WebApplication> ConfigureAsync(this WebApplication app)
        {
            app
                .UseDevTools()
                .UseMiddlewares()
                .UseExposers();

            return new ValueTask<WebApplication>(app);
        }
    }
}
