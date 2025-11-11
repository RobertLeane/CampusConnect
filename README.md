# CampusConnect

A modern campus club and event management platform built with Blazor WebAssembly and ASP.NET Core.

## Overview

Campus Connect encompasses 6 Blazor Razor pages: Login, Register, Home, Clubs, Events, and Dashboard. The application is containerised and deployed to Azure Container Apps using Docker and an Azure DevOps pipeline.

### Technology Stack

- **Frontend**: Blazor WebAssembly, HTML/CSS
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: Azure SQL Database
- **Storage**: Azure Blob Storage (for images)
- **Authentication**: Database-based (users table)
- **Deployment**: Docker container images on Azure Container Apps
- **Monitoring**: Application Insights
- **Testing**: bUnit framework and XPlat for code coverage

### Features

- **User Authentication**: Secure login and registration
- **Club Management**: Browse and join campus clubs
- **Event Management**: View and register for club events
- **Dashboard**: Personal overview of memberships and upcoming events

## Quick Start

### Using Docker Compose

```powershell
# Build and run locally
docker-compose up --build

# Access the application
# Client: http://localhost
# API: http://localhost:8080
```

## Architecture

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

## Docker Images

The application is available as two Docker images:
- `campusconnect-api`: ASP.NET Core Web API backend
- `campusconnect-client`: Blazor WebAssembly frontend (served via nginx)

## Deployment

### Azure Container Apps
- Fully managed container hosting on Azure Container Apps
- Auto-scaling capabilities
- Built-in load balancing
- Integrated with Azure SQL Database
- Easy configuration via Azure Portal

### Development Workflow
1. **Local Development**: Docker Compose for local testing
2. **Container Registry**: Push images to Docker Hub
3. **Deployment**: Deploy containers to Azure Container Apps environment

## CI/CD Pipeline

The project uses Docker containers deployed to Azure Container Apps:

1. **Build Process**
   - Docker images built for API and Client
   - Images pushed to container registry (Docker Hub or ACR)
   - Build, Test, Code Coverage, Deployment handled in Azure DevOps pipeline
   
2. **Deployment Process**
   - Container Apps updated with new image versions
   - Rolling deployments with zero downtime
   - Environment variables configured in Azure Portal for security

## Development

### Prerequisites

- .NET 8 SDK
- Azure CLI
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

## Configuration

### Environment Variables

**API Container (in Azure Container Apps):**
- `ASPNETCORE_ENVIRONMENT`: Environment name (Production)
- `ConnectionStrings__DefaultConnection`: SQL connection string
- `BlobStorage__ConnectionString`: Azure Blob Storage connection string
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: Application Insights connection

**Client Container:**
- `API_BASE_URL`: Base URL for the API (internal container communication)

### Azure Container Apps Configuration
Configuration is managed through the Azure Portal:
- Environment variables set in Container App configuration
- Secrets managed securely in Azure
- Database connection strings stored as secrets

## Monitoring (Application Insights)

Azure Container Apps doesn't support automatic Application Insights integration, so the Application Insights SDK was added to api/Program.cs and envirnoment variable `APPLICATIONINSIGHTS_CONNECTION_STRING` mapped to Application Insights resource connection string


### Monitoring Features with Application Insights:
- Request/response tracking
- Performance metrics
- Exception logging
- Custom telemetry
- Live metrics dashboard

## Links

- **Live Site**: [CampusConnectApp-Client](https://campusconnectapp-client.delightfulsky-c3ec110b.uksouth.azurecontainerapps.io)
- **Docker Hub**: [https://hub.docker.com](https://hub.docker.com/repositories/leanerob)
- **DevOps Project**: [dev.azure.com](https://dev.azure.com/3rdYearSoftwareTools/ClubsAndEventsPortal)
