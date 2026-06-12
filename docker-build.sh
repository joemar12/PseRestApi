#!/bin/bash
# Docker build and push script for PseRestApi
# Usage: ./docker-build.sh [version] [registry]

set -e

VERSION="${1:-latest}"
REGISTRY="${2:-docker.io/yourusername}"
IMAGE_NAME="pserestapi"

echo "🐳 Building PseRestApi Docker images..."
echo "   Version: $VERSION"
echo "   Registry: $REGISTRY"
echo ""

# Build API image
echo "📦 Building API image: $REGISTRY/$IMAGE_NAME:$VERSION"
docker build \
  --build-arg BUILD_VERSION="$VERSION" \
  -f Dockerfile \
  -t "$REGISTRY/$IMAGE_NAME:$VERSION" \
  -t "$REGISTRY/$IMAGE_NAME:latest" \
  .

if [ $? -eq 0 ]; then
  echo "✅ API image built successfully"
else
  echo "❌ API image build failed"
  exit 1
fi

# Build Sync image
echo ""
echo "📦 Building Sync image: $REGISTRY/$IMAGE_NAME-sync:$VERSION"
docker build \
  --build-arg BUILD_VERSION="$VERSION" \
  -f Dockerfile.Sync \
  -t "$REGISTRY/$IMAGE_NAME-sync:$VERSION" \
  -t "$REGISTRY/$IMAGE_NAME-sync:latest" \
  .

if [ $? -eq 0 ]; then
  echo "✅ Sync image built successfully"
else
  echo "❌ Sync image build failed"
  exit 1
fi

echo ""
echo "🎉 All images built successfully!"
echo ""
echo "📤 To push images to registry:"
echo "   docker push $REGISTRY/$IMAGE_NAME:$VERSION"
echo "   docker push $REGISTRY/$IMAGE_NAME-sync:$VERSION"
echo ""
echo "🚀 To run locally:"
echo "   docker-compose up -d"
echo ""
echo "📝 To run with production config:"
echo "   docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d"
