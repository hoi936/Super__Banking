# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY ["LocalLink.sln", "./"]
COPY ["src/LocalLink.API/LocalLink.API.csproj", "src/LocalLink.API/"]
COPY ["src/LocalLink.Application/LocalLink.Application.csproj", "src/LocalLink.Application/"]
COPY ["src/LocalLink.Domain/LocalLink.Domain.csproj", "src/LocalLink.Domain/"]
COPY ["src/LocalLink.Infrastructure/LocalLink.Infrastructure.csproj", "src/LocalLink.Infrastructure/"]
COPY ["src/LocalLink.Shared/LocalLink.Shared.csproj", "src/LocalLink.Shared/"]
COPY ["tests/LocalLink.UnitTests/LocalLink.UnitTests.csproj", "tests/LocalLink.UnitTests/"]

# Restore dependencies
RUN dotnet restore "LocalLink.sln"

# Copy full source code
COPY src/ ./src/
COPY tests/ ./tests/

# Build and publish API project
WORKDIR /src/src/LocalLink.API
RUN dotnet publish "LocalLink.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LocalLink.API.dll"]
