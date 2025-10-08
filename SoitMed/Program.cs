using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Location;
using SoitMed.Models.Hospital;
using SoitMed.Services;
using SoitMed.Repositories;
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

namespace SoitMed
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure logging
            var logger = LoggerFactory.Create(builder => builder.AddConsole().AddDebug()).CreateLogger("SoitMed");
            
            try
            {
                logger.LogInformation("Starting SoitMed Backend Application...");
                var builder = WebApplication.CreateBuilder(args);
                
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
            
            // Register Sales Report Services (Legacy)
            builder.Services.AddScoped<ISalesReportRepository, SalesReportRepository>();
            builder.Services.AddScoped<ISalesReportService, SalesReportService>();
            
            // Register Weekly Plan Services (New System)
            builder.Services.AddScoped<IWeeklyPlanRepository, WeeklyPlanRepository>();
            builder.Services.AddScoped<IWeeklyPlanTaskRepository, WeeklyPlanTaskRepository>();
            builder.Services.AddScoped<IDailyProgressRepository, DailyProgressRepository>();
            builder.Services.AddScoped<IWeeklyPlanService, WeeklyPlanService>();
            
            // Register Image Upload Services
            builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
            builder.Services.AddScoped<IRoleBasedImageUploadService, RoleBasedImageUploadService>();
            
            // Register Email and Verification Services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
            
            // Register FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateSalesReportDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateWeeklyPlanDtoValidator>();
            
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
			});
			#endregion
			//--------------------------------

			var app = builder.Build();

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
                        Console.WriteLine($"Unhandled exception: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        
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
            
            // Configure static files for uploads
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                    Path.Combine(builder.Environment.WebRootPath, "uploads")),
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

            logger.LogInformation("Application configured successfully. Starting server...");
            await app.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Application startup failed: {Message}", ex.Message);
                Console.WriteLine($"Application startup failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}