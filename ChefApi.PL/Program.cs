
using ChefApi.BLL.Services.Interfaces;
using ChefApi.BLL.Services.Classes;
using ChefApi.DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChefApi.DAL.Model;
using System;
using ChefApi.DAL.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChefApi.PL.Utils;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ChefApi
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
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            })
   .AddEntityFrameworkStores<ApplicationDbContext>()
   .AddDefaultTokenProviders(); 



            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTOptions")["SecretKey"]))
            };
        });


            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<ISeedData, SeedData>();
            builder.Services.AddScoped<IEmailSender, EmailSetting>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            // Add health check endpoint
            app.MapGet("/health", () => "Healthy");

            // Use Authentication before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Database migration and seeding in production
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await context.Database.MigrateAsync(); // Apply migrations automatically
                    
                    var seedData = scope.ServiceProvider.GetRequiredService<ISeedData>();
                    await seedData.DataSeeding();
                    await seedData.IdentityDataSeeding();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                }
            }

            app.Run();
        }
    }
}
