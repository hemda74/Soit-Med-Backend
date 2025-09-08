
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Location;
using SoitMed.Models.Hospital;
using SoitMed.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace SoitMed
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

            // Register QR Code Service
            builder.Services.AddScoped<IQRCodeService, QRCodeService>();
			
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
					Title = "Soit-Med Hospital Management API",
					Description = "Comprehensive hospital management system with equipment tracking and repair request management"
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

			// Seed roles, departments, governorates, hospitals, and users
			using (var scope = app.Services.CreateScope())
			{
				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
				var context = scope.ServiceProvider.GetRequiredService<Context>();
				await SeedRoles(roleManager);
				await SeedDepartments(context);
				await SeedEgyptGovernorates(context);
				await SeedEgyptianHospitals(context);
				await SeedUsersWithEgyptianNames(userManager, roleManager, context);
			}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles();
            app.UseCors("MyPolicy");
            app.UseAuthentication();
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

        private static async Task SeedEgyptGovernorates(Context context)
        {
            // All 27 governorates of Egypt
            var egyptGovernorates = new List<Governorate>
            {
                new Governorate { Name = "Cairo", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Alexandria", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Giza", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Qalyubia", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Port Said", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Suez", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Luxor", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Aswan", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Asyut", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Beheira", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Beni Suef", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Dakahlia", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Damietta", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Faiyum", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Gharbia", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Ismailia", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Kafr el-Sheikh", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Matrouh", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Minya", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Monufia", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "New Valley", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "North Sinai", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Qena", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Red Sea", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Sharqia", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "Sohag", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Governorate { Name = "South Sinai", CreatedAt = DateTime.UtcNow, IsActive = true }
            };

            foreach (var governorate in egyptGovernorates)
            {
                if (!context.Governorates.Any(g => g.Name == governorate.Name))
                {
                    context.Governorates.Add(governorate);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedEgyptianHospitals(Context context)
        {
            // Egyptian hospitals with real names and locations
            var egyptianHospitals = new List<Hospital>
            {
                // Cairo Hospitals
                new Hospital { HospitalId = "HOSP001", Name = "Cairo University Hospital", Location = "Cairo", Address = "Kasr Al Ainy Street, Cairo", PhoneNumber = "+20-2-23654321" },
                new Hospital { HospitalId = "HOSP002", Name = "Ain Shams University Hospital", Location = "Cairo", Address = "Abbassia, Cairo", PhoneNumber = "+20-2-24567890" },
                new Hospital { HospitalId = "HOSP003", Name = "Al-Azhar University Hospital", Location = "Cairo", Address = "Nasr City, Cairo", PhoneNumber = "+20-2-26789012" },
                new Hospital { HospitalId = "HOSP004", Name = "Kasr Al Ainy Hospital", Location = "Cairo", Address = "Downtown Cairo", PhoneNumber = "+20-2-23456789" },
                new Hospital { HospitalId = "HOSP005", Name = "Dar Al Fouad Hospital", Location = "Cairo", Address = "6th of October City", PhoneNumber = "+20-2-38567890" },

                // Alexandria Hospitals
                new Hospital { HospitalId = "HOSP006", Name = "Alexandria University Hospital", Location = "Alexandria", Address = "El Khartoum Square, Alexandria", PhoneNumber = "+20-3-45678901" },
                new Hospital { HospitalId = "HOSP007", Name = "El Hadra University Hospital", Location = "Alexandria", Address = "El Hadra, Alexandria", PhoneNumber = "+20-3-42345678" },
                new Hospital { HospitalId = "HOSP008", Name = "Alexandria Medical Research Institute", Location = "Alexandria", Address = "Mustafa Kamel, Alexandria", PhoneNumber = "+20-3-45432109" },

                // Giza Hospitals
                new Hospital { HospitalId = "HOSP009", Name = "Giza General Hospital", Location = "Giza", Address = "Dokki, Giza", PhoneNumber = "+20-2-37654321" },
                new Hospital { HospitalId = "HOSP010", Name = "Sheikh Zayed Hospital", Location = "Giza", Address = "Sheikh Zayed City, Giza", PhoneNumber = "+20-2-38765432" },

                // Other Governorates
                new Hospital { HospitalId = "HOSP011", Name = "Mansoura University Hospital", Location = "Dakahlia", Address = "Mansoura City, Dakahlia", PhoneNumber = "+20-50-2234567" },
                new Hospital { HospitalId = "HOSP012", Name = "Tanta University Hospital", Location = "Gharbia", Address = "Tanta City, Gharbia", PhoneNumber = "+20-40-3345678" },
                new Hospital { HospitalId = "HOSP013", Name = "Assiut University Hospital", Location = "Assiut", Address = "Assiut City, Assiut", PhoneNumber = "+20-88-2456789" },
                new Hospital { HospitalId = "HOSP014", Name = "Suez Canal University Hospital", Location = "Ismailia", Address = "Ismailia City, Ismailia", PhoneNumber = "+20-64-3567890" },
                new Hospital { HospitalId = "HOSP015", Name = "Zagazig University Hospital", Location = "Sharqia", Address = "Zagazig City, Sharqia", PhoneNumber = "+20-55-2678901" }
            };

            foreach (var hospital in egyptianHospitals)
            {
                if (!context.Hospitals.Any(h => h.HospitalId == hospital.HospitalId))
                {
                    context.Hospitals.Add(hospital);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersWithEgyptianNames(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, Context context)
        {
            // Egyptian names for users
            var maleNames = new[] { "Ahmed", "Mohamed", "Omar", "Ali", "Hassan", "Mahmoud", "Khaled", "Youssef", "Amr", "Tarek", "Mostafa", "Sherif", "Hany", "Waleed", "Hesham", "Karim", "Tamer", "Ashraf", "Magdy", "Sayed" };
            var femaleNames = new[] { "Fatma", "Aisha", "Mariam", "Nour", "Sara", "Dina", "Rana", "Hala", "Noha", "Mona", "Rania", "Yasmin", "Heba", "Ola", "Nadia", "Salma", "Doaa", "Eman", "Ghada", "Layla" };
            var lastNames = new[] { "Ibrahim", "Hassan", "Mohamed", "Ali", "Mahmoud", "Ahmad", "Abdel Rahman", "Farouk", "Nasser", "Salam", "Shehata", "Mansour", "Rashid", "Zaki", "Fouad", "Gaber", "Helmy", "Kamal", "Naguib", "Sabry" };

            var departments = await context.Departments.ToListAsync();
            var hospitals = await context.Hospitals.Take(5).ToListAsync(); // Use first 5 hospitals for assignments
            var governorates = await context.Governorates.Take(10).ToListAsync(); // Use first 10 governorates for engineers

            var roles = UserRoles.GetAllRoles();
            var random = new Random();
            var userCounter = 1;

            foreach (var roleName in roles)
            {
                for (int i = 0; i < 3; i++) // 3 users per role
                {
                    // Randomly select gender and names
                    bool isMale = random.Next(2) == 0;
                    string firstName = isMale ? maleNames[random.Next(maleNames.Length)] : femaleNames[random.Next(femaleNames.Length)];
                    string lastName = lastNames[random.Next(lastNames.Length)];
                    
                    string userName = $"{firstName.ToLower()}.{lastName.ToLower()}{userCounter}";
                    string email = $"{userName}@soitmed.com";

                    // Check if user already exists
                    if (await userManager.FindByEmailAsync(email) == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = userName,
                            Email = email,
                            EmailConfirmed = true,
                            FirstName = firstName,
                            LastName = lastName,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true,
                            DepartmentId = GetDepartmentIdForRole(roleName, departments)
                        };

                        var result = await userManager.CreateAsync(user, "Password123!");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, roleName);

                            // Create role-specific records
                            await CreateRoleSpecificRecord(user, roleName, context, hospitals, governorates, random);
                        }
                    }
                    userCounter++;
                }
            }

            await context.SaveChangesAsync();
        }

        private static int? GetDepartmentIdForRole(string roleName, List<Department> departments)
        {
            var departmentName = roleName.ToLower() switch
            {
                "superadmin" or "admin" => "Administration",
                "doctor" => "Medical",
                "technician" => "Medical", 
                "salesman" => "Sales",
                "engineer" => "Engineering",
                "financemanager" => "Finance",
                "financeemployee" => "Finance",
                "legalmanager" => "Legal",
                "legalemployee" => "Legal",
                _ => "Administration"
            };

            return departments.FirstOrDefault(d => d.Name == departmentName)?.Id;
        }

        private static async Task CreateRoleSpecificRecord(ApplicationUser user, string roleName, Context context, List<Hospital> hospitals, List<Governorate> governorates, Random random)
        {
            switch (roleName.ToLower())
            {
                case "doctor":
                    var medicalSpecialties = new[] { "Cardiology", "Neurology", "Orthopedics", "Pediatrics", "Internal Medicine", "Surgery", "Radiology", "Anesthesiology", "Emergency Medicine", "Dermatology" };
                    var doctor = new Doctor
                    {
                        Name = user.FullName,
                        Specialty = medicalSpecialties[random.Next(medicalSpecialties.Length)],
                        HospitalId = hospitals[random.Next(hospitals.Count)].HospitalId,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    context.Doctors.Add(doctor);
                    break;

                case "technician":
                    var techDepartments = new[] { "Radiology", "Laboratory", "Cardiology", "Respiratory", "Dialysis", "Pharmacy", "ICU", "Emergency", "Surgery", "Maintenance" };
                    var technician = new Technician
                    {
                        Name = user.FullName,
                        Department = techDepartments[random.Next(techDepartments.Length)],
                        HospitalId = hospitals[random.Next(hospitals.Count)].HospitalId,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    context.Technicians.Add(technician);
                    break;

                case "engineer":
                    var engineeringSpecialties = new[] { "Biomedical Engineering", "Electrical Engineering", "Mechanical Engineering", "Software Engineering", "Network Engineering", "HVAC Engineering" };
                    var engineer = new Engineer
                    {
                        Name = user.FullName,
                        Specialty = engineeringSpecialties[random.Next(engineeringSpecialties.Length)],
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    context.Engineers.Add(engineer);

                    // Assign random governorates to engineer
                    var assignedGovernorates = governorates.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList();
                    foreach (var gov in assignedGovernorates)
                    {
                        context.EngineerGovernorates.Add(new EngineerGovernorate
                        {
                            Engineer = engineer,
                            GovernorateId = gov.GovernorateId,
                            AssignedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                    break;
            }
        }
    }
}
