using Application.Common.Settings;
using Infrastructure.BackgroundServices;
using Quartz;
using Application.Common.Markers;
using FluentValidation;
using Infrastructure;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Text.Json.Serialization;
using WebApi.Infrastructure;
using WebApi.Middlewares;

namespace WebApi.Configuration
{
    public static partial class HostConfiguration
    {
        private static WebApplicationBuilder AddDevTools(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();

            return builder;
        }

        private static WebApplicationBuilder AddExposers(this WebApplicationBuilder builder)
        {
            builder.Services
                .AddControllers(options =>
                {
                    options.Filters.AddService<FluentValidationFilter>(order: -3000);
                })
                .AddJsonOptions(options => 
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            builder.Services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>(includeInternalTypes: true);
            builder.Services.AddScoped<FluentValidationFilter>();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddRouting(options => options.LowercaseUrls = true);

            return builder;
        }

        private static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.OperationFilter<CustomHeadersOperationFilter>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = @"JWT Authorization header using the Bearer scheme. <br />
                                    Enter 'Bearer' [space] and then your token in the text input below. <br /><br />
                                    Example: 'Bearer 12345abcdef'"
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            return builder;
        }

        private static WebApplicationBuilder AddLogger(this WebApplicationBuilder builder)
        {
            var logDirectory = "logs";
            var logFileName = "log-.log";
            var fullPath = Path.Combine(logDirectory, logFileName);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    path: fullPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 40,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }

        private static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddInfrastructure(builder.Configuration);
            
            return builder;
        }

        private static WebApplicationBuilder AddQuartz(this WebApplicationBuilder builder)
        {
            builder.Services.AddQuartz(q =>
            {
                // Backup Job — har kuni 04:05 da
                var backupJobKey = new JobKey("BackupJob");
                q.AddJob<BackupJob>(opts => opts.WithIdentity(backupJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(backupJobKey)
                    .WithIdentity("BackupJobTrigger")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(04, 05)));

                // AdjustBalance Job — har kuni 02:30 da
                var adjustJobKey = new JobKey("AdjustBalanceJob");
                q.AddJob<AdjustBalanceJob>(opts => opts.WithIdentity(adjustJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(adjustJobKey)
                    .WithIdentity("AdjustBalanceJobTrigger")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(2, 30)));
            });

            builder.Services.Configure<BackupJobSettings>(
                builder.Configuration.GetSection("BackupJob"));

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            return builder;
        }

        private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Default' is not configured.");
            }

            // Npgsql ga UTC DateTime ni "timestamp without time zone" ga yozishga ruxsat beradi
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });

                // Helpful during development: show EF Core SQL and detailed errors.
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(System.Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Debug);
            });

            return builder;
        }

        private static WebApplicationBuilder AddJwtToken(this WebApplicationBuilder builder)
        {
            var jwtSection = builder.Configuration.GetSection("Jwt");

            builder.Services.Configure<JwtOptions>(jwtSection);

            var jwt = jwtSection.Get<JwtOptions>()!;

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = jwt.ValidateIssuer,
                        ValidateAudience = jwt.ValidateAudience,
                        ValidateLifetime = jwt.ValidateLifetime,
                        ValidateIssuerSigningKey = jwt.ValidateIssuerSigningKey,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.Key)
                        ),

                        ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds)
                    };
                });

            builder.Services.AddAuthorization();

            return builder;
        }

        private static WebApplication UseExposers(this WebApplication app)
        {
            app.MapControllers();

            return app;
        }

        private static WebApplication UseDevTools(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                c.DisplayRequestDuration();
            });

            return app;
        }

        private static WebApplication UseMiddlewares(this WebApplication app)
        {
            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseSerilogRequestLogging();

            app.UseCors(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            app.UseAuthentication();
            app.UseMiddleware<OrganizationScopeMiddleware>();
            app.UseAuthorization();

            return app;
        }
    }
}
