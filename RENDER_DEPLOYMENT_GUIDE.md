# ChefApi - Complete Render Deployment Guide

## 🎯 Overview

Your ASP.NET Core 8 Web API is now configured for **dual database support**:
- **Production (Render)**: PostgreSQL via `DATABASE_URL` environment variable
- **Development (Local)**: SQL Server LocalDB

## ✅ What Was Fixed

### 1. **Database Configuration**
```csharp
// Production: PostgreSQL from DATABASE_URL
if (!string.IsNullOrEmpty(databaseUrl) && environment == "Production")
{
    // Parse DATABASE_URL and use PostgreSQL
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else 
{
    // Development: SQL Server from appsettings
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

### 2. **Environment Detection**
- `ASPNETCORE_ENVIRONMENT=Production` → PostgreSQL
- `ASPNETCORE_ENVIRONMENT=Development` → SQL Server

### 3. **Automatic Migrations**
- EF Core migrations applied on startup
- Database seeding included
- Works with both SQL Server and PostgreSQL

## 🐳 Dockerfile Features

### Multi-Stage Build
- **Build Stage**: `mcr.microsoft.com/dotnet/sdk:8.0`
- **Runtime Stage**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Security**: Non-root user execution
- **Optimization**: Layer caching for faster builds

### Environment Variables
```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "ChefApi.PL.dll"]
```

## 🌍 Environment Variables

### Required by Render (Automatically Set)
| Variable | Source | Description |
|----------|--------|-------------|
| `PORT` | Render | Dynamic port assignment |
| `DATABASE_URL` | Render PostgreSQL | Full PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | render.yaml | Set to "Production" |

### Required Configuration (Set in Render Dashboard)
| Variable | Example | Description |
|----------|---------|-------------|
| `JWTOptions__SecretKey` | Auto-generated | JWT secret key |
| `EmailSettings__SmtpUsername` | your@gmail.com | SMTP username |
| `EmailSettings__SmtpPassword` | app_password | SMTP password |
| `EmailSettings__FromEmail` | your@gmail.com | From email address |

## 🚀 Deployment Steps

### 1. **Prepare Repository**
```bash
git add .
git commit -m "Configure dual database support for Render deployment"
git push origin main
```

### 2. **Create Render Service**
1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click "New +" → "Web Service"
3. Connect your GitHub repository
4. Render will automatically detect `render.yaml` and configure everything

### 3. **Set Email Configuration**
In Render dashboard, go to Environment tab and add:
```
EmailSettings__SmtpUsername=your@gmail.com
EmailSettings__SmtpPassword=your_app_password
EmailSettings__FromEmail=your@gmail.com
```

### 4. **Deploy**
Render will automatically:
- ✅ Create PostgreSQL database
- ✅ Set `DATABASE_URL` environment variable
- ✅ Build Docker image
- ✅ Deploy your application
- ✅ Apply EF Core migrations
- ✅ Seed database

## 🧪 Testing Results

### ✅ Production Environment Test
```
Environment: Production
Using PostgreSQL for production (from DATABASE_URL)
✅ Database migrations applied successfully
✅ Data seeding completed successfully  
✅ Application started
✅ Listening on port 7000 (dynamic PORT)
```

### 🔧 Development Environment (Local)
```bash
# Uses SQL Server LocalDB
dotnet run --environment Development
```

## 📁 Project Structure

```
ChefApi/
├── ChefApi.PL/           # Main Web API project
│   ├── Program.cs        # ✅ Dual database configuration
│   ├── appsettings.json  # ✅ SQL Server for development
│   └── Dockerfile        # ✅ Production-ready
├── ChefApi.BLL/          # Business Logic Layer
├── ChefApi.DAL/          # Data Access Layer
│   └── Migrations/       # ✅ PostgreSQL-compatible
├── render.yaml           # ✅ Render deployment config
└── README.md
```

## 🔗 API Endpoints

After deployment, your API will be available at:
- **Swagger**: `https://your-app.onrender.com/swagger`
- **Health Check**: `https://your-app.onrender.com/health`
- **API Info**: `https://your-app.onrender.com/api/info`

## 🔍 Monitoring & Debugging

### View Logs
```bash
# In Render dashboard, go to "Logs" tab
# Or use Render CLI:
render logs -s your-service-name
```

### Health Check
Your app includes a health check endpoint that Render monitors:
```
GET /health
```

### Database Connection
Check logs for these messages:
- ✅ `Using PostgreSQL for production (from DATABASE_URL)`
- ✅ `Database migrations applied successfully`
- ✅ `Data seeding completed successfully`

## 🔐 Security Features

- ✅ **Non-root container user**
- ✅ **Environment-based configuration**
- ✅ **Secure JWT secret generation**
- ✅ **SSL/TLS for PostgreSQL**
- ✅ **CORS configuration**

## 🎯 Next Steps After Deployment

1. **Test API endpoints** via Swagger UI
2. **Verify email functionality** with real SMTP settings
3. **Monitor application logs** in Render dashboard
4. **Set up custom domain** (if needed)
5. **Configure environment-specific settings**

## 🆘 Troubleshooting

### Migration Issues
If migrations fail, check:
- DATABASE_URL is properly set by Render
- PostgreSQL database is accessible
- Network connectivity to database

### Connection Errors
- Verify `ASPNETCORE_ENVIRONMENT=Production`
- Check DATABASE_URL format in logs
- Ensure PostgreSQL service is running

### Build Issues
- Check Dockerfile syntax
- Verify all required packages are included
- Review build logs in Render dashboard

---

## 🎉 Your ChefApi is Ready for Production!

The application now automatically:
- ✅ Uses PostgreSQL in Production (via DATABASE_URL)
- ✅ Uses SQL Server in Development (via appsettings)
- ✅ Applies migrations on startup
- ✅ Handles environment-specific configuration
- ✅ Runs securely in Docker containers

**Deploy to Render and enjoy your production-ready API!** 🚀
