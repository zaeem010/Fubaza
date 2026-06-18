# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY API/Fubaza.API/*.csproj ./API/Fubaza.API/
COPY Application/Fubaza.Application.Infrastructure/*.csproj ./Application/Fubaza.Application.Infrastructure/
COPY Application/Fubaza.Application.Core/*.csproj ./Application/Fubaza.Application.Core/
COPY Application/Fubaza.Application.Utilities/*.csproj ./Application/Fubaza.Application.Utilities/

# Restore packages (solution-level)
RUN dotnet restore Fubaza.sln

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/API/Fubaza.API"
RUN dotnet publish "Fubaza.API.csproj" \
    -c Release \
    -o /app/publish \
    -p:UseAppHost=false \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Fubaza.API.dll"]
