using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Location;
using SoitMed.Models.Hospital;
using SoitMed.Services;
using SoitMed.Repositories;
using SoitMed.Extensions;
using SoitMed.Validators;
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

namespace SoitMed
{
    public class Program
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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<Context>(option => {
                option
                .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60);
                })
                .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .EnableServiceProviderCaching()
                .EnableDetailedErrors(builder.Environment.IsDevelopment());
            });
            
            // Allow reasonably sized image uploads (up to 20MB)
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 20L * 1024 * 1024; // 20MB
            });
           
            builder.Services.AddCors(options => {
                options.AddPolicy("MyPolicy",
                                  policy => policy.AllowAnyMethod()
                                  .AllowAnyOrigin()
                                  .AllowAnyHeader());
            });
			///this for make authorization to Admin
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
			});

            builder.Services.AddIdentity<ApplicationUser,IdentityRole>()
                .AddEntityFrameworkStores<Context>()
                .AddDefaultTokenProviders();

            // Register QR Code Service
            builder.Services.AddScoped<IQRCodeService, QRCodeService>();
            
            // Register User ID Generation Service
            builder.Services.AddScoped<UserIdGenerationService>();
            
            // Register Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Register Sales Report Services
            builder.Services.AddScoped<ISalesReportRepository, SalesReportRepository>();
            builder.Services.AddScoped<ISalesReportService, SalesReportService>();
            
            // Register Image Upload Services
            builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
            builder.Services.AddScoped<IRoleBasedImageUploadService, RoleBasedImageUploadService>();
            
            // Register Email and Verification Services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
            
            // Register Workflow and Notification Services
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IRequestWorkflowService, RequestWorkflowService>();
            
            // Register new refactored services
            builder.Services.AddApplicationServices();
            
            // Register SignalR
            builder.Services.AddSignalR();
            
            // Register FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateSalesReportDtoValidator>();
            
            // Add Health Checks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<Context>("database")
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), new[] { "ready" });
            
            // Add Memory Cache
            builder.Services.AddMemoryCache();
            
            // Add Response Caching
            builder.Services.AddResponseCaching();
            
            
			
			builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme=JwtBearerDefaults.AuthenticationScheme;

			}).AddJwtBearer(options => {
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
				
				// Configure JWT for SignalR
				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						var accessToken = context.Request.Query["access_token"];
						var path = context.HttpContext.Request.Path;
						
						if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
						{
							context.Token = accessToken;
						}
						
						return Task.CompletedTask;
					}
				};
			});


			/*-----------------------------Swagger PArt-----------------------------*/
			#region Swagger REgion
			//builder.Services.AddSwaggerGen();

			builder.Services.AddSwaggerGen(swagger =>
			{
				//ThisistogeneratetheDefaultUIofSwaggerDocumentation
				swagger.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Soit-Med Hospital Management API",
					Description = "Comprehensive hospital management system with equipment tracking and repair request management"
				});
				
				
				//ToEnableauthorizationusingSwagger(JWT)
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

                // Render IFormFile as file input in Swagger UI
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
			//--------------------------------

			var app = builder.Build();

            // Ensure critical tables exist (guard against drift in dev/test)
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
                    // Log and continue startup; controller action will still surface DB errors if any
                    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                    var startupLogger = loggerFactory.CreateLogger("StartupDbGuard");
                    startupLogger.LogError(guardEx, "Failed to ensure DoctorHospitals table exists");
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            // Add exception handling middleware
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
                        // Logging handled by configured providers
                        
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "An internal server error occurred",
                            message = "Please try again later"
                        }));
                    }
                });
            });
            app.UseStatusCodePages();
            
            app.UseStaticFiles();
            
            // Configure static files for uploads served from outside the project
            var externalUploadsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SoitMed", "uploads");
            Directory.CreateDirectory(externalUploadsRoot);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(externalUploadsRoot),
                RequestPath = "/uploads"
            });
            
            app.UseCors("MyPolicy");
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseAuthorization();

            // Add Health Check endpoints
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
            
            // Map SignalR hubs
            app.MapHub<SoitMed.Hubs.NotificationHub>("/notificationHub");

            

            logger.LogInformation("Application configured successfully. Starting server...");
            await app.RunAsync();
            }
            catch (Exception ex)
            {
                // Log and exit gracefully
                logger.LogError(ex, "Application error: {Message}", ex.Message);
            }
        }
    }
}