# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY client/Client.csproj client/
RUN dotnet restore "client/Client.csproj"

# Copy everything else and build
COPY client/ client/
WORKDIR /src/client
RUN dotnet build "Client.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - use nginx to serve static files
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy published Blazor WASM files
COPY --from=publish /app/publish/wwwroot .

# Copy custom nginx configuration
COPY client/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
