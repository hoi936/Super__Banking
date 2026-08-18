#!/bin/bash
set -e
echo "Resetting LocalLink Database and Docker volumes..."
docker compose down -v
echo "Rebuilding and starting fresh containers..."
docker compose up --build -d
echo "Database reset complete. Follow logs with: docker compose logs -f"
