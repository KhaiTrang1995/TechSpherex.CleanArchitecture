# ============================================================
# TechSpherex Clean Architecture — Production Dockerfile
# Supports both JIT (default) and Native AOT builds.
# Usage:
#   docker build -t techspherex-api .
#   docker build --build-arg PUBLISH_AOT=true -t techspherex-api:aot .
# ============================================================

# ---- Stage 1: Restore ----
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY Directory.Build.props Directory.Packages.props ./
COPY src/TechSpherex.CleanArchitecture.slnx ./src/
COPY src/Domain/TechSpherex.CleanArchitecture.Domain.csproj                       src/Domain/
COPY src/Application/TechSpherex.CleanArchitecture.Application.csproj             src/Application/
COPY src/Infrastructure/TechSpherex.CleanArchitecture.Infrastructure.csproj       src/Infrastructure/
COPY src/Api/TechSpherex.CleanArchitecture.Api.csproj                             src/Api/
COPY src/ServiceDefaults/TechSpherex.CleanArchitecture.ServiceDefaults.csproj     src/ServiceDefaults/

RUN dotnet restore src/Api/TechSpherex.CleanArchitecture.Api.csproj

# ---- Stage 2: Build & Publish ----
FROM restore AS build
ARG PUBLISH_AOT=false
ARG BUILD_CONFIGURATION=Release

COPY src/ src/

RUN if [ "$PUBLISH_AOT" = "true" ]; then \
        dotnet publish src/Api/TechSpherex.CleanArchitecture.Api.csproj \
            -c $BUILD_CONFIGURATION \
            -o /app/publish \
            /p:PublishAot=true \
            /p:StripSymbols=true; \
    else \
        dotnet publish src/Api/TechSpherex.CleanArchitecture.Api.csproj \
            -c $BUILD_CONFIGURATION \
            -o /app/publish \
            /p:PublishAot=false; \
    fi

# ---- Stage 3: Runtime (JIT) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
LABEL maintainer="TechSpherex <contact@techspherex.com>"
LABEL org.opencontainers.image.source="https://github.com/KhaiTrang1995/clean-architecture-template"

# Security: non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Expose default Kestrel port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER appuser
ENTRYPOINT ["dotnet", "TechSpherex.CleanArchitecture.Api.dll"]
