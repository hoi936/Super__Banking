#!/bin/bash
set -e
echo "Applying EF Core migrations..."
dotnet dotnet-ef database update --project src/LocalLink.Infrastructure --startup-project src/LocalLink.API
echo "Database update complete."
