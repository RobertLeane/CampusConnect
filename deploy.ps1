# CampusConnect Deployment Script
# This script helps you deploy the CampusConnect application to Azure

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('local', 'dockerhub', 'azure-appservice', 'azure-containerapp', 'azure-bicep')]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [string]$DockerHubUsername = "",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "campusconnect-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "uksouth",
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = "prod"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n✨ $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Test-DockerInstalled {
    try {
        docker --version | Out-Null
        return $true
    } catch {
        return $false
    }
}

function Test-AzureCliInstalled {
    try {
        az --version | Out-Null
        return $true
    } catch {
        return $false
    }
}

# Main script logic
switch ($Action) {
    'local' {
        Write-Step "Building and running locally with Docker Compose..."
        
        if (-not (Test-DockerInstalled)) {
            Write-Error "Docker is not installed. Please install Docker Desktop."
            exit 1
        }
        
        Write-Step "Stopping any existing containers..."
        docker-compose down
        
        Write-Step "Building and starting containers..."
        docker-compose up --build -d
        
        Write-Success "Application is running!"
        Write-Host "Client: http://localhost" -ForegroundColor Yellow
        Write-Host "API: http://localhost:8080" -ForegroundColor Yellow
        Write-Host "`nView logs with: docker-compose logs -f" -ForegroundColor Gray
        Write-Host "Stop with: docker-compose down" -ForegroundColor Gray
    }
    
    'dockerhub' {
        Write-Step "Building and pushing images to Docker Hub..."
        
        if (-not (Test-DockerInstalled)) {
            Write-Error "Docker is not installed. Please install Docker Desktop."
            exit 1
        }
        
        if ([string]::IsNullOrEmpty($DockerHubUsername)) {
            Write-Error "Docker Hub username is required. Use -DockerHubUsername parameter."
            exit 1
        }
        
        Write-Step "Logging in to Docker Hub..."
        docker login
        
        Write-Step "Building API image..."
        docker build -t "${DockerHubUsername}/campusconnect-api:latest" -f api/Dockerfile .
        
        Write-Step "Building Client image..."
        docker build -t "${DockerHubUsername}/campusconnect-client:latest" -f client/Dockerfile .
        
        Write-Step "Pushing API image..."
        docker push "${DockerHubUsername}/campusconnect-api:latest"
        
        Write-Step "Pushing Client image..."
        docker push "${DockerHubUsername}/campusconnect-client:latest"
        
        Write-Success "Images pushed to Docker Hub successfully!"
        Write-Host "API Image: ${DockerHubUsername}/campusconnect-api:latest" -ForegroundColor Yellow
        Write-Host "Client Image: ${DockerHubUsername}/campusconnect-client:latest" -ForegroundColor Yellow
    }
    
    'azure-appservice' {
        Write-Step "Deploying to Azure App Service..."
        
        if (-not (Test-AzureCliInstalled)) {
            Write-Error "Azure CLI is not installed. Please install Azure CLI."
            exit 1
        }
        
        if ([string]::IsNullOrEmpty($DockerHubUsername)) {
            Write-Error "Docker Hub username is required. Use -DockerHubUsername parameter."
            exit 1
        }
        
        Write-Step "Creating resource group..."
        az group create --name $ResourceGroup --location $Location
        
        $appServicePlan = "$ResourceGroup-plan"
        $apiAppName = "campusconnect-api-$Environment"
        $clientAppName = "campusconnect-client-$Environment"
        
        Write-Step "Creating App Service Plan..."
        az appservice plan create `
            --name $appServicePlan `
            --resource-group $ResourceGroup `
            --location $Location `
            --is-linux `
            --sku B1
        
        Write-Step "Creating API Web App..."
        az webapp create `
            --name $apiAppName `
            --resource-group $ResourceGroup `
            --plan $appServicePlan `
            --deployment-container-image-name "${DockerHubUsername}/campusconnect-api:latest"
        
        Write-Step "Configuring API settings..."
        az webapp config appsettings set `
            --name $apiAppName `
            --resource-group $ResourceGroup `
            --settings WEBSITES_PORT=8080 ASPNETCORE_ENVIRONMENT=Production
        
        Write-Step "Creating Client Web App..."
        az webapp create `
            --name $clientAppName `
            --resource-group $ResourceGroup `
            --plan $appServicePlan `
            --deployment-container-image-name "${DockerHubUsername}/campusconnect-client:latest"
        
        Write-Step "Enabling continuous deployment..."
        az webapp deployment container config `
            --name $apiAppName `
            --resource-group $ResourceGroup `
            --enable-cd true
        
        az webapp deployment container config `
            --name $clientAppName `
            --resource-group $ResourceGroup `
            --enable-cd true
        
        $apiUrl = "https://${apiAppName}.azurewebsites.net"
        $clientUrl = "https://${clientAppName}.azurewebsites.net"
        
        Write-Success "Deployment complete!"
        Write-Host "API URL: $apiUrl" -ForegroundColor Yellow
        Write-Host "Client URL: $clientUrl" -ForegroundColor Yellow
    }
    
    'azure-containerapp' {
        Write-Step "Deploying to Azure Container Apps..."
        
        if (-not (Test-AzureCliInstalled)) {
            Write-Error "Azure CLI is not installed. Please install Azure CLI."
            exit 1
        }
        
        if ([string]::IsNullOrEmpty($DockerHubUsername)) {
            Write-Error "Docker Hub username is required. Use -DockerHubUsername parameter."
            exit 1
        }
        
        Write-Step "Installing Container Apps extension..."
        az extension add --name containerapp --upgrade
        
        Write-Step "Creating resource group..."
        az group create --name $ResourceGroup --location $Location
        
        $containerAppEnv = "$ResourceGroup-env"
        $apiAppName = "campusconnect-api-$Environment"
        $clientAppName = "campusconnect-client-$Environment"
        
        Write-Step "Creating Container Apps environment..."
        az containerapp env create `
            --name $containerAppEnv `
            --resource-group $ResourceGroup `
            --location $Location
        
        Write-Step "Deploying API Container App..."
        az containerapp create `
            --name $apiAppName `
            --resource-group $ResourceGroup `
            --environment $containerAppEnv `
            --image "${DockerHubUsername}/campusconnect-api:latest" `
            --target-port 8080 `
            --ingress external `
            --env-vars ASPNETCORE_ENVIRONMENT=Production `
            --cpu 0.5 --memory 1.0Gi
        
        Write-Step "Deploying Client Container App..."
        az containerapp create `
            --name $clientAppName `
            --resource-group $ResourceGroup `
            --environment $containerAppEnv `
            --image "${DockerHubUsername}/campusconnect-client:latest" `
            --target-port 80 `
            --ingress external `
            --cpu 0.25 --memory 0.5Gi
        
        $apiUrl = az containerapp show `
            --name $apiAppName `
            --resource-group $ResourceGroup `
            --query properties.configuration.ingress.fqdn -o tsv
        
        $clientUrl = az containerapp show `
            --name $clientAppName `
            --resource-group $ResourceGroup `
            --query properties.configuration.ingress.fqdn -o tsv
        
        Write-Success "Deployment complete!"
        Write-Host "API URL: https://$apiUrl" -ForegroundColor Yellow
        Write-Host "Client URL: https://$clientUrl" -ForegroundColor Yellow
    }
    
    'azure-bicep' {
        Write-Step "Deploying to Azure using Bicep..."
        
        if (-not (Test-AzureCliInstalled)) {
            Write-Error "Azure CLI is not installed. Please install Azure CLI."
            exit 1
        }
        
        if ([string]::IsNullOrEmpty($DockerHubUsername)) {
            Write-Error "Docker Hub username is required. Use -DockerHubUsername parameter."
            exit 1
        }
        
        Write-Step "Creating resource group..."
        az group create --name $ResourceGroup --location $Location
        
        Write-Host "`nPlease provide the following secure parameters:" -ForegroundColor Yellow
        $sqlConnectionString = Read-Host "SQL Connection String" -AsSecureString
        $sqlAdminLogin = Read-Host "SQL Admin Login"
        $sqlAdminPassword = Read-Host "SQL Admin Password" -AsSecureString
        
        # Convert secure strings
        $sqlConnStr = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
            [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlConnectionString))
        $sqlAdminPwd = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
            [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlAdminPassword))
        
        Write-Step "Validating deployment..."
        az deployment group what-if `
            --resource-group $ResourceGroup `
            --template-file infra/main.bicep `
            --parameters `
                location=$Location `
                dockerHubUsername=$DockerHubUsername `
                sqlConnectionString=$sqlConnStr `
                sqlAdminLogin=$sqlAdminLogin `
                sqlAdminPassword=$sqlAdminPwd `
                environment=$Environment
        
        $confirm = Read-Host "`nDo you want to proceed with the deployment? (yes/no)"
        if ($confirm -ne "yes") {
            Write-Host "Deployment cancelled." -ForegroundColor Yellow
            exit 0
        }
        
        Write-Step "Deploying infrastructure..."
        az deployment group create `
            --resource-group $ResourceGroup `
            --template-file infra/main.bicep `
            --parameters `
                location=$Location `
                dockerHubUsername=$DockerHubUsername `
                sqlConnectionString=$sqlConnStr `
                sqlAdminLogin=$sqlAdminLogin `
                sqlAdminPassword=$sqlAdminPwd `
                environment=$Environment
        
        Write-Success "Deployment complete! Check Azure Portal for deployment details."
    }
}

Write-Host "`n🎉 Done!" -ForegroundColor Green
