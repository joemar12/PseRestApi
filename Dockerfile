# Multi-stage build for PseRestApi

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY ["src/PseRestApi.Host/PseRestApi.Host.csproj", "src/PseRestApi.Host/"]
COPY ["src/PseRestApi.Core/PseRestApi.Core.csproj", "src/PseRestApi.Core/"]
COPY ["src/PseRestApi.Infrastructure/PseRestApi.Infrastructure.csproj", "src/PseRestApi.Infrastructure/"]
COPY ["src/PseRestApi.Domain/PseRestApi.Domain.csproj", "src/PseRestApi.Domain/"]
COPY ["src/PseRestApi.Sync/PseRestApi.Sync.csproj", "src/PseRestApi.Sync/"]

# Restore dependencies
RUN dotnet restore "src/PseRestApi.Host/PseRestApi.Host.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "src/PseRestApi.Host/PseRestApi.Host.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "src/PseRestApi.Host/PseRestApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# # Create directory for SSL certificates
# RUN mkdir -p /etc/ssl/certs && chmod 755 /etc/ssl/certs

# # Create non-root user for security
# RUN useradd -m -u 1001 dotnetuser && chown -R dotnetuser /app /etc/ssl/certs
# USER dotnetuser

# Create non-root user for security
RUN useradd -m -u 1001 dotnetuser && chown -R dotnetuser /app
USER dotnetuser

# Expose HTTP and HTTPS ports
EXPOSE 8080 8443

# Health check (HTTP)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
	CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables for ASP.NET Core
# HTTPS configuration is set via docker-compose or kubernetes manifests
# ASPNETCORE_URLS can be overridden to include https://+:8443
# ASPNETCORE_Kestrel__Certificates__Default__Path should point to certificate file
# ASPNETCORE_Kestrel__Certificates__Default__Password should be provided securely
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "PseRestApi.Host.dll"]
