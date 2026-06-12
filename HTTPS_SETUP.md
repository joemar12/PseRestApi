# HTTPS Setup Guide for PseRestApi

## Overview

This guide explains how to add HTTPS support to both development and production builds of PseRestApi using Docker.

## Quick Start

### Development (Docker Compose)

```bash
# 1. Generate self-signed certificate
bash generate-dev-cert.sh

# 2. Start services with HTTPS
docker-compose up --build -d

# 3. Access via HTTPS
curl --insecure https://localhost:5001/health
```

### Production (Docker Swarm/Kubernetes)

```bash
# 1. Obtain SSL certificate (Let's Encrypt, etc.)
# 2. Create Docker secret from certificate
docker secret create pserestapi_ssl_cert - < server.pfx

# 3. Deploy with certificate secret
docker-compose -f docker-compose.prod.yml up -d
```

---

## Architecture

### Development Setup
- **Self-signed certificates** stored in `certs/` directory
- **Docker Compose** mounts certificates as volumes
- **ASP.NET Core** uses Kestrel with HTTPS
- **HTTP→HTTPS redirect** enabled

### Production Setup
- **SSL certificates** from Let's Encrypt or your CA
- **Docker secrets** store certificates securely
- **ASP.NET Core** Kestrel uses secrets
- **Load balancer** (reverse proxy) recommended for TLS termination

---

## Environment Variables

### Development
```yaml
ASPNETCORE_ENVIRONMENT: Development
ASPNETCORE_URLS: http://+:8080;https://+:8443
ASPNETCORE_Kestrel__Certificates__Default__Path: /etc/ssl/certs/certificate.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password: dev-password
```

### Production
```yaml
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_URLS: http://+:8080;https://+:8443
ASPNETCORE_Kestrel__Certificates__Default__Path: /run/secrets/ssl_cert
ASPNETCORE_Kestrel__Certificates__Default__Password: ${SSL_CERT_PASSWORD}
```

---

## Certificate Formats

### Development (Self-Signed)
- Format: PFX (PKCS#12)
- Generated: `generate-dev-cert.sh`
- Password: `dev-password` (default)
- Validity: 365 days

### Production
- Format: PFX (preferred) or PEM
- Source: Let's Encrypt, DigiCert, etc.
- Password: From your certificate provider
- Validity: Usually 1-2 years

---

## File Structure

```
PseRestApi/
├── certs/                          (NEW)
│   ├── certificate.pfx             (dev certificate, git-ignored)
│   ├── certificate.crt             (dev public cert)
│   └── certificate.key             (dev private key)
│
├── generate-dev-cert.sh            (NEW)
├── HTTPS_SETUP.md                  (NEW)
│
├── docker-compose.yml              (UPDATED - HTTPS support)
├── docker-compose.prod.yml         (UPDATED - HTTPS support)
├── Dockerfile                       (UPDATED - cert mounting)
├── Dockerfile.Sync                 (updated if needed)
│
└── src/PseRestApi.Host/
	└── Program.cs                  (no changes needed)
```

---

## Step-by-Step Setup

### Step 1: Generate Development Certificate

```bash
# Make script executable
chmod +x generate-dev-cert.sh

# Generate self-signed certificate
bash generate-dev-cert.sh
```

This creates:
- `certs/certificate.pfx` – Private key + certificate (for Kestrel)
- `certs/certificate.crt` – Public certificate
- `certs/certificate.key` – Private key

### Step 2: Update docker-compose.yml

Certificate is mounted as volume and environment variable set to point to it.

### Step 3: Update Dockerfile

Dockerfile copies certificates into container.

### Step 4: Verify HTTPS

```bash
# Start with HTTPS
docker-compose up --build -d

# Test HTTPS endpoint (ignore self-signed warning for dev)
curl --insecure https://localhost:5001/health

# View logs
docker-compose logs -f pserestapi
```

---

## Production Deployment

### Option 1: Reverse Proxy (Recommended)

```
Internet (HTTPS)
	↓
Nginx/Apache (TLS termination)
	↓ (HTTP)
ASP.NET Core (Kestrel)
```

**Advantages:**
- Simpler Kestrel configuration
- Centralized certificate management
- Better performance

**Example Nginx config:**
```nginx
server {
	listen 443 ssl;
	server_name api.example.com;

	ssl_certificate /etc/ssl/certs/server.crt;
	ssl_certificate_key /etc/ssl/private/server.key;

	location / {
		proxy_pass http://pserestapi:8080;
	}
}
```

### Option 2: Kestrel with Secret (Docker Swarm)

```bash
# 1. Create secret from PFX file
docker secret create pserestapi_ssl_cert - < server.pfx

# 2. Set environment variables
ASPNETCORE_Kestrel__Certificates__Default__Path: /run/secrets/ssl_cert
ASPNETCORE_Kestrel__Certificates__Default__Password: your-password

# 3. Deploy
docker-compose -f docker-compose.prod.yml up -d
```

### Option 3: Kubernetes with Secret

```bash
# 1. Create TLS secret
kubectl create secret tls pserestapi-tls \
  --cert=server.crt \
  --key=server.key

# 2. Mount in pod
volumeMounts:
  - name: tls-secret
	mountPath: /etc/ssl/certs
volumes:
  - name: tls-secret
	secret:
	  secretName: pserestapi-tls
```

---

## Certificate Management

### Renewing Development Certificate

```bash
# Remove old certificate
rm -rf certs/certificate.*

# Generate new one
bash generate-dev-cert.sh
```

### Renewing Production Certificate

```bash
# 1. Obtain new certificate from CA
# 2. Create new Docker secret
docker secret create pserestapi_ssl_cert_new - < server.pfx

# 3. Update service to use new secret
docker service update --secret-rm pserestapi_ssl_cert \
					  --secret-add pserestapi_ssl_cert_new \
					  pserestapi

# 4. Remove old secret
docker secret rm pserestapi_ssl_cert
```

---

## Troubleshooting

### Certificate Not Found

```bash
# Check if certificate exists
ls -la certs/certificate.pfx

# Check mounted path in container
docker-compose exec pserestapi ls -la /etc/ssl/certs/

# View container logs
docker-compose logs pserestapi
```

### HTTPS Connection Refused

```bash
# Check port binding
docker ps | grep pserestapi

# Test port
curl -v https://localhost:5001/health

# Check firewall
sudo ufw status
```

### Certificate Password Error

```
# Verify password is correct
# Check environment variable is set
docker-compose exec pserestapi env | grep ASPNETCORE_Kestrel

# View error logs
docker-compose logs pserestapi
```

### Self-Signed Certificate Warning

This is **expected** for development.

**For testing:** Use `curl --insecure` or `--cacert certs/certificate.crt`

**For browsers:** 
1. Import `certs/certificate.crt` to trusted CA store
2. Or accept warning (development only)

---

## Security Best Practices

✅ **Do:**
- Use strong passwords for certificates
- Store certificates securely (Docker secrets, HashiCorp Vault)
- Rotate certificates regularly
- Use trusted CAs for production
- Enable HSTS header (HTTP Strict-Transport-Security)

❌ **Don't:**
- Commit PFX files to git
- Use self-signed certificates in production
- Hardcode certificate passwords in docker-compose
- Expose certificate files in container logs

---

## Environment Variables Reference

### Kestrel HTTPS Configuration

```yaml
ASPNETCORE_URLS: http://+:8080;https://+:8443
ASPNETCORE_Kestrel__Certificates__Default__Path: /path/to/cert.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password: password
```

### Additional Options

```yaml
# Bind specific IP
ASPNETCORE_URLS: https://0.0.0.0:8443

# Multiple certificates
ASPNETCORE_Kestrel__Certificates__Default__Path: cert1.pfx
ASPNETCORE_Kestrel__Certificates__Other__Path: cert2.pfx

# Disable HTTP
ASPNETCORE_URLS: https://+:8443
```

---

## Command Reference

### Development

```bash
# Generate certificate
bash generate-dev-cert.sh

# Start with HTTPS
docker-compose up --build -d

# Test HTTPS
curl --insecure https://localhost:5001/health
curl --cacert certs/certificate.crt https://localhost:5001/health

# View logs
docker-compose logs -f pserestapi

# Restart with certificate reload
docker-compose restart pserestapi
```

### Production (Docker Swarm)

```bash
# Create certificate secret
docker secret create pserestapi_ssl_cert - < server.pfx

# Check secret
docker secret ls

# Deploy with certificate
docker-compose -f docker-compose.prod.yml up -d

# Verify HTTPS
curl --insecure https://localhost:8443/health

# Update certificate
docker secret create pserestapi_ssl_cert_v2 - < new-server.pfx
docker service update --secret-rm pserestapi_ssl_cert \
					  --secret-add pserestapi_ssl_cert_v2 \
					  pserestapi
```

---

## Testing HTTPS

### Using curl

```bash
# Ignore certificate warnings (dev only)
curl --insecure https://localhost:5001/health

# With custom CA certificate
curl --cacert certs/certificate.crt https://localhost:5001/health

# Verbose output
curl -v --insecure https://localhost:5001/health

# Test API endpoint
curl --insecure https://localhost:5001/api/stockprice/ALI
```

### Using PowerShell

```powershell
# Ignore certificate warnings (dev only)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
Invoke-WebRequest -Uri "https://localhost:5001/health" -UseBasicParsing

# Check certificate
$cert = Get-ChildItem -Path "Cert:\LocalMachine\My" | Where-Object {$_.Subject -like "*pserestapi*"}
$cert | Format-List
```

### Using OpenSSL

```bash
# View certificate details
openssl x509 -in certs/certificate.crt -text -noout

# Check certificate expiry
openssl x509 -in certs/certificate.crt -noout -dates

# Test connection
openssl s_client -connect localhost:5001 -showcerts
```

---

## Docker Swarm Deployment Example

```bash
# 1. Initialize Swarm
docker swarm init

# 2. Create secrets
docker secret create db_connection_string - < secrets/db_connection_string.txt
docker secret create ssl_cert - < server.pfx
echo "your-password" | docker secret create ssl_cert_password -

# 3. Deploy stack
docker stack deploy -c docker-compose.prod.yml pserestapi

# 4. Verify
docker service ls
docker service ps pserestapi_pserestapi

# 5. Check logs
docker service logs pserestapi_pserestapi

# 6. Test
curl --insecure https://localhost:5001/health
```

---

## Kubernetes Deployment Example

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: pserestapi-tls
type: kubernetes.io/tls
data:
  tls.crt: <base64-encoded-cert>
  tls.key: <base64-encoded-key>
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pserestapi
spec:
  containers:
  - name: pserestapi
	image: pserestapi:latest
	ports:
	- name: https
	  containerPort: 8443
	env:
	- name: ASPNETCORE_URLS
	  value: "https://+:8443"
	- name: ASPNETCORE_Kestrel__Certificates__Default__Path
	  value: "/etc/ssl/certs/tls.crt"
	volumeMounts:
	- name: tls
	  mountPath: /etc/ssl/certs
  volumes:
  - name: tls
	secret:
	  secretName: pserestapi-tls
```

---

## Next Steps

1. **Development:** Run `generate-dev-cert.sh` and test with `docker-compose up`
2. **Testing:** Verify HTTPS with `curl --insecure https://localhost:5001/health`
3. **Production:** Obtain SSL certificate and configure according to your deployment platform
4. **Monitoring:** Set up certificate expiry alerts

---

## References

- [ASP.NET Core HTTPS Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints)
- [Docker Secrets Documentation](https://docs.docker.com/engine/swarm/secrets/)
- [Let's Encrypt Free SSL](https://letsencrypt.org/)
- [Kubernetes TLS Secrets](https://kubernetes.io/docs/concepts/configuration/secret/#tls-secrets)

---

**Ready to secure your API with HTTPS!** 🔒
