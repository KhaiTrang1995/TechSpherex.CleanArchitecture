# Docker Guide

## Overview

The template supports three Docker deployment modes:

| Mode | Command | What's Included |
|------|---------|-----------------|
| **Core** | `docker compose up -d` | API + PostgreSQL + Redis |
| **Core + ELK** | `docker compose --profile elk up -d` | + Elasticsearch + Logstash + Kibana |
| **Core + Grafana** | `docker compose --profile grafana up -d` | + Loki + Grafana + Tempo + Prometheus |
| **Core + Tools** | `docker compose --profile tools up -d` | + pgAdmin + RedisInsight |

## Building the Docker Image

### Standard Build (JIT)

```bash
docker build -t techspherex-api .
```

### Native AOT Build

```bash
docker build --build-arg PUBLISH_AOT=true -t techspherex-api:aot .
```

### Image Sizes

| Build | Approximate Size |
|-------|-----------------|
| JIT (Alpine) | ~120 MB |
| AOT (Alpine) | ~35 MB |

## Docker Compose Profiles

### Core (Always Running)

```bash
docker compose up -d
```

Services:
- `techspherex-api` → `http://localhost:8080`
- `techspherex-postgres` → `localhost:5432`
- `techspherex-redis` → `localhost:6379`

### ELK Stack

```bash
docker compose --profile elk up -d
```

Additional services:
- `techspherex-elasticsearch` → `http://localhost:9200`
- `techspherex-logstash` → `localhost:5044` (Beats), `localhost:31311` (TCP)
- `techspherex-kibana` → `http://localhost:5601`
- `techspherex-otel-collector` → `localhost:4320` (gRPC), `localhost:4321` (HTTP)

### Grafana Stack

```bash
docker compose --profile grafana up -d
```

Additional services:
- `techspherex-grafana` → `http://localhost:3000` (admin / Admin@123)
- `techspherex-loki` → `http://localhost:3100`
- `techspherex-tempo` → `http://localhost:3200`
- `techspherex-prometheus` → `http://localhost:9090`
- `techspherex-otel-collector` → `localhost:4320` (gRPC), `localhost:4321` (HTTP)

### Tools

```bash
docker compose --profile tools up -d
```

Additional services:
- `techspherex-pgadmin` → `http://localhost:5050` (admin@techspherex.dev / Admin@123)
- `techspherex-redisinsight` → `http://localhost:5540`

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | Docker internal |
| `Jwt__Secret` | JWT signing key | ⚠️ Change this! |
| `Jwt__Issuer` | JWT issuer | `TechSpherex.CleanArchitecture` |
| `Jwt__Audience` | JWT audience | `TechSpherex.CleanArchitecture` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry collector | `http://otel-collector:4317` |

## Kubernetes Deployment Hints

### Minimal Deployment YAML

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: techspherex-api
spec:
  replicas: 2
  selector:
    matchLabels:
      app: techspherex-api
  template:
    metadata:
      labels:
        app: techspherex-api
    spec:
      containers:
        - name: api
          image: techspherex-api:latest
          ports:
            - containerPort: 8080
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: techspherex-secrets
                  key: db-connection
          livenessProbe:
            httpGet:
              path: /alive
              port: 8080
            initialDelaySeconds: 10
          readinessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 5
          resources:
            requests:
              cpu: "100m"
              memory: "128Mi"
            limits:
              cpu: "500m"
              memory: "512Mi"
---
apiVersion: v1
kind: Service
metadata:
  name: techspherex-api
spec:
  selector:
    app: techspherex-api
  ports:
    - port: 80
      targetPort: 8080
  type: ClusterIP
```

## Docker Best Practices

1. **Never commit secrets** — Use Docker secrets or Kubernetes secrets
2. **Use health checks** — The Dockerfile includes `HEALTHCHECK`
3. **Non-root user** — The container runs as `appuser` for security
4. **Layer caching** — Project files are copied before source for efficient builds
5. **Alpine base** — Minimal attack surface and smaller images
