
using ChefApi.BLL.Services.Interfaces;
using ChefApi.BLL.Services.Classes;
using ChefApi.DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChefApi.DAL.Model;
using ChefApi.DAL.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChefApi.PL.Utils;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ChefApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel for Render deployment
        ConfigureKestrel(builder);

        // Add services to the container
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline
        ConfigurePipeline(app);

        // Apply migrations and seed data
        await InitializeDatabaseAsync(app);

        app.Run();
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        // Configure Kestrel to work with Render
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(int.Parse(port));
        });

        // Additional configuration for Render
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers and API documentation
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Chef API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new()
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new()
            {
                {
                    new()
                    {
                        Reference = new()
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Configure database with PostgreSQL for production and SQL Server for development
        ConfigureDatabase(services, configuration);

        // Configure Identity
        ConfigureIdentity(services);

        // Configure Authentication & Authorization
        ConfigureAuthentication(services, configuration);

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Register custom services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ISeedData, SeedData>();
        services.AddScoped<IEmailSender, EmailSetting>();

        // Add health checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        // Configure logging
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        });
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        string connectionString;
        
        if (!string.IsNullOrEmpty(databaseUrl) && environment == "Production")
        {
            // Production: Parse Render PostgreSQL URL format: postgres://user:password@host:port/database
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
            
            Console.WriteLine($"Environment: {environment}");
            Console.WriteLine("Using PostgreSQL for production (from DATABASE_URL)");
            
            // Configure DbContext with PostgreSQL for Production
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(60);
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                }));
        }
        else
        {
            // Development: Use SQL Server from appsettings
            connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection string is not configured in appsettings.json");
            
            Console.WriteLine($"Environment: {environment}");
            Console.WriteLine($"Using SQL Server for development: {connectionString.Replace("Password=", "Password=***")}");
            
            // Configure DbContext with SQL Server for Development
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60);
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                }));
        }
    }

    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = Environment.GetEnvironmentVariable("JWTOptions__SecretKey") 
                          ?? configuration.GetSection("JWTOptions")["SecretKey"]
                          ?? throw new InvalidOperationException("JWT Secret Key not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production with HTTPS
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        // Always enable Swagger for API documentation
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chef API V1");
            c.RoutePrefix = "swagger";
        });

        // Add security headers
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            await next();
        });

        // Enable CORS
        app.UseCors("AllowAll");

        // Configure routing and authentication
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // Map endpoints
        app.MapControllers();
        app.MapHealthChecks("/health");

        // Add a root endpoint
        app.MapGet("/", () => new
        {
            Service = "Chef API",
            Version = "v1.0",
            Status = "Running",
            Environment = app.Environment.EnvironmentName,
            Timestamp = DateTime.UtcNow
        });

        // API info endpoint
        app.MapGet("/api/info", () => new
        {
            Title = "Chef API",
            Version = "1.0.0",
            Description = "RESTful API for Chef application",
            Documentation = "/swagger",
            Health = "/health"
        });
    }

    private static async Task InitializeDatabaseAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Apply migrations
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            // Seed data
            logger.LogInformation("Starting data seeding...");
            var seedData = services.GetRequiredService<ISeedData>();
            await seedData.DataSeeding();
            await seedData.IdentityDataSeeding();
            logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            
            // In production, you might want to handle this differently
            if (app.Environment.IsProduction())
            {
                logger.LogCritical("Database initialization failed in production. Application will continue but may not function correctly.");
            }
            else
            {
                throw; // Re-throw in development for easier debugging
            }
        }
    }
}
