# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY . .

# Restore dependencies
RUN dotnet restore

# Publish the API project (which will host the Blazor WebAssembly app)
RUN dotnet publish api/api.csproj -c Release -o /app/publish

# Publish the Blazor WebAssembly client
RUN dotnet publish client/Client.csproj -c Release -o /app/client-publish

# Copy the Blazor WebAssembly files to the API's wwwroot
RUN rm -rf /app/publish/wwwroot && \
    cp -r /app/client-publish/wwwroot /app/publish/

# Use the official .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose port 8080 (Azure App Service default)
EXPOSE 8080

# Set environment variables for Azure App Service
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

# Add a startup script to help with debugging
RUN echo '#!/bin/bash\necho "Starting CampusConnect application..."\necho "PORT: $PORT"\necho "ASPNETCORE_URLS: $ASPNETCORE_URLS"\ndotnet api.dll' > /app/start.sh && chmod +x /app/start.sh

# Run the application
ENTRYPOINT ["/app/start.sh"]