# Getting Started

## Prerequisites

| Tool | Version | Required |
|------|---------|----------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0+ | ✅ |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | 4.x+ | ✅ |
| IDE (VS 2022 / Rider / VS Code) | Latest | Recommended |

## 1. Clone the Repository

```bash
git clone https://github.com/KhaiTrang1995/clean-architecture-template.git
cd clean-architecture-template
```

## 2. Choose a Run Mode

### Option A: Aspire (Recommended for Development)

Aspire orchestrates all dependencies (PostgreSQL, Redis) automatically:

```bash
cd src/TechSpherex.CleanArchitecture.AppHost
dotnet run
```

This starts:
- **PostgreSQL** database with pgAdmin
- **Redis** cache with RedisInsight  
- **API** with auto-migration and seed data
- **Aspire Dashboard** for OpenTelemetry

### Option B: Docker Compose (Standalone)

```bash
# Start core infrastructure
docker compose up -d

# Run the API
cd src/TechSpherex.CleanArchitecture.Api
dotnet run
```

### Option C: Full Docker (API + Infrastructure)

```bash
# Build and start everything
docker compose up -d --build

# With ELK Stack for log monitoring
docker compose --profile elk up -d --build

# With Grafana Stack for observability
docker compose --profile grafana up -d --build
```

## 3. Explore the API

Open **Scalar API docs**: `https://localhost:7200/scalar/v1`

### Default Credentials

| User | Email | Password |
|------|-------|----------|
| Admin | `admin@TechSpherex.dev` | `Admin@123` |

### Get a JWT Token

```bash
curl -X POST https://localhost:7200/api/identity/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@TechSpherex.dev", "password": "Admin@123"}'
```

### Test an Endpoint

```bash
curl https://localhost:7200/api/todos \
  -H "Authorization: Bearer <your-token>"
```

## 4. Run Tests

```bash
cd src
dotnet test TechSpherex.CleanArchitecture.slnx
```

### Test Coverage

| Test Suite | Count | Description |
|-----------|-------|-------------|
| Architecture Tests | 9 | Dependency rule enforcement |
| Application Unit Tests | 8 | Handler logic validation |

## 5. Configure for Your Project

### Rename the Template

1. Find & replace `TechSpherex.CleanArchitecture` with your namespace (e.g., `MyCompany.MyApp`)
2. Rename project folders accordingly
3. Update `Directory.Build.props` and solution file

### Change the JWT Secret

In `appsettings.json`:
```json
{
  "Jwt": {
    "Secret": "YOUR-UNIQUE-SECRET-KEY-AT-LEAST-32-CHARS-LONG"
  }
}
```

> ⚠️ **Never commit real secrets!** Use User Secrets or environment variables in production.

### Configure Database

The connection string is managed by Aspire. For standalone mode, update docker-compose or set the `ConnectionStrings__DefaultConnection` environment variable.

## Next Steps

- 📖 [Architecture Guide](architecture.md) — Understand the layer structure
- 🔧 [Adding Features](adding-features.md) — Build your first feature
- 🐳 [Docker Guide](docker.md) — Production deployment
- 🔍 [Observability](observability.md) — Monitoring and logging
