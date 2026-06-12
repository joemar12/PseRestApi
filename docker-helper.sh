#!/bin/bash
# Docker deployment helper script for PseRestApi
# Provides common Docker operations

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
show_usage() {
  cat << EOF
Usage: ./docker-helper.sh [command] [options]

Commands:
  up                  Start all services (docker-compose up -d)
  down                Stop all services (docker-compose down)
  restart             Restart all services
  logs [service]      View logs (optionally for specific service)
  ps                  Show running containers
  build               Build Docker images
  push [version]      Push images to registry (requires 'version' argument)
  clean               Remove stopped containers and dangling images
  health              Check health of services
  db-init             Initialize database with migrations
  db-shell            Open SQL Server shell
  bash [service]      Open bash shell in container (default: pserestapi)
  stats               Show container resource usage
  help                Show this help message

Examples:
  ./docker-helper.sh up
  ./docker-helper.sh logs pserestapi
  ./docker-helper.sh push 1.0.0
  ./docker-helper.sh bash pserestapi-sync
  ./docker-helper.sh db-shell

Environment:
  Set .env file for configuration:
	cp .env.example .env
	# Edit .env with your values

EOF
}

# Commands
cmd_up() {
  echo -e "${GREEN}Starting PseRestApi services...${NC}"
  docker-compose up -d
  echo -e "${GREEN}✅ Services started${NC}"
  sleep 3
  cmd_health
}

cmd_down() {
  echo -e "${YELLOW}Stopping PseRestApi services...${NC}"
  docker-compose down
  echo -e "${GREEN}✅ Services stopped${NC}"
}

cmd_restart() {
  echo -e "${YELLOW}Restarting services...${NC}"
  docker-compose restart
  echo -e "${GREEN}✅ Services restarted${NC}"
}

cmd_logs() {
  if [ -z "$1" ]; then
	echo -e "${YELLOW}Showing logs from all services (Ctrl+C to exit)${NC}"
	docker-compose logs -f
  else
	echo -e "${YELLOW}Showing logs from $1 (Ctrl+C to exit)${NC}"
	docker-compose logs -f "$1"
  fi
}

cmd_ps() {
  echo -e "${YELLOW}Running containers:${NC}"
  docker-compose ps
}

cmd_build() {
  echo -e "${GREEN}Building Docker images...${NC}"
  docker build -f Dockerfile -t pserestapi:latest .
  docker build -f Dockerfile.Sync -t pserestapi-sync:latest .
  echo -e "${GREEN}✅ Images built${NC}"
}

cmd_push() {
  if [ -z "$1" ]; then
	echo -e "${RED}Error: Version argument required${NC}"
	echo "Usage: ./docker-helper.sh push [version]"
	exit 1
  fi

  VERSION=$1
  REGISTRY="${REGISTRY:-docker.io/yourusername}"

  echo -e "${GREEN}Pushing images to $REGISTRY...${NC}"
  docker tag pserestapi:latest "$REGISTRY/pserestapi:$VERSION"
  docker tag pserestapi-sync:latest "$REGISTRY/pserestapi-sync:$VERSION"

  docker push "$REGISTRY/pserestapi:$VERSION"
  docker push "$REGISTRY/pserestapi-sync:$VERSION"

  echo -e "${GREEN}✅ Images pushed${NC}"
}

cmd_clean() {
  echo -e "${YELLOW}Cleaning up Docker resources...${NC}"
  docker container prune -f
  docker image prune -f
  echo -e "${GREEN}✅ Cleanup complete${NC}"
}

cmd_health() {
  echo -e "${YELLOW}Checking service health:${NC}"
  echo ""

  # Check API
  if curl -s http://localhost:5000/health > /dev/null; then
	echo -e "${GREEN}✅ API is healthy${NC}"
  else
	echo -e "${RED}❌ API is unhealthy or not running${NC}"
  fi

  # Check database
  if docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD:-P@ssw0rd123!}" -Q "SELECT 1" > /dev/null 2>&1; then
	echo -e "${GREEN}✅ Database is healthy${NC}"
  else
	echo -e "${RED}❌ Database is unhealthy or not running${NC}"
  fi

  echo ""
  docker-compose ps
}

cmd_db_init() {
  echo -e "${GREEN}Running database migrations...${NC}"
  docker-compose exec pserestapi dotnet ef database update
  echo -e "${GREEN}✅ Database initialized${NC}"
}

cmd_db_shell() {
  echo -e "${YELLOW}Connecting to SQL Server...${NC}"
  docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD:-P@ssw0rd123!}"
}

cmd_bash() {
  SERVICE="${1:-pserestapi}"
  echo -e "${YELLOW}Opening bash shell in $SERVICE...${NC}"
  docker-compose exec "$SERVICE" bash
}

cmd_stats() {
  echo -e "${YELLOW}Container resource usage:${NC}"
  docker stats --no-stream
}

cmd_help() {
  show_usage
}

# Main script logic
if [ $# -eq 0 ]; then
  echo -e "${RED}Error: No command provided${NC}"
  echo ""
  show_usage
  exit 1
fi

COMMAND=$1
shift

case $COMMAND in
  up)          cmd_up ;;
  down)        cmd_down ;;
  restart)     cmd_restart ;;
  logs)        cmd_logs "$@" ;;
  ps)          cmd_ps ;;
  build)       cmd_build ;;
  push)        cmd_push "$@" ;;
  clean)       cmd_clean ;;
  health)      cmd_health ;;
  db-init)     cmd_db_init ;;
  db-shell)    cmd_db_shell ;;
  bash)        cmd_bash "$@" ;;
  stats)       cmd_stats ;;
  help|--help|-h) cmd_help ;;
  *)           
	echo -e "${RED}Error: Unknown command '$COMMAND'${NC}"
	echo ""
	show_usage
	exit 1
	;;
esac
