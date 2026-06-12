# Self-Signed SSL Certificate Generation Script for Windows
# Generates a self-signed certificate for local development with ASP.NET Core
# Requirements: PowerShell 5.0+, OpenSSL installed (or use Windows-native certificate creation)
# Usage: .\generate-dev-cert.ps1 [-Password "dev-password"] [-Days 365]

[CmdletBinding()]
param(
	[string]$Password = "dev-password",
	[int]$Days = 365,
	[string]$CertDir = "certs",
	[string]$CertName = "certificate"
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success {
	Write-Host $args -ForegroundColor Green
}

function Write-Info {
	Write-Host $args -ForegroundColor Blue
}

function Write-Warning {
	Write-Host $args -ForegroundColor Yellow
}

Write-Info "════════════════════════════════════════════════════════════"
Write-Info "    Self-Signed SSL Certificate Generation for Windows"
Write-Info "════════════════════════════════════════════════════════════"
Write-Host ""

# Check if OpenSSL is available
try {
	$opensslVersion = openssl version 2>&1
	Write-Success "✓ OpenSSL found: $opensslVersion"
}
catch {
	Write-Warning "⚠ OpenSSL not found in PATH"
	Write-Warning ""
	Write-Warning "Install OpenSSL using one of these methods:"
	Write-Warning "  1. Chocolatey: choco install openssl"
	Write-Warning "  2. Download: https://slproweb.com/products/Win32OpenSSL.html"
	Write-Warning "  3. Windows Store: 'OpenSSL' or 'WSL'"
	Write-Warning ""
	Write-Warning "Or use the Windows-native method below (no OpenSSL needed):"
	Write-Host ""
	exit 1
}

# Create certificates directory
if (-not (Test-Path $CertDir)) {
	New-Item -ItemType Directory -Path $CertDir | Out-Null
	Write-Success "✓ Created $CertDir\ directory"
}

# Check if certificate already exists
$pfxPath = Join-Path $CertDir "$CertName.pfx"
if (Test-Path $pfxPath) {
	Write-Warning "⚠ Certificate already exists at $pfxPath"
	$response = Read-Host "Do you want to regenerate it? (y/n)"
	if ($response -ne 'y') {
		Write-Host "Aborted."
		exit 0
	}
	Write-Host "Removing old certificates..."
	Remove-Item (Join-Path $CertDir "$CertName.*") -Force
}

Write-Info "Generating self-signed certificate..."
Write-Host ""

# Step 1: Generate private key
Write-Info "Step 1/3: Generating private key..."
& openssl genrsa -out (Join-Path $CertDir "$CertName.key") 2048 2>$null
Write-Success "✓ Private key generated"

# Step 2: Generate certificate signing request (CSR)
Write-Info "Step 2/3: Creating certificate request..."
$subj = "/C=PH/ST=Metro Manila/L=Makati/O=PseRestApi/CN=localhost"
& openssl req -new `
	-key (Join-Path $CertDir "$CertName.key") `
	-out (Join-Path $CertDir "$CertName.csr") `
	-subj $subj `
	2>$null
Write-Success "✓ Certificate request created"

# Step 3: Generate self-signed certificate with extensions
Write-Info "Step 3/3: Generating self-signed certificate..."

# Create config file for certificate extensions
$configPath = Join-Path $CertDir "cert_ext.cnf"
@"
[v3_req]
subjectAltName = DNS:localhost,DNS:*.localhost,IP:127.0.0.1,IP:0.0.0.0
"@ | Set-Content $configPath

& openssl x509 -req `
	-days $Days `
	-in (Join-Path $CertDir "$CertName.csr") `
	-signkey (Join-Path $CertDir "$CertName.key") `
	-out (Join-Path $CertDir "$CertName.crt") `
	-extfile $configPath `
	-extensions v3_req `
	2>$null

Write-Success "✓ Self-signed certificate created"

# Step 4: Create PKCS12 (.pfx) format for use with Kestrel
Write-Info "Creating PKCS12 format (.pfx) for Kestrel..."
& openssl pkcs12 -export `
	-in (Join-Path $CertDir "$CertName.crt") `
	-inkey (Join-Path $CertDir "$CertName.key") `
	-out (Join-Path $CertDir "$CertName.pfx") `
	-name $CertName `
	-passout pass:$Password `
	2>$null

Write-Success "✓ PKCS12 (.pfx) format created"

# Remove temporary files
Remove-Item (Join-Path $CertDir "$CertName.csr") -Force
Remove-Item $configPath -Force

Write-Host ""
Write-Info "════════════════════════════════════════════════════════════"
Write-Success "✓ Certificate Generation Complete!"
Write-Info "════════════════════════════════════════════════════════════"
Write-Host ""

# Display certificate information
Write-Info "Certificate Details:"
Write-Host "  Location: $CertDir\"
Write-Host "  Files:"
Write-Host "    • $CertName.pfx      - PKCS12 format (for Kestrel)"
Write-Host "    • $CertName.crt      - Public certificate"
Write-Host "    • $CertName.key      - Private key"
Write-Host ""

Write-Info "Certificate Information:"
& openssl x509 -in (Join-Path $CertDir "$CertName.crt") -text -noout | `
	Select-String "Subject:|Issuer:|Not Before|Not After|Public Key" | `
	ForEach-Object { Write-Host "  $_" }

Write-Host ""
Write-Info "Usage:"
Write-Host "  Password: $Password"
Write-Host ""

Write-Warning "Development (Docker Compose):"
Write-Host "  docker-compose up --build -d"
Write-Host "  curl --insecure https://localhost:5001/health"
Write-Host ""

Write-Warning "Testing (with certificate):"
Write-Host "  curl --cacert $CertDir\$CertName.crt https://localhost:5001/health"
Write-Host ""

Write-Warning "View certificate expiry:"
Write-Host "  openssl x509 -in $CertDir\$CertName.crt -noout -dates"
Write-Host ""

Write-Warning "PowerShell test:"
Write-Host "  [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {`$true}"
Write-Host "  Invoke-WebRequest -Uri 'https://localhost:5001/health' -UseBasicParsing"
Write-Host ""

Write-Success "✓ Ready to use with docker-compose!"
Write-Host ""

# Optional: Add to .gitignore
$gitIgnorePath = ".gitignore"
if (Test-Path $gitIgnorePath) {
	$gitIgnoreContent = Get-Content $gitIgnorePath
	if (-not ($gitIgnoreContent -contains "$CertDir/$CertName.pfx")) {
		Write-Warning "ℹ Consider adding to .gitignore to prevent accidental commit:"
		Write-Host "  $CertDir/$CertName.pfx"
	}
}
