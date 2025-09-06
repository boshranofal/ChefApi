# ChefApi - .NET 8 Web API

A RESTful API built with .NET 8, designed for cloud deployment on Render with Docker support.

## Features

- **Authentication & Authorization**: JWT-based authentication with ASP.NET Identity
- **Database**: PostgreSQL (production) / SQL Server (development)
- **Email Services**: SMTP-based email notifications
- **API Documentation**: Swagger/OpenAPI integration
- **Health Monitoring**: Built-in health checks
- **Cloud-Ready**: Optimized for Render deployment with Docker

## Quick Start

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/boshranofal/ChefApi.git
   cd ChefApi
   ```

2. **Set up environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your local database and email settings
   ```

3. **Run with Docker**
   ```bash
   docker build -t chefapi .
   docker run -p 8080:8080 --env-file .env chefapi
   ```

4. **Or run locally**
   ```bash
   dotnet restore
   dotnet run --project ChefApi.PL
   ```

### API Endpoints

- **API Documentation**: `http://localhost:8080/swagger`
- **Health Check**: `http://localhost:8080/health`
- **API Info**: `http://localhost:8080/api/info`
- **Authentication**: `http://localhost:8080/api/Identity/Account/`

## Render Deployment

### 1. Environment Variables

Set these in Render Dashboard:

| Variable | Value | Required |
|----------|-------|----------|
| `DATABASE_URL` | Provided by Render PostgreSQL | ✅ |
| `JWTOptions__SecretKey` | Your JWT secret key (256+ bits) | ✅ |
| `EmailSettings__SmtpServer` | smtp.gmail.com | ✅ |
| `EmailSettings__SmtpPort` | 587 | ✅ |
| `EmailSettings__SmtpUsername` | Your Gmail address | ✅ |
| `EmailSettings__SmtpPassword` | Your Gmail app password | ✅ |
| `EmailSettings__FromEmail` | Your Gmail address | ✅ |
| `EmailSettings__FromName` | Chef API | ✅ |
| `PORT` | Provided by Render | ✅ |
| `ASPNETCORE_ENVIRONMENT` | Production | ✅ |

### 2. Deploy Steps

1. **Push to GitHub**
   ```bash
   git add .
   git commit -m "Deploy to Render"
   git push origin master
   ```

2. **Create Render Service**
   - Go to [Render Dashboard](https://dashboard.render.com)
   - Create new "Web Service"
   - Connect your GitHub repository
   - Choose "Docker" environment
   - Set environment variables

3. **Create PostgreSQL Database**
   - Create new "PostgreSQL" service
   - Link to your web service

### 3. Automatic Features

- ✅ **Database Migrations**: Applied automatically on startup
- ✅ **Data Seeding**: Initial data created automatically
- ✅ **Health Monitoring**: Available at `/health` endpoint
- ✅ **HTTPS**: Handled by Render
- ✅ **Scaling**: Managed by Render

## Project Structure

```
ChefApi/
├── ChefApi.PL/           # Presentation Layer (Web API)
├── ChefApi.BLL/          # Business Logic Layer
├── ChefApi.DAL/          # Data Access Layer
├── Dockerfile            # Docker configuration
├── render.yaml          # Render deployment config
└── docker-run.bat       # Local Docker helper script
```

## Configuration

### Database Configuration

The application automatically detects the environment:

- **Production**: Uses PostgreSQL via `DATABASE_URL` environment variable
- **Development**: Uses SQL Server or PostgreSQL based on connection string

### Security Features

- JWT token authentication
- CORS configuration
- Security headers
- Email confirmation for user registration
- Password reset functionality

## Troubleshooting

### Common Issues

1. **Database Connection**: Ensure `DATABASE_URL` is properly set
2. **JWT Errors**: Verify `JWTOptions__SecretKey` is 256+ bits
3. **Email Issues**: Check Gmail app password and SMTP settings
4. **Port Issues**: Render automatically provides `PORT` environment variable

### Logs

Check application logs in Render Dashboard for detailed error information.

## API Documentation

Once deployed, visit `/swagger` for interactive API documentation with authentication support.

## Support

For issues and questions, please check the application logs and ensure all required environment variables are properly configured.
