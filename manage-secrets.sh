#!/bin/bash
# Docker Secrets Management Script for PseRestApi
# Manages creation, update, and deletion of Docker secrets

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

show_usage() {
  cat << EOF
Usage: ./manage-secrets.sh [command] [options]

Commands:
  create              Create Docker secrets (interactive)
  create-file FILE    Create secrets from file
  list                List existing secrets
  remove              Remove all PseRestApi secrets
  help                Show this help message

Environment Variables Required (for create):
  DB_CONNECTION_STRING    Database connection string
  ASPNETCORE_ENV         Environment (Development/Production)

Examples:
  # Interactive creation
  ./manage-secrets.sh create

  # Create from file
  ./manage-secrets.sh create-file secrets.txt

  # List secrets
  ./manage-secrets.sh list

  # Remove secrets
  ./manage-secrets.sh remove

For Docker Compose (Local Development):
  • Create a secrets/ directory
  • Put connection string in secrets/db_connection_string.txt
  • Uncomment secrets section in docker-compose.yml

For Docker Swarm (Production):
  • Use: docker secret create
  • Secrets are encrypted at rest
  • Mounted read-only in containers

For Kubernetes:
  • Use: kubectl create secret
  • Configure in deployment manifests

EOF
}

# Create secrets interactively
create_secrets() {
  echo -e "${BLUE}Creating Docker Secrets for PseRestApi${NC}"
  echo ""

  # Check if Docker is running
  if ! docker info > /dev/null 2>&1; then
	echo -e "${RED}❌ Docker is not running${NC}"
	exit 1
  fi

  # Check for Swarm mode
  SWARM_STATUS=$(docker info --format '{{.Swarm.LocalNodeState}}')

  if [ "$SWARM_STATUS" == "active" ]; then
	echo -e "${GREEN}✓ Docker Swarm is active${NC}"
	create_swarm_secrets
  else
	echo -e "${YELLOW}⚠️ Docker Swarm not active${NC}"
	echo "For local development, using file-based secrets."
	echo "For production, initialize Docker Swarm:"
	echo "  docker swarm init"
	create_file_secrets
  fi
}

# Create Swarm secrets
create_swarm_secrets() {
  echo ""
  echo "Database Connection String (or press Ctrl+C to skip):"
  read -r DB_CONNECTION_STRING

  if [ -n "$DB_CONNECTION_STRING" ]; then
	# Remove old secret if exists
	docker secret rm pserestapi_db_connection 2>/dev/null || true

	# Create new secret
	echo "$DB_CONNECTION_STRING" | docker secret create pserestapi_db_connection -
	echo -e "${GREEN}✅ Secret 'pserestapi_db_connection' created${NC}"
  fi
}

# Create file-based secrets (for docker-compose)
create_file_secrets() {
  mkdir -p secrets

  echo ""
  echo "Database Connection String:"
  read -r DB_CONNECTION_STRING

  if [ -n "$DB_CONNECTION_STRING" ]; then
	echo "$DB_CONNECTION_STRING" > secrets/db_connection_string.txt
	chmod 600 secrets/db_connection_string.txt
	echo -e "${GREEN}✅ Secret saved to secrets/db_connection_string.txt${NC}"
  fi

  echo ""
  echo "To use these secrets:"
  echo "1. Uncomment secrets section in docker-compose.yml"
  echo "2. Run: docker-compose up -d"
}

# Create secrets from file
create_from_file() {
  if [ -z "$1" ]; then
	echo -e "${RED}Error: File path required${NC}"
	show_usage
	exit 1
  fi

  if [ ! -f "$1" ]; then
	echo -e "${RED}Error: File not found: $1${NC}"
	exit 1
  fi

  SWARM_STATUS=$(docker info --format '{{.Swarm.LocalNodeState}}')

  if [ "$SWARM_STATUS" == "active" ]; then
	# Docker Swarm mode
	while IFS='=' read -r key value; do
	  if [ -n "$key" ] && [ "${key:0:1}" != "#" ]; then
		secret_name="pserestapi_${key,,}"
		echo "Creating secret: $secret_name"
		echo "$value" | docker secret create "$secret_name" - 2>/dev/null || \
		(docker secret rm "$secret_name" 2>/dev/null && echo "$value" | docker secret create "$secret_name" -)
	  fi
	done < "$1"
	echo -e "${GREEN}✅ Secrets created from file${NC}"
  else
	# File-based for docker-compose
	mkdir -p secrets
	while IFS='=' read -r key value; do
	  if [ -n "$key" ] && [ "${key:0:1}" != "#" ]; then
		filename="secrets/${key,,}.txt"
		echo "$value" > "$filename"
		chmod 600 "$filename"
		echo "Created: $filename"
	  fi
	done < "$1"
	echo -e "${GREEN}✅ Secrets saved to secrets/ directory${NC}"
  fi
}

# List secrets
list_secrets() {
  echo -e "${BLUE}PseRestApi Secrets:${NC}"
  echo ""

  SWARM_STATUS=$(docker info --format '{{.Swarm.LocalNodeState}}')

  if [ "$SWARM_STATUS" == "active" ]; then
	docker secret list | grep pserestapi || echo "No PseRestApi secrets found"
  else
	if [ -d "secrets" ]; then
	  echo "File-based secrets in secrets/ directory:"
	  ls -lah secrets/ 2>/dev/null || echo "No secrets found"
	else
	  echo "No secrets directory found"
	fi
  fi
}

# Remove secrets
remove_secrets() {
  echo -e "${YELLOW}⚠️ Removing PseRestApi secrets...${NC}"
  read -p "Are you sure? (yes/no): " confirm

  if [ "$confirm" != "yes" ]; then
	echo "Cancelled."
	exit 0
  fi

  SWARM_STATUS=$(docker info --format '{{.Swarm.LocalNodeState}}')

  if [ "$SWARM_STATUS" == "active" ]; then
	docker secret ls | grep pserestapi | awk '{print $1}' | while read secret; do
	  docker secret rm "$secret"
	  echo -e "${GREEN}✅ Removed: $secret${NC}"
	done
  else
	if [ -d "secrets" ]; then
	  rm -rf secrets/
	  echo -e "${GREEN}✅ Removed secrets/ directory${NC}"
	fi
  fi

  echo -e "${GREEN}✅ All secrets removed${NC}"
}

# Main
if [ $# -eq 0 ]; then
  show_usage
  exit 0
fi

COMMAND=$1
shift

case $COMMAND in
  create)       create_secrets ;;
  create-file)  create_from_file "$@" ;;
  list)         list_secrets ;;
  remove)       remove_secrets ;;
  help|--help)  show_usage ;;
  *)
	echo -e "${RED}Unknown command: $COMMAND${NC}"
	show_usage
	exit 1
	;;
esac
