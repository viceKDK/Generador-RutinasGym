# Script simple para construir el instalador MSIX
# Epic 6 Story 6.2: Windows Installer & Desktop Integration

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "src\GymRoutineGenerator.UI.csproj"

Write-Host "Building Gym Routine Generator..." -ForegroundColor Green
Write-Host "Configuration: $Configuration | Platform: $Platform" -ForegroundColor Yellow
Write-Host ""

if (-not (Test-Path $ProjectFile)) {
    Write-Error "Project file not found: $ProjectFile"
    exit 1
}

try {
    # Step 1: Clean previous build
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    dotnet clean $ProjectFile -c $Configuration

    # Step 2: Restore packages
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore $ProjectFile

    # Step 3: Build application
    Write-Host "Building application..." -ForegroundColor Yellow
    dotnet build $ProjectFile -c $Configuration --no-restore

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit $LASTEXITCODE
    }

    # Step 4: Create MSIX package
    Write-Host "Creating MSIX package..." -ForegroundColor Yellow
    dotnet publish $ProjectFile -c $Configuration -p:GenerateAppxPackageOnBuild=true

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Package creation failed"
        exit $LASTEXITCODE
    }

    Write-Host ""
    Write-Host "Build completed successfully!" -ForegroundColor Green

    # Find and display the package location
    $PackageSearchPath = Join-Path $ProjectRoot "src\bin\$Configuration\net9.0-windows10.0.19041.0\*\AppPackages"
    $MSIXFiles = Get-ChildItem -Path $PackageSearchPath -Filter "*.msix" -Recurse -ErrorAction SilentlyContinue

    if ($MSIXFiles.Count -gt 0) {
        Write-Host "Package created at: $($MSIXFiles[0].FullName)" -ForegroundColor Cyan
    }

} catch {
    Write-Error "Error during build: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "=== Build Process Completed ===" -ForegroundColor Green