# PseRestApi Containerization Guide

## Overview

This guide walks you through containerizing the **PseRestApi** application using Docker. The solution includes:

- **Dockerfile** – Multi-stage build for the ASP.NET Core API
- **Dockerfile.Sync** – Separate image for the background sync service (optional)
- **docker-compose.yml** – Orchestrates the API, Sync service, and SQL Server database
- **.dockerignore** – Optimizes build context
- **.env.example** – Environment configuration template

---

## Prerequisites

1. **Docker** and **Docker Compose** installed
   - [Docker Desktop](https://www.docker.com/products/docker-desktop) (includes Docker Compose)
   - Or install separately: [Docker Engine](https://docs.docker.com/engine/) + [Docker Compose](https://docs.docker.com/compose/)

2. **Git** (to clone the repository)

3. **.NET 10 SDK** (optional, only needed if building locally without Docker)

---

## Quick Start

### 1. Build and Run with Docker Compose

```bash
# Clone the repository (if not already done)
git clone https://github.com/joemar12/PseRestApi.git
cd PseRestApi

# Copy environment template (optional but recommended)
cp .env.example .env

# Build images and start services
docker-compose up --build -d

# View logs
docker-compose logs -f pserestapi
```

### 2. Verify Services

```bash
# Check running containers
docker-compose ps

# Test API health
curl http://localhost:5000/health

# View Swagger UI
# Open browser: http://localhost:5000/swagger/index.html
```

### 3. Stop Services

```bash
docker-compose down

# Stop and remove volumes (cleans up database)
docker-compose down -v
```

---

## Architecture

### Multi-Stage Build Process

The Dockerfile uses three stages to minimize image size:

```
Stage 1: Build
  ├─ FROM mcr.microsoft.com/dotnet/sdk:10.0
  ├─ Copy project files
  ├─ dotnet restore
  └─ dotnet build

Stage 2: Publish
  ├─ dotnet publish (creates optimized release output)
  └─ Output: /app/publish

Stage 3: Runtime
  ├─ FROM mcr.microsoft.com/dotnet/aspnet:10.0
  ├─ Copy published application
  ├─ Create non-root user (security)
  ├─ Expose port 8080
  ├─ HEALTHCHECK configuration
  └─ Start application
```

**Benefits:**
- **Final image** contains only runtime, not SDK (~200MB vs ~800MB)
- **Security** – runs as non-root user (dotnetuser)
- **Health checks** – automatic monitoring and restart

---

## Services in docker-compose.yml

### 1. **pserestapi** (ASP.NET Core API)
- **Build:** Dockerfile in current directory
- **Port:** 5000 (HTTP), 5001 (HTTPS)
- **Environment Variables:**
  - Connection string points to `sqlserver` service
  - PseApi configuration (FramesUrl, Referer)
  - Rate limiting settings
- **Dependencies:** Waits for SQL Server to be healthy
- **Health Check:** Calls /health endpoint

### 3. **pserestapi-sync** (Background Sync Service) – Optional
- **Build:** Dockerfile.Sync
- **Runs:** Once at startup (pulls stock data from PSE)
- **Dependencies:** Waits for both database and API
- **Auto-restart:** unless explicitly stopped

---

## Configuration

### Environment Variables

Edit the environment variables in `docker-compose.yml` or create a `.env` file:

```bash
# Copy the example
cp .env.example .env

# Edit .env with your values
nano .env
```

Key variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Development` or `Production` |
| `ConnectionString` | (in yml) | Database connection string |
| `PseApiConfig__FramesUrl` | https://frames.pse.com.ph/ | PSE API frames URL |
| `PseApiConfig__Referer` | https://www1.pse.com.ph/... | HTTP referer header |
| `RateLimitConfig__PermitLimit` | 10 | Max requests per window |
| `RateLimitConfig__WindowInMinutes` | 1 | Rate limit window duration |

### Database Connection String

The connection string is formatted for Docker:
```
Server=sqlserver,1433;Database=PseRestApiDb;User Id=sa;Password=P@ssw0rd123!;Encrypt=false;TrustServerCertificate=true;
```

- `sqlserver` – service name (Docker DNS resolves this)
- `1433` – SQL Server port (internal, no mapping needed)
- `Encrypt=false; TrustServerCertificate=true;` – allows self-signed certificates in containers

---

## Common Tasks

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f pserestapi

# Last 100 lines
docker-compose logs --tail 100 pserestapi
```

### Execute Commands Inside Container

```bash
# Run bash/shell
docker-compose exec pserestapi bash

# Run dotnet CLI
docker-compose exec pserestapi dotnet --version

# Access SQL Server
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P P@ssw0rd123!
```

### Database Initialization

Migrations are applied via `AppDbInitializer` in the application startup:

```bash
# To reset database (remove all data):
docker-compose down -v
docker-compose up --build -d

# Or manually execute migrations:
docker-compose exec pserestapi dotnet ef database update
```

### Build Specific Image

```bash
# Build only API image
docker build -f Dockerfile -t pserestapi:latest .

# Build only Sync image
docker build -f Dockerfile.Sync -t pserestapi-sync:latest .

# With build arguments (for multi-environment)
docker build --build-arg ENV=Production -f Dockerfile -t pserestapi:prod .
```

### Run Container Standalone (without compose)

```bash
# Run API only (requires pre-existing database)
docker run -d \
  -p 5000:8080 \
  -e ConnectionStrings__DefaultConnectionString="Server=your-db-host;..." \
  -e ASPNETCORE_ENVIRONMENT=Production \
  pserestapi:latest

# Run with database link (if SQL Server in another container)
docker run -d \
  -p 5000:8080 \
  --link sqlserver:sqlserver \
  -e ConnectionStrings__DefaultConnectionString="Server=sqlserver,1433;..." \
  pserestapi:latest
```

---

## Security Best Practices

✅ **Implemented in Dockerfile:**
- Non-root user (`dotnetuser`)
- Health checks (automatic restart on failure)
- Minimal runtime image (no SDK)
- .dockerignore to exclude unnecessary files

🔒 **Recommended Additions (not yet implemented):**

1. **Use secrets for sensitive data** (Docker Secrets or Kubernetes)
   ```bash
   echo "your-password" | docker secret create db_password -
   ```

2. **Network isolation** – Use named networks (already in docker-compose.yml)

3. **Resource limits**
   ```yaml
   services:
	 pserestapi:
	   deploy:
		 resources:
		   limits:
			 cpus: '1.0'
			 memory: 512M
   ```

4. **Read-only file system** (for hardening)
   ```yaml
   pserestapi:
	 read_only: true
	 tmpfs: /tmp
   ```

5. **Use environment-specific .env files**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up
   ```

---

## Deployment Scenarios

### Local Development

```bash
docker-compose up -d
# API at http://localhost:5000
# Swagger at http://localhost:5000/swagger
```

### Staging/Production on Single Host

```bash
# With .env for production
docker-compose -f docker-compose.yml up -d

# Enable auto-restart
docker update --restart unless-stopped pse_api
docker update --restart unless-stopped pse_sqlserver
```

### Kubernetes Deployment

1. **Generate YAML from compose** (convert with Kompose)
   ```bash
   kompose convert -f docker-compose.yml
   ```

2. **Or create native Kubernetes manifests** (Deployment, Service, StatefulSet for DB)

3. **Benefits on K8s:**
   - Auto-scaling
   - Rolling updates
   - Self-healing
   - Persistent volumes for database

---

## Troubleshooting

### Container exits immediately

```bash
docker-compose logs pserestapi
# Check for errors related to database connection or configuration
```

**Solution:** Verify SQL Server is healthy first:
```bash
docker-compose ps
# If sqlserver shows "unhealthy", wait and retry
```

### Port already in use

```bash
# Change port in docker-compose.yml or use environment:
docker-compose -e "API_PORT=5005" up
```

Or modify `.env`:
```
API_PORT=5005
```

### Database connection refused

**Symptoms:** `SqlConnection Timeout` or `Server=sqlserver not found`

**Solution:**
1. Ensure `sqlserver` service is running and healthy
2. Check connection string uses `sqlserver` (not `localhost`)
3. Verify network connectivity
   ```bash
   docker-compose exec pserestapi ping sqlserver
   ```

### Build fails with "project not found"

Ensure you're in the repository root and all project files are present:
```bash
ls -la src/*/
```

---

## Performance Optimization

### Layer Caching

The Dockerfile caches layers by copying .csproj files first (rarely change) before source code:

```dockerfile
COPY ["src/PseRestApi.Host/PseRestApi.Host.csproj", "src/PseRestApi.Host/"]
RUN dotnet restore ...
COPY . .  # Only re-run if source changes
```

### Build Time Tips

1. **Use BuildKit** (faster, parallel layers)
   ```bash
   DOCKER_BUILDKIT=1 docker build .
   ```

2. **Skip sync service** if not needed
   ```bash
   docker-compose up -d --no-build pserestapi sqlserver
   ```

3. **Pre-pull base images**
   ```bash
   docker pull mcr.microsoft.com/dotnet/sdk:10.0
   docker pull mcr.microsoft.com/dotnet/aspnet:10.0
   ```

---

## Image Size Optimization

**Current size:** ~200MB for API image (after publish)

**Further reductions:**
- Use `Alpine` runtime images (~50MB) – but may require dependencies
- Trim unused files in publish profile (`.csproj` setting)
- Use `.NET AOT` compilation (future enhancement)

---

## Next Steps

1. ✅ **Test locally** with `docker-compose up`
2. ✅ **Verify API** at http://localhost:5000/swagger
3. ✅ **Test database** connectivity
4. 📦 **Push images** to registry (Docker Hub, Azure Container Registry, etc.)
   ```bash
   docker tag pserestapi:latest yourusername/pserestapi:latest
   docker push yourusername/pserestapi:latest
   ```
5. 🚀 **Deploy** to production environment (Docker host, Kubernetes, Azure Container Instances, etc.)

---

## Additional Resources

- [Docker Official Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Microsoft .NET in Container Guide](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [SQL Server on Linux Container](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker)
- [Kubernetes for .NET Developers](https://learn.microsoft.com/en-us/dotnet/architecture/container-kubernetes/)

---

## Questions?

For more details, refer to:
- `Dockerfile` – ASP.NET Core API image definition
- `Dockerfile.Sync` – Background sync service image
- `docker-compose.yml` – Service orchestration and networking
- `.env.example` – Configuration template
