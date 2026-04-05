# Deployment Guide

## Production Checklist

### Security
- [ ] Change JWT secret to a strong, unique key (≥32 chars)
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Use Docker secrets or Key Vault for sensitive config
- [ ] Enable HTTPS with valid certificates
- [ ] Remove Scalar API docs in production (already conditional on `IsDevelopment()`)
- [ ] Disable database seeder in production
- [ ] Set strong PostgreSQL and Redis passwords
- [ ] Run containers as non-root user (already configured in Dockerfile)

### Database
- [ ] Run EF Core migrations before deployment
- [ ] Enable connection pooling (Npgsql has built-in)
- [ ] Set up automated backups
- [ ] Enable SSL for PostgreSQL connections

### Caching
- [ ] Configure Redis persistence (AOF or RDB)
- [ ] Set Redis maxmemory and eviction policy
- [ ] Enable Redis password authentication

### Observability
- [ ] Configure Serilog for production (disable Debug/Verbose)
- [ ] Set up alerting in Grafana or equivalent
- [ ] Configure log retention policies
- [ ] Enable distributed tracing

### Performance
- [ ] Enable response compression
- [ ] Configure connection limits
- [ ] Set up horizontal scaling (multiple API instances)
- [ ] Consider Native AOT for cold-start-sensitive deployments

## CI/CD Pipeline

### GitHub Actions Example

```yaml
name: Build and Deploy

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore src/TechSpherex.CleanArchitecture.slnx
      - run: dotnet build src/TechSpherex.CleanArchitecture.slnx --no-restore
      - run: dotnet test src/TechSpherex.CleanArchitecture.slnx --no-build

  build-image:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    permissions:
      contents: read
      packages: write
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - uses: docker/build-push-action@v5
        with:
          push: true
          tags: |
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}

  deploy:
    needs: build-image
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Deploy to server
        run: |
          # SSH to server and pull new image
          ssh deploy@your-server << 'EOF'
            cd /opt/techspherex
            docker compose pull api
            docker compose up -d api
          EOF
```

### Azure DevOps Pipeline

```yaml
trigger:
  branches:
    include: [main]

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      packageType: 'sdk'
      version: '10.0.x'

  - script: dotnet test src/TechSpherex.CleanArchitecture.slnx
    displayName: 'Run Tests'

  - task: Docker@2
    displayName: 'Build and Push Image'
    inputs:
      containerRegistry: 'your-registry'
      repository: 'techspherex-api'
      command: 'buildAndPush'
      Dockerfile: 'Dockerfile'
      tags: |
        $(Build.BuildId)
        latest
```

## Scaling Strategies

### Horizontal Scaling

The API is stateless (JWT auth, Redis cache) — ideal for horizontal scaling:

```yaml
# docker-compose scale
services:
  api:
    deploy:
      replicas: 3
```

For Kubernetes:
```yaml
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1
      maxSurge: 1
```

### Database Connection Pooling

For high-concurrency deployments, use PgBouncer:

```yaml
services:
  pgbouncer:
    image: edoburu/pgbouncer:latest
    environment:
      DATABASE_URL: "postgres://TechSpherex:password@postgres:5432/TechSpherex_cleanarch"
      MAX_CLIENT_CONN: 1000
      DEFAULT_POOL_SIZE: 20
    ports:
      - "6432:5432"
```

### Redis Clustering

For high-availability Redis:
```yaml
services:
  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes --maxmemory 256mb --maxmemory-policy allkeys-lru
```

## Environment Configuration

### Docker Compose Override

Create `docker-compose.override.yml` for environment-specific settings:

```yaml
services:
  api:
    environment:
      Jwt__Secret: ${JWT_SECRET}
      ConnectionStrings__DefaultConnection: ${DB_CONNECTION}
```

### .env File

```env
JWT_SECRET=your-super-secure-secret-key-at-least-32-chars
DB_CONNECTION=Host=postgres;Port=5432;Database=prod_db;Username=prod_user;Password=secure_pass
```

## Health Checks

Built-in health endpoints:

| Endpoint | Purpose |
|----------|---------|
| `/health` | Full health check (all dependencies) |
| `/alive` | Liveness probe (app is running) |

Use these for:
- **Kubernetes**: `livenessProbe` + `readinessProbe`
- **Load balancer**: Health check path
- **Docker**: `HEALTHCHECK` instruction
- **Monitoring**: Uptime monitoring
