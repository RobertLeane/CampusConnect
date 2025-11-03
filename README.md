# CampusConnect

A modern campus club and event management platform built with Blazor WebAssembly and ASP.NET Core.

## 🎯 Overview

Campus Connect encompasses 6 Blazor Razor pages: Login, Register, Home, Clubs, Events, and Dashboard. The application is containerized and deployed to Azure using Docker.

### Technology Stack

- **Frontend**: Blazor WebAssembly, HTML/CSS
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: Azure SQL Database
- **Storage**: Azure Blob Storage (for images)
- **Authentication**: Custom authentication (database-based)
- **Monitoring**: Azure Application Insights
- **Deployment**: Docker containers via Docker Hub to Azure
- **Testing**: bUnit framework

### Features

- **User Authentication**: Secure login and registration
- **Club Management**: Browse and join campus clubs
- **Event Management**: View and register for club events
- **Dashboard**: Personal overview of memberships and upcoming events

## 🚀 Quick Start

### Local Development with Docker

```powershell
# Build and run locally
docker-compose up --build

# Access the application
# Client: http://localhost
# API: http://localhost:8080
```

### Using Deployment Script

```powershell
# Run locally
.\deploy.ps1 -Action local

# Push to Docker Hub
.\deploy.ps1 -Action dockerhub -DockerHubUsername YOUR_USERNAME

# Deploy to Azure Container Apps
.\deploy.ps1 -Action azure-containerapp -DockerHubUsername YOUR_USERNAME
```

## 📚 Documentation

- **[Quick Start Guide](QUICKSTART.md)** - Get started with Docker and Azure deployment
- **[Detailed Deployment Guide](DOCKER_DEPLOYMENT.md)** - Comprehensive deployment instructions
- **[GitHub Actions Workflows](.github/workflows/)** - CI/CD pipeline configurations

## 🏗️ Architecture

```
┌─────────────────┐         ┌──────────────────┐
│  Blazor WASM    │────────▶│   ASP.NET Core   │
│   (Client)      │         │   Web API        │
│   Port: 80      │         │   Port: 8080     │
└─────────────────┘         └──────────────────┘
                                     │
                    ┌────────────────┼────────────────┐
                    ▼                ▼                ▼
            ┌──────────────┐  ┌──────────┐  ┌─────────────┐
            │  Azure SQL   │  │  Blob    │  │ Application │
            │  Database    │  │ Storage  │  │  Insights   │
            └──────────────┘  └──────────┘  └─────────────┘
```

## 🐳 Docker Images

The application is available as two Docker images:
- `campusconnect-api`: ASP.NET Core Web API backend
- `campusconnect-client`: Blazor WebAssembly frontend (served via nginx)

## 🌐 Deployment Options

### 1. Azure Container Apps (Recommended)
- Fully managed container hosting
- Auto-scaling capabilities
- Built-in load balancing
- Easy configuration

### 2. Azure App Service (Web App for Containers)
- Managed web hosting
- Continuous deployment from Docker Hub
- Integrated monitoring

### 3. Infrastructure as Code (Bicep)
- Declarative infrastructure deployment
- Version-controlled infrastructure
- Repeatable deployments

## 🔄 CI/CD Pipeline

The project includes two GitHub Actions workflows:

1. **Docker Build and Push** (`.github/workflows/docker-publish.yml`)
   - Automatically builds and pushes images to Docker Hub on push to main
   - Tags images with version, SHA, and 'latest'

2. **Azure Deployment** (`.github/workflows/azure-deploy.yml`)
   - Deploys infrastructure using Bicep
   - Configures Container Apps
   - Validates deployment

## 🛠️ Development

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Azure CLI (for Azure deployments)
- Visual Studio 2022 or VS Code

### Local Development (Without Docker)

```powershell
# Restore dependencies
dotnet restore

# Run API
cd api
dotnet run

# Run Client (in a new terminal)
cd client
dotnet run
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔐 Configuration

### Environment Variables

**API Container:**
- `ASPNETCORE_ENVIRONMENT`: Environment name (Development/Production)
- `ConnectionStrings__DefaultConnection`: SQL connection string
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Application Insights connection

**Client Container:**
- `API_BASE_URL`: Base URL for the API

### GitHub Secrets

Required for CI/CD:
- `DOCKERHUB_USERNAME`: Docker Hub username
- `DOCKERHUB_TOKEN`: Docker Hub access token
- `AZURE_CREDENTIALS`: Azure service principal credentials
- `AZURE_SUBSCRIPTION_ID`: Azure subscription ID
- `SQL_CONNECTION_STRING`: Database connection string
- `SQL_ADMIN_LOGIN`: SQL admin username
- `SQL_ADMIN_PASSWORD`: SQL admin password

## 📊 Monitoring

Application Insights provides:
- Real-time performance metrics
- Request/response monitoring
- Exception tracking
- Custom telemetry

Access monitoring data in the Azure Portal or via:
```powershell
az monitor app-insights metrics show --app campusconnect-insights --metrics requests/count
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests
5. Submit a pull request

## 📝 License

This project is licensed for educational purposes.

## 🔗 Links

- **Live Site**: [campusconnect-a5gmchaahpf4ajem.uksouth-01.azurewebsites.net](https://campusconnect-a5gmchaahpf4ajem.uksouth-01.azurewebsites.net)
- **Docker Hub**: [hub.docker.com](https://hub.docker.com)
- **Azure Portal**: [portal.azure.com](https://portal.azure.com)
