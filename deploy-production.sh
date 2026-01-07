#!/bin/bash

# ========================================
# SCRIPT DEPLOY TRÊN MÁY PRODUCTION
# ========================================
# Script này chạy trên máy production để pull image từ registry và deploy

set -e

# Configuration
REGISTRY_URL="${REGISTRY_URL:-192.168.1.100:5000}"  # Thay bằng IP máy CI
IMAGE_NAME="football-api"
IMAGE_TAG="${IMAGE_TAG:-latest}"

echo "========================================"
echo "Deploying Football Field Booking API"
echo "Registry: ${REGISTRY_URL}"
echo "Image: ${IMAGE_NAME}:${IMAGE_TAG}"
echo "========================================"

# Step 1: Login to registry
echo "Step 1: Login to Docker Registry..."
echo "admin123" | docker login ${REGISTRY_URL} -u admin --password-stdin

# Step 2: Pull latest image
echo "Step 2: Pulling latest image..."
docker pull ${REGISTRY_URL}/${IMAGE_NAME}:${IMAGE_TAG}

# Step 3: Stop existing containers
echo "Step 3: Stopping existing containers..."
docker compose -f docker-compose.prod.yml --env-file .env.prod down || true

# Step 4: Start new containers
echo "Step 4: Starting new containers..."
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d --force-recreate

# Step 5: Check health
echo "Step 5: Checking container health..."
sleep 5
docker compose -f docker-compose.prod.yml ps

echo "========================================"
echo "✅ Deployment completed successfully!"
echo "========================================"
