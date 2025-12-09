using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Location;
using SoitMed.Models.Hospital;
using SoitMed.Services;
using SoitMed.Repositories;
using SoitMed.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.Common;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SoitMed
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure logging
            var logger = LoggerFactory.Create(builder => builder.AddConsole().AddDebug()).CreateLogger("SoitMed");

            // Global exception handlers to avoid process termination
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                try
                {
                    var ex = eventArgs.ExceptionObject as Exception;
                    logger.LogError(ex, "UnhandledException: {Message}", ex?.Message);
                    Console.WriteLine($"[UnhandledException] {ex?.Message}\n{ex?.StackTrace}");
                }
                catch { /* swallow logging errors */ }
            };
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                try
                {
                    logger.LogError(eventArgs.Exception, "UnobservedTaskException: {Message}", eventArgs.Exception.Message);
                    Console.WriteLine($"[UnobservedTaskException] {eventArgs.Exception.Message}\n{eventArgs.Exception.StackTrace}");
                    eventArgs.SetObserved();
                }
                catch { /* swallow logging errors */ }
            };

            try
            {
                logger.LogInformation("Starting SoitMed Backend Application...");
                var builder = WebApplication.CreateBuilder(args);
                // Ensure web root exists and is set (needed for file uploads)
                var ensuredWebRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
                Directory.CreateDirectory(ensuredWebRoot);
                builder.WebHost.UseWebRoot(ensuredWebRoot);

                // Configure logging
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.AddDebug();
                builder.Logging.SetMinimumLevel(LogLevel.Information);

                // Add services to the container.
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                
                builder.Services.AddDbContext<Context>(option =>
                {
                    option
                    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(120); // Increased timeout for complex queries
                        // Use split queries for better performance with multiple collections
                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    })
                    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                    .EnableServiceProviderCaching()
                    .EnableDetailedErrors(builder.Environment.IsDevelopment())
                    .ConfigureWarnings(warnings =>
                    {
                        // Suppress the multiple collection include warning since we're using SplitQuery
                        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning);
                    });
                }, ServiceLifetime.Scoped); // Explicitly set to Scoped to ensure proper disposal

                // Allow reasonably sized image uploads (up to 20MB)
                builder.Services.Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB
                    options.ValueLengthLimit = int.MaxValue;
                    options.ValueCountLimit = int.MaxValue;
                    options.KeyLengthLimit = int.MaxValue;
                    options.MultipartHeadersLengthLimit = int.MaxValue;
                    options.MultipartBoundaryLengthLimit = int.MaxValue;
                });

                // Configure Kestrel for better file upload handling
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = 20L * 1024 * 1024; // 20MB
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
                    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                    options.Limits.MaxRequestBufferSize = 1024 * 1024;
                    options.Limits.MaxRequestLineSize = 8192;
                    options.Limits.MaxRequestHeaderCount = 100;
                    options.Limits.MaxRequestHeadersTotalSize = 32 * 1024;
                });

                // Configure request size limits
                builder.Services.Configure<IISServerOptions>(options =>
                {
                    options.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB
                });

                // Configure memory management
                builder.Services.Configure<Microsoft.Extensions.Caching.Memory.MemoryCacheOptions>(options =>
                {
                    options.SizeLimit = 100 * 1024 * 1024; // 100MB cache limit
                });

                // Add health checks for monitoring
                builder.Services.AddHealthChecks()
                    .AddDbContextCheck<Context>("database")
                    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), new[] { "ready" })
                    .AddCheck("memory", () =>
                    {
                        var memory = GC.GetTotalMemory(false);
                        var maxMemory = 500 * 1024 * 1024; // 500MB threshold
                        return memory < maxMemory ?
                            HealthCheckResult.Healthy($"Memory usage: {memory / 1024 / 1024}MB") :
                            HealthCheckResult.Unhealthy($"High memory usage: {memory / 1024 / 1024}MB");
                    });

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("MyPolicy",
                                      policy => policy
                                      // Allow ALL origins - any device can call the API
                                      .SetIsOriginAllowed(_ => true)
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials()
                                      // Set preflight cache duration
                                      .SetPreflightMaxAge(TimeSpan.FromDays(1)));
                });
                
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                });

                builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<Context>()
                    .AddDefaultTokenProviders();

                // Register services
                builder.Services.AddScoped<IQRCodeService, QRCodeService>();
                builder.Services.AddScoped<UserIdGenerationService>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<ISalesmanStatisticsService, SalesmanStatisticsService>();
                builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
                builder.Services.AddScoped<IPdfUploadService, PdfUploadService>();
                builder.Services.AddScoped<IOfferPdfService, OfferPdfService>();
                builder.Services.AddScoped<IRoleBasedImageUploadService, RoleBasedImageUploadService>();
                builder.Services.AddScoped<IVoiceUploadService, VoiceUploadService>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
                builder.Services.AddScoped<INotificationService, NotificationService>();
                builder.Services.AddHttpClient<IMobileNotificationService, MobileNotificationService>();
                builder.Services.AddScoped<IRequestWorkflowService, RequestWorkflowService>();
                builder.Services.AddHttpContextAccessor(); // Required for ChatService
                builder.Services.AddApplicationServices();
                builder.Services.AddSignalR();
                builder.Services.AddFluentValidationAutoValidation();
                builder.Services.AddMemoryCache();
                builder.Services.AddResponseCaching();

                // Configure Rate Limiting
                builder.Services.AddRateLimiter(options =>
                {
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.User?.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 100,
                                QueueLimit = 50,
                                Window = TimeSpan.FromMinutes(1)
                            }));

                    options.AddPolicy("API", httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.User?.Identity?.Name ?? httpContext.Connection.Id,
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 200,
                                QueueLimit = 100,
                                Window = TimeSpan.FromMinutes(1)
                            }));

                    options.AddPolicy("Auth", httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 10,
                                QueueLimit = 5,
                                Window = TimeSpan.FromMinutes(1)
                            }));
                });

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JWT:ValidIss"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JWT:ValidAud"],
                        IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecritKey"] ?? ""))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var path = context.HttpContext.Request.Path;
                            if (path.StartsWithSegments("/notificationHub") || path.StartsWithSegments("/chatHub"))
                            {
                                var tokenFromHeader = context.Request.Headers["Authorization"].ToString();
                                if (!string.IsNullOrEmpty(tokenFromHeader) && tokenFromHeader.StartsWith("Bearer "))
                                {
                                    context.Token = tokenFromHeader.Substring("Bearer ".Length).Trim();
                                    return Task.CompletedTask;
                                }
                                var accessToken = context.Request.Query["access_token"];
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    context.Token = accessToken;
                                }
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

                #region Swagger Configuration
                builder.Services.AddSwaggerGen(swagger =>
                {
                    swagger.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "SoitMed",
                        Description = ""
                    });

                    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter'Bearer'[space]andthenyourvalidtokeninthetextinputbelow.\r\n\r\nExample:\"BearereyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                    });
                    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                    {
                    new OpenApiSecurityScheme
                    {
                    Reference = new OpenApiReference
                    {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                    }
                    },
                    new string[] {}
                    }
                        });

                    swagger.MapType<IFormFile>(() => new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    });
                    swagger.MapType<IEnumerable<IFormFile>>(() => new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string", Format = "binary" }
                    });
                });
                #endregion

                var app = builder.Build();

                // Ensure critical tables exist
                using (var scope = app.Services.CreateScope())
                {
                    try
                    {
                        var db = scope.ServiceProvider.GetRequiredService<Context>();
                        var connection = db.Database.GetDbConnection();
                        await connection.OpenAsync();
                        using var command = connection.CreateCommand();
                        command.CommandText = @"IF OBJECT_ID(N'[dbo].[DoctorHospitals]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DoctorHospitals] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [DoctorId] INT NOT NULL,
        [HospitalId] NVARCHAR(450) NOT NULL,
        [AssignedAt] DATETIME2 NOT NULL,
        [IsActive] BIT NOT NULL,
        CONSTRAINT [PK_DoctorHospitals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DoctorHospitals_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [dbo].[Doctors] ([DoctorId]) ON DELETE CASCADE,
        CONSTRAINT [FK_DoctorHospitals_Hospitals_HospitalId] FOREIGN KEY ([HospitalId]) REFERENCES [dbo].[Hospitals] ([HospitalId]) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_DoctorHospitals_DoctorId_HospitalId]
        ON [dbo].[DoctorHospitals] ([DoctorId], [HospitalId]);

    CREATE INDEX [IX_DoctorHospitals_HospitalId]
        ON [dbo].[DoctorHospitals] ([HospitalId]);
END";
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception guardEx)
                    {
                        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                        var startupLogger = loggerFactory.CreateLogger("StartupDbGuard");
                        startupLogger.LogError(guardEx, "Failed to ensure DoctorHospitals table exists");
                    }
                }

                // Ensure SalesmanTargets table has TargetType and TargetRevenue columns
                using (var scope = app.Services.CreateScope())
                {
                    try
                    {
                        var db = scope.ServiceProvider.GetRequiredService<Context>();
                        var connection = db.Database.GetDbConnection();
                        await connection.OpenAsync();
                        
                        // Check and add TargetType column
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[SalesmanTargets]') 
    AND name = 'TargetType'
)
BEGIN
    ALTER TABLE [dbo].[SalesmanTargets]
    ADD [TargetType] INT NOT NULL DEFAULT 2;
END";
                            await command.ExecuteNonQueryAsync();
                        }
                        
                        // Check and add TargetRevenue column
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[SalesmanTargets]') 
    AND name = 'TargetRevenue'
)
BEGIN
    ALTER TABLE [dbo].[SalesmanTargets]
    ADD [TargetRevenue] DECIMAL(18, 2) NULL;
END";
                            await command.ExecuteNonQueryAsync();
                        }
                        
                        logger.LogInformation("SalesmanTargets table columns verified/updated");
                    }
                    catch (Exception guardEx)
                    {
                        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                        var startupLogger = loggerFactory.CreateLogger("StartupDbGuard");
                        startupLogger.LogError(guardEx, "Failed to ensure SalesmanTargets columns exist");
                    }
                }

                // Seed roles
                using (var scope = app.Services.CreateScope())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var roles = UserRoles.GetAllRoles();

                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole { Name = role });
                            logger.LogInformation($"Created role: {role}");
                        }
                    }
                }

                // Configure the HTTP request pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";

                        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            var ex = error.Error;
                            var logger = context.RequestServices.GetService<ILogger<Program>>();
                            logger?.LogError(ex, "Unhandled exception in request pipeline");

                            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                            {
                                error = "An internal server error occurred",
                                message = "Please try again later",
                                details = app.Environment.IsDevelopment() ? ex.Message : null
                            }));
                        }
                    });
                });

                app.Use(async (context, next) =>
                {
                    try
                    {
                        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                        context.Response.Headers.Append("X-Frame-Options", "DENY");
                        await next();
                    }
                    catch (Exception ex)
                    {
                        var logger = context.RequestServices.GetService<ILogger<Program>>();
                        logger?.LogError(ex, "Error in custom middleware");

                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Request processing failed",
                            message = "Please try again with a smaller file or check your connection"
                        }));
                    }
                });
                
                app.UseStatusCodePages();
                
                // Add CORS middleware for static files BEFORE UseStaticFiles
                app.UseMiddleware<SoitMed.Middleware.StaticFileCorsMiddleware>();
                
                // Configure static files - MUST be before authentication/authorization
                // wwwroot is already set as web root, so files are served from root
                // Files in wwwroot/products/ are accessible at /products/
                // Files in wwwroot/images/ are accessible at /images/
                app.UseStaticFiles();
                
                // CORS must be before authentication
                app.UseCors("MyPolicy");
                app.UseResponseCaching();
                app.UseRateLimiter();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapHealthChecks("/health");
                app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
                {
                    Predicate = check => check.Tags.Contains("ready"),
                });
                app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
                {
                    Predicate = _ => false,
                });

                app.MapControllers();
                app.MapHub<SoitMed.Hubs.NotificationHub>("/notificationHub");
                app.MapHub<SoitMed.Hubs.ChatHub>("/chatHub");

                logger.LogInformation("Application configured successfully. Starting server...");
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Application error: {Message}", ex.Message);
            }
        }
    }
}



