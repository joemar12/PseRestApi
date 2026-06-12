# Docker Secrets Configuration for PseRestApi

## Overview

This guide explains how to configure PseRestApi to use Docker secrets for secure connection string management when using an external database container.

## What are Docker Secrets?

Docker secrets are encrypted credentials stored in Docker Swarm that are securely mounted into containers at runtime. This is more secure than passing connection strings as environment variables.

**Security Benefits:**
- ✅ Secrets are encrypted at rest in Swarm
- ✅ Only transmitted via encrypted channels
- ✅ Mounted read-only in containers
- ✅ Not visible in `docker inspect` output
- ✅ Auditable access logs
- ✅ No secrets in environment variables

---

## Quick Setup (3 Steps)

### Step 1: Create Secret File

```bash
# Create secrets directory
mkdir -p secrets

# Copy example
cp secrets/db_connection_string.txt.example secrets/db_connection_string.txt

# Edit with your connection string
nano secrets/db_connection_string.txt
```

**Example connection string for external database:**
```
Server=your-db-host,1433;Database=PseRestApiDb;User Id=sa;Password=YourPassword!;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

### Step 2: Start Services

```bash
# For development (docker-compose with file-based secrets)
docker-compose up --build -d

# For production (docker-compose.prod.yml)
docker-compose -f docker-compose.prod.yml up -d
```

### Step 3: Verify

```bash
# Check if API can connect to database
curl http://localhost:5000/health

# View logs if connection fails
docker-compose logs pserestapi
```

---

## Configuration Methods

### Method 1: Docker Compose (Development)

**File-based secrets** - Recommended for local development and single-host deployments.

**Setup:**
1. Create `secrets/db_connection_string.txt` with your connection string
2. Run: `docker-compose up -d`

**How it works:**
- docker-compose mounts the file as a secret
- Application reads from `/run/secrets/db_connection_string`
- Secrets directory is mounted as volume

**Files:**
- `docker-compose.yml` - Uses file-based secrets
- `secrets/db_connection_string.txt` - Your connection string (git-ignored)

---

### Method 2: Docker Swarm (Production)

**Docker Swarm secrets** - Recommended for production with encrypted storage.

**Prerequisites:**
```bash
# Initialize Docker Swarm (run once on manager node)
docker swarm init

# Or join existing swarm
docker swarm join --token SWMTKN-1-... manager-ip:2377
```

**Setup:**

**Option A: Using script**
```bash
chmod +x manage-secrets.sh

# Interactive setup
./manage-secrets.sh create

# Or from file
./manage-secrets.sh create-file secrets.txt

# List secrets
./manage-secrets.sh list

# Remove secrets
./manage-secrets.sh remove
```

**Option B: Manual setup**
```bash
# Create secret from file
docker secret create pserestapi_db_connection - < secrets/db_connection_string.txt

# Create secret from stdin
echo "Server=...;Password=..." | docker secret create pserestapi_db_connection -

# Create from environment variable
echo "$DB_CONNECTION_STRING" | docker secret create pserestapi_db_connection -
```

**Deploy services:**
```bash
# Use docker-compose.prod.yml (configured for secrets)
docker-compose -f docker-compose.prod.yml up -d

# Or use docker service commands
docker service create \
  --name pserestapi \
  --secret pserestapi_db_connection \
  --env ConnectionStrings__DefaultConnectionString=/run/secrets/pserestapi_db_connection \
  pserestapi:latest
```

**Verify:**
```bash
# List secrets
docker secret ls

# Inspect secret (shows metadata, not content)
docker secret inspect pserestapi_db_connection

# View service using secret
docker service ls
docker service inspect pserestapi
```

---

### Method 3: Kubernetes (Enterprise)

**Kubernetes secrets** - Recommended for Kubernetes clusters.

**Create secret:**
```bash
# From file
kubectl create secret generic pserestapi-db-connection \
  --from-file=connection-string=secrets/db_connection_string.txt

# From literal
kubectl create secret generic pserestapi-db-connection \
  --from-literal=connection-string='Server=...;Password=...'
```

**Mount in deployment:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pserestapi
spec:
  template:
	spec:
	  containers:
	  - name: api
		image: pserestapi:latest
		env:
		- name: ConnectionStrings__DefaultConnectionString
		  valueFrom:
			secretKeyRef:
			  name: pserestapi-db-connection
			  key: connection-string
		volumeMounts:
		- name: connection-secret
		  mountPath: /run/secrets
		  readOnly: true
	  volumes:
	  - name: connection-secret
		secret:
		  secretName: pserestapi-db-connection
```

---

## How Application Reads Secrets

The application code reads the secret from `/run/secrets/db_connection_string`:

**In Program.cs:**
```csharp
// Load connection string from Docker secret if available
var connectionStringPath = builder.Configuration["ConnectionStrings:DefaultConnectionString"];
if (!string.IsNullOrEmpty(connectionStringPath) && connectionStringPath.StartsWith("/run/secrets/"))
{
	// Read connection string from secret file
	var secretContent = File.ReadAllText(connectionStringPath).Trim();
	builder.Configuration["ConnectionStrings:DefaultConnectionString"] = secretContent;
}
```

**How it works:**
1. docker-compose.yml specifies: `ConnectionStrings__DefaultConnectionString: /run/secrets/db_connection_string`
2. This tells the app where the secret is mounted
3. Application reads the actual connection string from the file
4. Connection string is used normally by Entity Framework Core

---

## Connection String Examples

### SQL Server (On-Premises)
```
Server=db.example.com,1433;Database=PseRestApiDb;User Id=sa;Password=YourPassword!;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;
```

### SQL Server (Docker Host - External)
```
Server=192.168.1.100,1433;Database=PseRestApiDb;User Id=sa;Password=YourPassword!;Encrypt=true;TrustServerCertificate=false;
```

### Azure SQL Database
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=PseRestApiDb;Persist Security Info=False;User ID=username@your-server;Password=YourPassword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### Amazon RDS SQL Server
```
Server=pserestapi.cxxxxxxx.us-east-1.rds.amazonaws.com,1433;Database=PseRestApiDb;User Id=admin;Password=YourPassword!;Encrypt=true;
```

### Google Cloud SQL
```
Server=cloudsql-instance:PseRestApiDb;Database=PseRestApiDb;User Id=cloudsql-user;Password=YourPassword!;
```

---

## File Structure

```
project-root/
├── docker-compose.yml              # Development (file-based secrets)
├── docker-compose.prod.yml         # Production (Swarm secrets)
├── secrets/
│   ├── db_connection_string.txt          # Your actual connection string
│   └── db_connection_string.txt.example  # Example template
├── manage-secrets.sh               # Secret management script
└── src/
	├── PseRestApi.Host/
	│   └── Program.cs              # Reads secret from /run/secrets/
	└── PseRestApi.Sync/
		└── Program.cs              # Reads secret from /run/secrets/
```

**Important:**
- `.gitignore` should contain: `secrets/db_connection_string.txt`
- Never commit actual connection strings
- Only commit `.example` files

---

## Common Tasks

### Update Connection String

**Docker Compose:**
```bash
# Edit the secret file
nano secrets/db_connection_string.txt

# Restart services to reload
docker-compose restart pserestapi pserestapi-sync
```

**Docker Swarm:**
```bash
# Remove old secret
docker secret rm pserestapi_db_connection

# Create new secret
echo "New connection string" | docker secret create pserestapi_db_connection -

# Update service (requires restart)
docker service update --secret-rm pserestapi_db_connection \
					  --secret-add pserestapi_db_connection \
					  pserestapi
```

### Verify Secret is Mounted

```bash
# Check if mounted in container
docker-compose exec pserestapi ls -la /run/secrets/

# Read secret from container
docker-compose exec pserestapi cat /run/secrets/db_connection_string
```

### Debug Connection Issues

```bash
# View application logs
docker-compose logs -f pserestapi

# Check if secret file exists and is readable
docker-compose exec pserestapi test -f /run/secrets/db_connection_string && echo "Secret exists" || echo "Secret missing"

# Check connection string was loaded
docker-compose exec pserestapi dotnet user-secrets list  # May not show if from secret file

# Test database connection
docker-compose exec pserestapi dotnet ef database validate
```

---

## Security Best Practices

### ✅ DO
- ✅ Use Docker secrets in production (not environment variables)
- ✅ Encrypt connection strings (use SQL Server encryption)
- ✅ Rotate secrets periodically
- ✅ Audit secret access
- ✅ Use strong database passwords (min 12 characters, complex)
- ✅ Use TLS for database connections (Encrypt=true)
- ✅ Keep secrets directory in .gitignore
- ✅ Backup secrets securely
- ✅ Use separate credentials for different environments

### ❌ DON'T
- ❌ Hardcode connection strings in code
- ❌ Pass secrets in environment variables (production)
- ❌ Commit secrets to version control
- ❌ Log connection strings
- ❌ Use weak database passwords
- ❌ Share credentials across environments
- ❌ Leave default credentials unchanged
- ❌ Transmit secrets unencrypted

---

## Troubleshooting

### Issue: "Cannot find file `/run/secrets/db_connection_string`"

**Causes:**
- Secret not configured in docker-compose.yml
- Secrets directory doesn't exist or wrong file name
- Permission denied

**Solution:**
```bash
# Verify secrets section in docker-compose.yml
grep -A 2 "secrets:" docker-compose.yml

# Verify file exists
ls -la secrets/db_connection_string.txt

# Check file permissions
chmod 600 secrets/db_connection_string.txt

# Verify it's mounted in container
docker-compose exec pserestapi ls -la /run/secrets/
```

### Issue: "Connection string is empty or null"

**Causes:**
- Secret file is empty
- File has wrong encoding
- Path wrong

**Solution:**
```bash
# Check file content
cat secrets/db_connection_string.txt

# Ensure no extra whitespace
echo -n "YOUR_CONNECTION_STRING" > secrets/db_connection_string.txt

# Restart container
docker-compose restart pserestapi
```

### Issue: Database connection fails with "Cannot connect"

**Causes:**
- Wrong host/port in connection string
- Database not accessible from container
- Firewall blocking connection
- Wrong credentials

**Solution:**
```bash
# Verify connection string
docker-compose exec pserestapi cat /run/secrets/db_connection_string

# Test connection from container
docker-compose exec pserestapi sqlcmd -S your-server,1433 -U username -P password -Q "SELECT 1"

# Check network connectivity
docker-compose exec pserestapi ping your-db-host

# Check if port is open
docker-compose exec pserestapi telnet your-db-host 1433
```

### Issue: "Secret not found" in Docker Swarm

**Causes:**
- Secret doesn't exist
- Wrong secret name
- Service can't access secret

**Solution:**
```bash
# List all secrets
docker secret ls

# Check secret name matches service config
docker secret inspect pserestapi_db_connection

# Recreate if needed
docker secret rm pserestapi_db_connection 2>/dev/null || true
echo "Your connection string" | docker secret create pserestapi_db_connection -
```

---

## Advanced: Using manage-secrets.sh Script

The `manage-secrets.sh` script automates secret management:

```bash
# Make executable
chmod +x manage-secrets.sh

# Interactive creation (prompts for connection string)
./manage-secrets.sh create

# Create from file (key=value format)
./manage-secrets.sh create-file secrets.txt

# List existing secrets
./manage-secrets.sh list

# Remove all secrets
./manage-secrets.sh remove

# Get help
./manage-secrets.sh help
```

**Example secrets.txt format:**
```
DB_CONNECTION_STRING=Server=...;Password=...
API_KEY=your-api-key
```

---

## Integration with CI/CD

### GitHub Actions Example
```yaml
- name: Create Docker secret
  run: |
	mkdir -p secrets
	echo "${{ secrets.DB_CONNECTION_STRING }}" > secrets/db_connection_string.txt

- name: Deploy with docker-compose
  run: docker-compose -f docker-compose.prod.yml up -d
```

### GitLab CI Example
```yaml
deploy:
  script:
	- mkdir -p secrets
	- echo "$DB_CONNECTION_STRING" > secrets/db_connection_string.txt
	- docker-compose -f docker-compose.prod.yml up -d
  environment:
	variables:
	  DB_CONNECTION_STRING: $DB_CONNECTION_STRING
```

---

## Summary

| Method | Use Case | Security | Complexity |
|--------|----------|----------|-----------|
| Environment Variables | Development only | ❌ Low | ✅ Simple |
| File-based Secrets (docker-compose) | Single host | ⚠️ Medium | ✅ Simple |
| Docker Swarm Secrets | Small production | ✅ High | ⚠️ Medium |
| Kubernetes Secrets | Enterprise | ✅ High | ⚠️ Medium |
| Vault / AWS Secrets Manager | Large scale | ✅✅ High | ❌ Complex |

---

## Related Files

- **docker-compose.yml** – Development setup with secrets
- **docker-compose.prod.yml** – Production setup with secrets
- **manage-secrets.sh** – Secret management script
- **secrets/db_connection_string.txt.example** – Connection string template
- **src/PseRestApi.Host/Program.cs** – Secret loading logic
- **src/PseRestApi.Sync/Program.cs** – Secret loading logic

---

## Next Steps

1. ✅ Create `secrets/db_connection_string.txt` with your connection string
2. ✅ Run `docker-compose up --build -d`
3. ✅ Verify with `curl http://localhost:5000/health`
4. 🔐 For production, use Docker Swarm or Kubernetes secrets
5. 📊 Set up monitoring and logging

---

For more information, see:
- [Docker Secrets Documentation](https://docs.docker.com/engine/swarm/secrets/)
- [Docker Swarm Mode](https://docs.docker.com/engine/swarm/)
- [Kubernetes Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
