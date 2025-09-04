
using Lab1.Models;
using Lab1.Models.Identity;
using Lab1.Models.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Lab1
{
    public class Program
    {
        public static async Task Main(string[] args)
		
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<Context>(option => {
                option
                .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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

            builder.Services.AddIdentity<ApplicationUser,IdentityRole>().AddEntityFrameworkStores<Context>();
			
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
				//This�is�to�generate�the�Default�UI�of�Swagger�Documentation����
				swagger.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "ASP.NET�Core�Web�API E-Commerce",
					Description = " ITI Project"
				});
				//�To�Enable�authorization�using�Swagger�(JWT)����
				swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
				{
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter�'Bearer'�[space]�and�then�your�valid�token�in�the�text�input�below.\r\n\r\nExample:�\"Bearer�eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
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

			// Seed roles and departments
			using (var scope = app.Services.CreateScope())
			{
				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
				var context = scope.ServiceProvider.GetRequiredService<Context>();
				await SeedRoles(roleManager);
				await SeedDepartments(context);
			}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles();
            app.UseCors("MyPolicy");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = UserRoles.GetAllRoles();
            
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedDepartments(Context context)
        {
            // Define departments with descriptions
            var departments = new List<Department>
            {
                new Department { Name = "Administration", Description = "Administrative and management roles" },
                new Department { Name = "Medical", Description = "Medical staff including doctors and technicians" },
                new Department { Name = "Sales", Description = "Sales team and customer relations" },
                new Department { Name = "Engineering", Description = "Technical and engineering staff" },
                new Department { Name = "Finance", Description = "Financial management and accounting" },
                new Department { Name = "Legal", Description = "Legal affairs and compliance" }
            };

            foreach (var department in departments)
            {
                if (!context.Departments.Any(d => d.Name == department.Name))
                {
                    context.Departments.Add(department);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
