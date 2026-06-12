#!/usr/bin/env bash
# Docker Secrets Implementation Checklist
# Use this to verify everything is set up correctly

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║        Docker Secrets Implementation Checklist              ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Counter for completed items
COMPLETED=0
TOTAL=0

check_item() {
  TOTAL=$((TOTAL + 1))
  local description="$1"
  local check_command="$2"

  echo -n "  [$TOTAL] $description ... "

  if eval "$check_command" > /dev/null 2>&1; then
	echo -e "${GREEN}✓${NC}"
	COMPLETED=$((COMPLETED + 1))
  else
	echo -e "${RED}✗${NC}"
  fi
}

# Files Created
echo -e "${YELLOW}📁 Files & Configuration${NC}"
check_item "Docker Compose exists" "test -f docker-compose.yml"
check_item "Docker Compose Prod exists" "test -f docker-compose.prod.yml"
check_item "Secrets example exists" "test -f secrets/db_connection_string.txt.example"
check_item "Manage secrets script exists" "test -f manage-secrets.sh"
check_item "Secrets guide exists" "test -f DOCKER_SECRETS_GUIDE.md"
check_item "Secrets summary exists" "test -f DOCKER_SECRETS_SUMMARY.md"
check_item "README exists" "test -f README_DOCKER_SECRETS.md"

echo ""
echo -e "${YELLOW}🔐 Secrets Directory${NC}"
check_item "Secrets directory exists" "test -d secrets"
check_item "Connection string example exists" "test -f secrets/db_connection_string.txt.example"

echo ""
echo -e "${YELLOW}🐳 Docker Configuration${NC}"
check_item "docker-compose has secrets section" "grep -q 'secrets:' docker-compose.yml"
check_item "docker-compose has pserestapi service" "grep -q 'pserestapi:' docker-compose.yml"
check_item "docker-compose has Sync service" "grep -q 'pserestapi-sync:' docker-compose.yml"
check_item "docker-compose references secret" "grep -q '/run/secrets/db_connection_string' docker-compose.yml"
check_item "docker-compose.prod has secrets" "grep -q 'secrets:' docker-compose.prod.yml"

echo ""
echo -e "${YELLOW}💻 Source Code${NC}"
check_item "Host Program.cs exists" "test -f src/PseRestApi.Host/Program.cs"
check_item "Sync Program.cs exists" "test -f src/PseRestApi.Sync/Program.cs"
check_item "Host reads secrets" "grep -q '/run/secrets/' src/PseRestApi.Host/Program.cs"
check_item "Sync reads secrets" "grep -q '/run/secrets/' src/PseRestApi.Sync/Program.cs"
check_item "HealthController exists" "test -f src/PseRestApi.Host/Controllers/HealthController.cs"

echo ""
echo -e "${YELLOW}🔧 Git Configuration${NC}"
check_item ".gitignore has secrets entry" "grep -q 'secrets/db_connection_string.txt' .gitignore"

echo ""
echo -e "${YELLOW}📦 NuGet Packages${NC}"
check_item "Microsoft.Extensions.Http in Host" "grep -q 'Microsoft.Extensions.Http' src/PseRestApi.Host/PseRestApi.Host.csproj"
check_item "Microsoft.Extensions.Http in Sync" "grep -q 'Microsoft.Extensions.Http' src/PseRestApi.Sync/PseRestApi.Sync.csproj"

echo ""
echo -e "${YELLOW}🏗️  Docker Images${NC}"
check_item "Dockerfile exists" "test -f Dockerfile"
check_item "Dockerfile.Sync exists" "test -f Dockerfile.Sync"

echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "Checklist: ${GREEN}$COMPLETED${NC} / $TOTAL items verified"

if [ $COMPLETED -eq $TOTAL ]; then
  echo -e "${GREEN}✓ All items verified!${NC}"
  echo ""
  echo "Next steps:"
  echo "  1. Edit: nano secrets/db_connection_string.txt.example"
  echo "  2. Copy: cp secrets/db_connection_string.txt.example secrets/db_connection_string.txt"
  echo "  3. Fill: Add your connection string to secrets/db_connection_string.txt"
  echo "  4. Start: docker-compose up --build -d"
  echo "  5. Test: curl http://localhost:5000/health"
  exit 0
else
  echo -e "${YELLOW}⚠️  Some items need attention (see above)${NC}"
  exit 1
fi
