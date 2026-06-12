#!/bin/bash

# Self-Signed SSL Certificate Generation Script
# Generates a self-signed certificate for local development with ASP.NET Core
# Usage: bash generate-dev-cert.sh [password]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Configuration
CERT_DIR="certs"
CERT_NAME="certificate"
PASSWORD="${1:-dev-password}"
DAYS_VALID=365

# Subject information for certificate
COUNTRY="PH"
STATE="Metro Manila"
CITY="Makati"
ORGANIZATION="PseRestApi"
COMMON_NAME="localhost"

echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${BLUE}    Self-Signed SSL Certificate Generation${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""

# Check if openssl is available
if ! command -v openssl &> /dev/null; then
	echo -e "${RED}✗ OpenSSL is not installed${NC}"
	echo -e "${YELLOW}Install it using:${NC}"
	echo -e "${YELLOW}  Ubuntu/Debian: sudo apt-get install openssl${NC}"
	echo -e "${YELLOW}  macOS: brew install openssl${NC}"
	echo -e "${YELLOW}  Windows: Install from https://slproweb.com/products/Win32OpenSSL.html${NC}"
	exit 1
fi

# Create certificates directory
if [ ! -d "$CERT_DIR" ]; then
	mkdir -p "$CERT_DIR"
	echo -e "${GREEN}✓ Created$NC $CERT_DIR/ directory"
fi

# Check if certificate already exists
if [ -f "$CERT_DIR/$CERT_NAME.pfx" ]; then
	echo -e "${YELLOW}⚠ Certificate already exists at $CERT_DIR/$CERT_NAME.pfx${NC}"
	read -p "Do you want to regenerate it? (y/n) " -n 1 -r
	echo
	if [[ ! $REPLY =~ ^[Yy]$ ]]; then
		echo "Aborted."
		exit 0
	fi
	echo -e "${YELLOW}Removing old certificate...${NC}"
	rm -f "$CERT_DIR/$CERT_NAME".*
fi

echo -e "${BLUE}Generating self-signed certificate...${NC}"
echo ""

# Step 1: Generate private key
echo -e "${BLUE}Step 1/3: Generating private key...${NC}"
openssl genrsa -out "$CERT_DIR/$CERT_NAME.key" 2048 2>/dev/null
echo -e "${GREEN}✓ Private key generated${NC}"

# Step 2: Generate certificate signing request (CSR)
echo -e "${BLUE}Step 2/3: Creating certificate request...${NC}"
openssl req -new \
	-key "$CERT_DIR/$CERT_NAME.key" \
	-out "$CERT_DIR/$CERT_NAME.csr" \
	-subj "/C=$COUNTRY/ST=$STATE/L=$CITY/O=$ORGANIZATION/CN=$COMMON_NAME" \
	2>/dev/null
echo -e "${GREEN}✓ Certificate request created${NC}"

# Step 3: Generate self-signed certificate
echo -e "${BLUE}Step 3/3: Generating self-signed certificate...${NC}"
openssl x509 -req \
	-days $DAYS_VALID \
	-in "$CERT_DIR/$CERT_NAME.csr" \
	-signkey "$CERT_DIR/$CERT_NAME.key" \
	-out "$CERT_DIR/$CERT_NAME.crt" \
	-extfile <(printf "subjectAltName=DNS:localhost,DNS:*.localhost,IP:127.0.0.1,IP:0.0.0.0") \
	2>/dev/null
echo -e "${GREEN}✓ Self-signed certificate created${NC}"

# Step 4: Create PKCS12 (.pfx) format for use with Kestrel
echo -e "${BLUE}Creating PKCS12 format (.pfx) for Kestrel...${NC}"
openssl pkcs12 -export \
	-in "$CERT_DIR/$CERT_NAME.crt" \
	-inkey "$CERT_DIR/$CERT_NAME.key" \
	-out "$CERT_DIR/$CERT_NAME.pfx" \
	-name "$CERT_NAME" \
	-passout pass:"$PASSWORD" \
	2>/dev/null
echo -e "${GREEN}✓ PKCS12 (.pfx) format created${NC}"

# Remove temporary CSR file
rm -f "$CERT_DIR/$CERT_NAME.csr"

echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✓ Certificate Generation Complete!${NC}"
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""

# Display certificate information
echo -e "${BLUE}Certificate Details:${NC}"
echo "  Location: $CERT_DIR/"
echo "  Files:"
echo "    • $CERT_NAME.pfx      - PKCS12 format (for Kestrel)"
echo "    • $CERT_NAME.crt      - Public certificate"
echo "    • $CERT_NAME.key      - Private key"
echo ""
echo -e "${BLUE}Certificate Information:${NC}"
openssl x509 -in "$CERT_DIR/$CERT_NAME.crt" -text -noout | grep -E "Subject:|Issuer:|Not Before|Not After|Public Key" | sed 's/^/  /'

echo ""
echo -e "${BLUE}Usage:${NC}"
echo "  Password: $PASSWORD"
echo ""
echo -e "${YELLOW}Development (Docker Compose):${NC}"
echo "  docker-compose up --build -d"
echo "  curl --insecure https://localhost:5001/health"
echo ""
echo -e "${YELLOW}Testing (with certificate):${NC}"
echo "  curl --cacert $CERT_DIR/$CERT_NAME.crt https://localhost:5001/health"
echo ""
echo -e "${YELLOW}View certificate (expires):${NC}"
echo "  openssl x509 -in $CERT_DIR/$CERT_NAME.crt -noout -dates"
echo ""
echo -e "${YELLOW}Browser/CA store:${NC}"
echo "  1. Import $CERT_DIR/$CERT_NAME.crt to your CA store"
echo "  2. Or accept browser warning (development only)"
echo ""

# Git-ignore notice
if [ -f ".gitignore" ] && ! grep -q "$CERT_DIR/$CERT_NAME.pfx" ".gitignore" 2>/dev/null; then
	echo -e "${YELLOW}ℹ Add to .gitignore to prevent accidental commit:${NC}"
	echo "  $CERT_DIR/$CERT_NAME.pfx"
fi

echo ""
echo -e "${GREEN}✓ Ready to use with docker-compose!${NC}"
echo ""
