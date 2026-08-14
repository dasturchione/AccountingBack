using Application.Common.Markers;
using Application.Common.Settings;
using FluentValidation;
using Infrastructure;
using Infrastructure.BackgroundServices;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using Quartz;
using Serilog;
using Serilog.Events;
using SharedKernel.Time;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
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
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            builder.Services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>(includeInternalTypes: true);
            builder.Services.AddScoped<FluentValidationFilter>();

            AddCorsPolicies(builder);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddRouting(options => options.LowercaseUrls = true);
            builder.Services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap["short"] = typeof(IntRouteConstraint);
            });

            return builder;
        }

        private static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.OperationFilter<CustomHeadersOperationFilter>();
                c.OperationFilter<EdoPublicContractOperationFilter>();
                c.OperationFilter<EdoSwaggerContractOperationFilter>();

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
            builder.AddDataProtectionKeys(builder.Environment);

            builder.Services.AddInfrastructure(builder.Configuration);

            return builder;
        }

        private static WebApplicationBuilder AddDataProtectionKeys(this WebApplicationBuilder builder, IWebHostEnvironment environment)
        {
            var isProduction = environment.IsProduction();
            if (isProduction)
            {
                var productionKeyPath = RequiredProductionSetting(builder.Configuration, "DataProtection:KeysPath");

                if (!Path.IsPathRooted(productionKeyPath))
                    throw new InvalidOperationException("DataProtection:KeysPath must be an absolute production path.");

                try
                {
                    var directoryInfo = EnsureDataProtectionKeyDirectory(productionKeyPath);

                    builder.Services.AddDataProtection()
                        .PersistKeysToFileSystem(directoryInfo)
                        .SetApplicationName("accounting-back");
                }
                catch (Exception)
                {
                    // Production must fail closed. Do not log the configured path or exception
                    // text because they can disclose deployment details.
                    throw new InvalidOperationException("Production DataProtection key-ring initialization failed.");
                }

                return builder;
            }

            var candidatePath = Path.Combine(environment.ContentRootPath, ".aspnet-dataprotection-keys");

            var configuredPath = builder.Configuration["DataProtection:KeysPath"];
            var keyDirectory = !string.IsNullOrWhiteSpace(configuredPath)
                ? configuredPath
                : candidatePath;

            try
            {
                if (string.IsNullOrWhiteSpace(keyDirectory))
                {
                    throw new InvalidOperationException("DataProtection keys path is not configured.");
                }

                if (!Path.IsPathRooted(keyDirectory))
                {
                    keyDirectory = Path.GetFullPath(Path.Combine(environment.ContentRootPath, keyDirectory));
                }

                var directoryInfo = EnsureDataProtectionKeyDirectory(keyDirectory);
                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(directoryInfo)
                    .SetApplicationName("accounting-back");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is PathTooLongException)
            {
                Log.Warning(ex, "Could not initialize file system DataProtection key persistence. Key persistence will fall back to system defaults.");
            }

            return builder;
        }

        private static DirectoryInfo EnsureDataProtectionKeyDirectory(string path)
        {
            const UnixFileMode ownerOnly =
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

            if (OperatingSystem.IsLinux())
            {
                Directory.CreateDirectory(path, ownerOnly);
                File.SetUnixFileMode(path, ownerOnly);
            }
            else
            {
                Directory.CreateDirectory(path);
            }

            return new DirectoryInfo(path);
        }

        private static void AddCorsPolicies(WebApplicationBuilder builder)
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            ValidateCorsOrigins(allowedOrigins, builder.Environment.EnvironmentName);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ApiCors", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                        .WithHeaders("Accept", "Authorization", "Content-Type", "X-OrganizationId", "X-Language");
                });
            });
        }

        private static void ValidateCorsOrigins(IReadOnlyCollection<string> origins, string environmentName)
        {
            if (environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase) && origins.Count == 0)
                throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one production origin.");

            var index = 0;
            foreach (var origin in origins)
            {
                if (string.IsNullOrWhiteSpace(origin)
                    || origin.Contains('*')
                    || !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                    || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    || string.IsNullOrWhiteSpace(uri.Host)
                    || uri.AbsolutePath != "/"
                    || !string.IsNullOrEmpty(uri.Query)
                    || !string.IsNullOrEmpty(uri.Fragment)
                    || !string.IsNullOrEmpty(uri.UserInfo))
                {
                    throw new InvalidOperationException($"Cors:AllowedOrigins contains an invalid origin at index {index}.");
                }

                index++;
            }
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

                var notificationEmailJobKey = new JobKey("NotificationEmailDispatchJob");
                q.AddJob<NotificationEmailDispatchJob>(opts => opts.WithIdentity(notificationEmailJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(notificationEmailJobKey)
                    .WithIdentity("NotificationEmailDispatchJobTrigger")
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromMinutes(15))
                        .RepeatForever()));

                var contractExpiryJobKey = new JobKey("ContractExpiryNotificationJob");
                q.AddJob<ContractExpiryNotificationJob>(opts => opts.WithIdentity(contractExpiryJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(contractExpiryJobKey)
                    .WithIdentity("ContractExpiryNotificationJobTrigger")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(9, 0)
                        .InTimeZone(TashkentTime.Zone)));

                var edoImportPreflightJobKey = new JobKey(EdoImportPreflightJob.JobName);
                q.AddJob<EdoImportPreflightJob>(opts => opts
                    .WithIdentity(edoImportPreflightJobKey)
                    .StoreDurably());
                q.AddTrigger(opts => opts
                    .ForJob(edoImportPreflightJobKey)
                    .WithIdentity("EdoImportPreflightRecoveryTrigger")
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromMinutes(1))
                        .RepeatForever()));

                var edoBulkDraftImportJobKey = new JobKey(EdoBulkDraftImportJob.JobName);
                q.AddJob<EdoBulkDraftImportJob>(opts => opts
                    .WithIdentity(edoBulkDraftImportJobKey)
                    .StoreDurably());
                q.AddTrigger(opts => opts
                    .ForJob(edoBulkDraftImportJobKey)
                    .WithIdentity("EdoBulkDraftImportRecoveryTrigger")
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromMinutes(1))
                        .RepeatForever()));
            });

            builder.Services.Configure<BackupJobSettings>(
                builder.Configuration.GetSection("BackupJob"));

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            return builder;
        }

        private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
        {
            var rawConnectionString = builder.Configuration.GetConnectionString("Default");
            ValidateSecuritySettings(builder.Configuration, builder.Environment.EnvironmentName);

            if (string.IsNullOrEmpty(rawConnectionString))
            {
                throw new InvalidOperationException("Connection string 'Default' is not configured.");
            }

            var connectionString = NormalizeConnectionString(rawConnectionString);
            LogConnectionStringSource(builder.Configuration, connectionString);

            // Npgsql ga UTC DateTime ni "timestamp without time zone" ga yozishga ruxsat beradi
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                    options.LogTo(System.Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Debug);
                }
            });

            return builder;
        }

        private static string NormalizeConnectionString(string connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                return builder.ConnectionString;
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:Default is malformed. If the password contains ';' or '=', wrap the password value in double quotes.",
                    ex);
            }
        }

        private static void LogConnectionStringSource(ConfigurationManager configuration, string connectionString)
        {
            var providerName = "unknown";

            if (configuration is IConfigurationRoot root)
            {
                foreach (var provider in root.Providers.Reverse())
                {
                    if (provider.TryGet("ConnectionStrings:Default", out _))
                    {
                        providerName = provider.ToString() ?? provider.GetType().Name;
                        break;
                    }
                }
            }

            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                var password = builder.Password ?? string.Empty;

                Log.Information(
                    "Resolved ConnectionStrings:Default from {Provider}. Host={Host}; Port={Port}; Database={Database}; Username={Username}; PasswordLength={PasswordLength}; ContainsSemicolon={ContainsSemicolon}; ContainsEquals={ContainsEquals}",
                    providerName,
                    builder.Host,
                    builder.Port,
                    builder.Database,
                    builder.Username,
                    password.Length,
                    password.Contains(';'),
                    password.Contains('='));
            }
            catch (ArgumentException)
            {
                Log.Warning("Resolved ConnectionStrings:Default from {Provider}, but Npgsql could not parse it.", providerName);
            }
        }

        private static void ValidateSecuritySettings(ConfigurationManager configuration, string env)
        {
            var connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:Default is not configured with a real secret value.");
            }

            var jwtSection = configuration.GetSection("Jwt");
            ValidateJwtOption(jwtSection, env);

            var backupPassword = configuration["BackupJob:Database:Password"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(backupPassword))
                throw new InvalidOperationException("BackupJob:Database:Password is not configured with a real secret value.");

            var aslBelgiServerBaseUrl = configuration["AslBelgi:ServerBaseUrl"];
            var aslBelgiAuthPath = configuration["AslBelgi:AuthenticatePath"];
            var aslBelgiRefreshPath = configuration["AslBelgi:RefreshPath"];
            var hasLegacyAslBelgiConfiguration = !string.IsNullOrWhiteSpace(aslBelgiServerBaseUrl)
                || !string.IsNullOrWhiteSpace(aslBelgiAuthPath)
                || !string.IsNullOrWhiteSpace(aslBelgiRefreshPath);
            var hasAslBelgiV1Configuration = !string.IsNullOrWhiteSpace(configuration["AslBelgi:BaseUrl"]);

            if (hasLegacyAslBelgiConfiguration && env.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                if (IsPlaceholderValue(aslBelgiServerBaseUrl))
                    throw new InvalidOperationException("AslBelgi:ServerBaseUrl is not configured with a real value.");

                if (string.IsNullOrWhiteSpace(aslBelgiAuthPath))
                    throw new InvalidOperationException("AslBelgi:AuthenticatePath is required in production.");

                if (string.IsNullOrWhiteSpace(aslBelgiRefreshPath))
                    throw new InvalidOperationException("AslBelgi:RefreshPath is required in production.");
            }
            else if (hasLegacyAslBelgiConfiguration)
            {
                if (IsPlaceholderValue(aslBelgiServerBaseUrl) || IsPlaceholderValue(aslBelgiAuthPath) || IsPlaceholderValue(aslBelgiRefreshPath))
                    Log.Warning("AslBelgi development config still uses placeholder values. Set real values or override via environment variables before production.");
            }

            var fakturaClientSecret = configuration["FakturaAuthSettings:ClientSecret"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(fakturaClientSecret))
                throw new InvalidOperationException("FakturaAuthSettings:ClientSecret is not configured with a real secret value.");

            var fakturaPassword = configuration["FakturaAuthSettings:Password"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(fakturaPassword))
                throw new InvalidOperationException("FakturaAuthSettings:Password is not configured with a real secret value.");

            var fakturaUsername = configuration["FakturaAuthSettings:Username"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(fakturaUsername))
                throw new InvalidOperationException("FakturaAuthSettings:Username is not configured with a real value.");

            var emailUsername = configuration["Email:Username"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(emailUsername))
                throw new InvalidOperationException("Email:Username is not configured with a real value.");

            var emailPassword = configuration["Email:Password"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(emailPassword))
                throw new InvalidOperationException("Email:Password is not configured with a real secret value.");

            var didoxPartnerToken = configuration["Didox:PartnerToken"];
            var useDidoxPartnerlessLegacyApi = configuration.GetValue<bool>("Didox:UsePartnerlessLegacyApi");
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase)
                && !useDidoxPartnerlessLegacyApi
                && IsPlaceholderValue(didoxPartnerToken))
                throw new InvalidOperationException("Didox:PartnerToken is not configured with a real secret value.");

            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var key in ProductionRequiredSettingKeys)
                    _ = RequiredProductionSetting(configuration, key);

                foreach (var key in ForbiddenGlobalProviderSecretKeys)
                {
                    if (hasAslBelgiV1Configuration && string.Equals(key, "AslBelgi:ApiKey", StringComparison.Ordinal))
                        continue;

                    if (!IsPlaceholderValue(configuration[key]))
                        throw new InvalidOperationException($"{key} must not be configured globally in production; use the scoped credential store.");
                }
            }
        }

        private static readonly string[] ProductionRequiredSettingKeys =
        [
            "TelegramFileStorage:BotToken",
            "ConnectionStrings:Default",
            "Jwt:Key",
            "BackupJob:Database:Password",
            "FakturaAuthSettings:ClientSecret",
            "FakturaAuthSettings:AuthorizationClientId",
            "FakturaAuthSettings:Username",
            "FakturaAuthSettings:ClientId",
            "FakturaAuthSettings:Password",
            "Email:Username",
            "Email:Password",
            "AslBelgi:ApiKey",
            "Edocs:PartnerId",
            "DataProtection:KeysPath"
        ];

        private static readonly string[] ForbiddenGlobalProviderSecretKeys =
        [
            "AslBelgi:Login",
            "AslBelgi:Password",
            "AslBelgi:ApiKey"
        ];

        private static void ValidateJwtOption(IConfigurationSection jwtSection, string env)
        {
            var key = jwtSection["Key"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(key))
                throw new InvalidOperationException("Jwt:Key is not configured with a real secret value.");

            if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
                throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HS256 signing.");

            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("Jwt issuer and audience must be configured.");
        }

        private static bool IsPlaceholderValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            return value.Contains("SET_VIA_ENVIRONMENT", StringComparison.OrdinalIgnoreCase)
                || value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
                || value.Contains("REPLACE_ME", StringComparison.OrdinalIgnoreCase)
                || value.Contains("YOUR_", StringComparison.OrdinalIgnoreCase)
                || value.Contains("YOUR-", StringComparison.OrdinalIgnoreCase)
                || value.Contains("REAL_TOKEN", StringComparison.OrdinalIgnoreCase)
                || value.Contains("EXAMPLE", StringComparison.OrdinalIgnoreCase);
        }

        private static string RequiredProductionSetting(ConfigurationManager configuration, string key)
        {
            var value = configuration[key];
            if (IsPlaceholderValue(value))
                throw new InvalidOperationException($"{key} is not configured with a real value.");

            return value!.Trim();
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

        private static WebApplicationBuilder AddRateLimiting(this WebApplicationBuilder builder)
        {
            var permitLimit = builder.Configuration.GetValue("RateLimit:PermitLimit", 1200);
            var queueLimit = builder.Configuration.GetValue("RateLimit:QueueLimit", 0);
            var windowSeconds = builder.Configuration.GetValue("RateLimit:WindowSeconds", 60);

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        QueueLimit = queueLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        AutoReplenishment = true
                    });
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

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
            app.UseHsts();

            app.UseMiddleware<SecurityHeadersMiddleware>();

            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseSerilogRequestLogging();

            app.UseCors("ApiCors");

            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseMiddleware<OrganizationScopeMiddleware>();
            app.UseAuthorization();

            return app;
        }
    }
}
