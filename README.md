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
- **Deployment**: Docker containers on Azure Container Apps
- **Monitoring**: Application Insights (manual SDK integration)
- **Testing**: bUnit framework

### Features

- **User Authentication**: Secure login and registration
- **Club Management**: Browse and join campus clubs
- **Event Management**: View and register for club events
- **Dashboard**: Personal overview of memberships and upcoming events

## 🚀 Quick Start

### Using Docker Compose

```powershell
# Build and run locally
docker-compose up --build

# Access the application
# Client: http://localhost
# API: http://localhost:8080
```

## 🏗️ Architecture

```
┌─────────────────┐         ┌──────────────────┐
│  Blazor WASM    │────────▶│   ASP.NET Core   │
│   (Client)      │         │   Web API        │
│                 │         │                  │
└─────────────────┘         └──────────────────┘
        │                            │
        └────────────────────────────┘
                     │
           ┌─────────┼─────────┐
           ▼         ▼         ▼
    ┌──────────┐ ┌─────────┐ ┌──────────────┐
    │Azure SQL │ │  Blob   │ │ Application  │
    │Database  │ │Storage  │ │  Insights    │
    └──────────┘ └─────────┘ └──────────────┘

Azure Container Apps Environment
├── campusconnect-client (Port 80)
└── campusconnect-api (Port 8080)
```

## 🐳 Docker Images

The application is available as two Docker images:
- `campusconnect-api`: ASP.NET Core Web API backend
- `campusconnect-client`: Blazor WebAssembly frontend (served via nginx)

## 🌐 Deployment

### Azure Container Apps (Current)
- Fully managed container hosting on Azure Container Apps
- Auto-scaling capabilities
- Built-in load balancing
- Integrated with Azure SQL Database
- Easy configuration via Azure Portal

### Development Workflow
1. **Local Development**: Docker Compose for local testing
2. **Container Registry**: Push images to Docker Hub or Azure Container Registry
3. **Deployment**: Deploy containers to Azure Container Apps environment

## 🔄 CI/CD Pipeline

The project uses Docker containers deployed to Azure Container Apps:

1. **Build Process**
   - Docker images built for API and Client
   - Images pushed to container registry (Docker Hub or ACR)
   
2. **Deployment Process**
   - Container Apps updated with new image versions
   - Rolling deployments with zero downtime
   - Environment variables configured in Azure Portal

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

**API Container (in Azure Container Apps):**
- `ASPNETCORE_ENVIRONMENT`: Environment name (Production)
- `ConnectionStrings__DefaultConnection`: SQL connection string
- `BlobStorage__ConnectionString`: Azure Blob Storage connection string
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Application Insights connection (optional)

**Client Container:**
- `API_BASE_URL`: Base URL for the API (internal container communication)

### Azure Container Apps Configuration
Configuration is managed through the Azure Portal:
- Environment variables set in Container App configuration
- Secrets managed securely in Azure
- Database connection strings stored as secrets

## 📊 Monitoring (Application Insights)

Azure Container Apps doesn't support automatic Application Insights integration. To add monitoring, you need to manually implement the Application Insights SDK:

### Step 1: Add NuGet Package
Add to your `api/api.csproj`:
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />
```

### Step 2: Update Program.cs
Add Application Insights to your API's `Program.cs`:
```csharp
// Add this after builder.Services.AddControllers()
builder.Services.AddApplicationInsightsTelemetry();
```

### Step 3: Configure in Azure Container Apps
1. Create an Application Insights resource in Azure Portal
2. Copy the connection string
3. Add environment variable in your Container App:
   - `APPLICATIONINSIGHTS_CONNECTION_STRING`: Your Application Insights connection string

### Step 4: Deploy Updated Container
After making code changes:
1. Build new Docker image
2. Push to your container registry
3. Update Container App with new image

### Monitoring Features You'll Get:
- Request/response tracking
- Performance metrics
- Exception logging
- Custom telemetry
- Live metrics dashboard

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
