using Application.Common.Settings;
using Infrastructure.BackgroundServices;
using Quartz;
using Application.Common.Markers;
using FluentValidation;
using Infrastructure;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using System.Security.Cryptography.X509Certificates;
using WebApi.Infrastructure;
using WebApi.Middlewares;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Npgsql;
using Microsoft.AspNetCore.DataProtection;

namespace WebApi.Configuration
{
    public static partial class HostConfiguration
    {
        private static WebApplicationBuilder AddDevTools(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();

            if (builder.Environment.IsDevelopment())
                TryAddUserSecrets(builder.Configuration, typeof(Program).Assembly);

            return builder;
        }

        private static void TryAddUserSecrets(ConfigurationManager configuration, Assembly assembly)
        {
            var secretsId = assembly.GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId;
            if (string.IsNullOrWhiteSpace(secretsId))
                return;

            try
            {
                var userSecretsRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Microsoft",
                    "UserSecrets",
                    secretsId);

                var secretsFile = Path.Combine(userSecretsRoot, "secrets.json");
                if (!File.Exists(secretsFile))
                    return;

                configuration.AddUserSecrets(assembly, optional: true);
            }
            catch (UnauthorizedAccessException)
            {
                // Optional secrets must never block startup in restricted environments.
            }
            catch (IOException)
            {
                // Optional secrets must never block startup in restricted environments.
            }
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
                var productionKeyPath = RequiredExternalSetting(builder.Configuration, "DataProtection:KeysPath");
                var certificatePath = RequiredExternalSetting(builder.Configuration, "DataProtection:EncryptionCertificatePath");
                var certificatePassword = RequiredExternalSetting(builder.Configuration, "DataProtection:EncryptionCertificatePassword");

                if (!Path.IsPathRooted(productionKeyPath))
                    throw new InvalidOperationException("DataProtection:KeysPath must be an absolute production path.");

                try
                {
                    Directory.CreateDirectory(productionKeyPath);
                    var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                        certificatePath,
                        certificatePassword,
                        X509KeyStorageFlags.EphemeralKeySet);

                    builder.Services.AddDataProtection()
                        .PersistKeysToFileSystem(new DirectoryInfo(productionKeyPath))
                        .ProtectKeysWithCertificate(certificate)
                        .SetApplicationName("accounting-back");
                }
                catch (Exception)
                {
                    // Production must fail closed. Do not log paths, certificate details or
                    // exception text because they can disclose deployment secrets.
                    throw new InvalidOperationException("Production DataProtection key-ring initialization failed.");
                }

                return builder;
            }

            var rootPath = Environment.GetEnvironmentVariable("APPDATA")
                ?? Environment.GetEnvironmentVariable("HOME")
                ?? environment.ContentRootPath;

            var candidatePath = rootPath != null
                ? Path.Combine(rootPath, "ASP.NET", "DataProtection-Keys")
                : Path.Combine(environment.ContentRootPath, ".aspnet-dataprotection-keys");

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

                Directory.CreateDirectory(keyDirectory);

                var directoryInfo = new DirectoryInfo(keyDirectory);
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

        private static void AddCorsPolicies(WebApplicationBuilder builder)
        {
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("ApiCors", policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
                });
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

            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                if (IsPlaceholderValue(aslBelgiServerBaseUrl))
                    throw new InvalidOperationException("AslBelgi:ServerBaseUrl is not configured with a real value.");

                if (string.IsNullOrWhiteSpace(aslBelgiAuthPath))
                    throw new InvalidOperationException("AslBelgi:AuthenticatePath is required in production.");

                if (string.IsNullOrWhiteSpace(aslBelgiRefreshPath))
                    throw new InvalidOperationException("AslBelgi:RefreshPath is required in production.");
            }
            else
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

            var eImzoCertificatePassword = configuration["EImzo:CertificatePassword"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(eImzoCertificatePassword))
                throw new InvalidOperationException("EImzo:CertificatePassword is not configured with a real secret value.");

            var emailPassword = configuration["Email:Password"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(emailPassword))
                throw new InvalidOperationException("Email:Password is not configured with a real secret value.");

            var didoxPartnerToken = configuration["TaxIntegration:Didox:PartnerToken"];
            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase) && IsPlaceholderValue(didoxPartnerToken))
                throw new InvalidOperationException("TaxIntegration:Didox:PartnerToken is not configured with a real secret value.");

            if (env.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var key in ProductionExternalSecretKeys)
                    _ = RequiredExternalSetting(configuration, key);

                foreach (var key in ForbiddenGlobalProviderSecretKeys)
                {
                    if (!IsPlaceholderValue(configuration[key]))
                        throw new InvalidOperationException($"{key} must not be configured globally in production; use the scoped credential store.");
                }
            }
        }

        private static readonly string[] ProductionExternalSecretKeys =
        [
            "ConnectionStrings:Default",
            "Jwt:Key",
            "BackupJob:Database:Password",
            "FakturaAuthSettings:ClientSecret",
            "FakturaAuthSettings:Password",
            "Email:Password",
            "EImzo:CertificatePassword",
            "EImzo:CertificatePath",
            "TaxIntegration:Didox:PartnerToken"
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
                || value.Contains("EXAMPLE", StringComparison.OrdinalIgnoreCase);
        }

        private static string RequiredExternalSetting(ConfigurationManager configuration, string key)
        {
            var value = configuration[key];
            if (IsPlaceholderValue(value) || !IsExternalConfigurationValue(configuration, key))
                throw new InvalidOperationException($"{key} must be supplied by the production environment or secret store.");

            return value!.Trim();
        }

        private static bool IsExternalConfigurationValue(ConfigurationManager configuration, string key)
        {
            if (configuration is not IConfigurationRoot root)
                return false;

            foreach (var provider in root.Providers.Reverse())
            {
                if (!provider.TryGet(key, out var value) || string.IsNullOrWhiteSpace(value))
                    continue;

                var providerName = provider.GetType().FullName ?? provider.GetType().Name;
                return providerName.Contains("EnvironmentVariables", StringComparison.OrdinalIgnoreCase)
                    || providerName.Contains("UserSecrets", StringComparison.OrdinalIgnoreCase)
                    || providerName.Contains("KeyVault", StringComparison.OrdinalIgnoreCase)
                    || providerName.Contains("Vault", StringComparison.OrdinalIgnoreCase)
                    || providerName.Contains("KeyPerFile", StringComparison.OrdinalIgnoreCase);
            }

            return false;
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
