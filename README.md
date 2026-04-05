<p align="center">
  <h1 align="center">🏗️ TechSpherex Clean Architecture Template</h1>
  <p align="center">
    A production-ready <strong>Clean Architecture</strong> starter template for <strong>.NET 10</strong>
    <br />
    <em>Ship features from day one — not boilerplate.</em>
  </p>
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com"><img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" /></a>
  <a href="https://www.docker.com"><img src="https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker" /></a>
  <a href="docs/aot.md"><img src="https://img.shields.io/badge/Native_AOT-Supported-FF6F00?style=for-the-badge&logo=dotnet&logoColor=white" alt="AOT" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="MIT License" /></a>
</p>

<p align="center">
  <a href="https://github.com/KhaiTrang1995"><img src="https://img.shields.io/badge/C%23-14-239120?style=flat-square&logo=csharp" alt="C#" /></a>
  <a href="#"><img src="https://img.shields.io/badge/Aspire-13-6C3483?style=flat-square" alt="Aspire" /></a>
  <a href="#"><img src="https://img.shields.io/badge/EF_Core-10-6C3483?style=flat-square" alt="EF Core" /></a>
  <a href="#"><img src="https://img.shields.io/badge/xUnit-v3-5E81AC?style=flat-square" alt="xUnit" /></a>
  <a href="#"><img src="https://img.shields.io/badge/Version-2.0.0-blue?style=flat-square" alt="v2.0.0" /></a>
</p>

---

## ✨ What's Included

<table>
  <tr>
    <td>🏛️ <strong>Clean Architecture</strong><br/><sub>4-layer separation with enforced dependency rules</sub></td>
    <td>⚡ <strong>Native AOT</strong><br/><sub>~35 MB images, ~50ms startup</sub></td>
    <td>🐳 <strong>Docker Production</strong><br/><sub>Multi-stage build, Alpine, non-root</sub></td>
  </tr>
  <tr>
    <td>🏢 <strong>Multi-Tenancy</strong><br/><sub>Shared-table with auto query filters</sub></td>
    <td>🤖 <strong>Skill Agents (AI)</strong><br/><sub>Pluggable LLM orchestrator pattern</sub></td>
    <td>📊 <strong>ELK Stack</strong><br/><sub>Elasticsearch + Logstash + Kibana</sub></td>
  </tr>
  <tr>
    <td>📈 <strong>Grafana Stack</strong><br/><sub>Loki + Tempo + Prometheus + Grafana</sub></td>
    <td>🔐 <strong>JWT Auth</strong><br/><sub>Identity + refresh tokens + role-based</sub></td>
    <td>🚀 <strong>HybridCache</strong><br/><sub>L1 in-memory + L2 Redis with stampede protection</sub></td>
  </tr>
</table>

---

## 🛠️ Tech Stack

| Layer | Technology |
|:------|:-----------|
| **Architecture** | Clean Architecture (Domain → Application → Infrastructure → Api) |
| **Runtime** | .NET 10 / C# 14 |
| **API** | Minimal APIs with `TypedResults` |
| **CQRS** | Manual handlers — zero dependencies, zero licensing risk |
| **Validation** | FluentValidation 12 + Result pattern |
| **Error Handling** | `ProblemDetails` (RFC 9457) + global exception handler |
| **Database** | EF Core 10 + PostgreSQL |
| **Caching** | Microsoft `HybridCache` (L1 in-memory + L2 Redis) |
| **Auth** | ASP.NET Identity + JWT Bearer with refresh tokens |
| **Multi-Tenancy** | Shared-table strategy with EF Core global query filters |
| **AI / Agents** | Skill Agents pattern (pluggable — OpenAI, Ollama, Semantic Kernel) |
| **API Docs** | Scalar (modern OpenAPI UI) |
| **Logging** | Serilog 10 + Elasticsearch sink |
| **Observability** | .NET Aspire 13 + OpenTelemetry + ELK + Grafana |
| **Containerization** | Docker multi-stage build (JIT + Native AOT) |
| **Testing** | xUnit v3 + FluentAssertions + NSubstitute + NetArchTest |
| **Solution** | `.slnx` format + Central Package Management |

---

## 🏛️ Architecture

```
┌──────────────────────────────────────────────────┐
│                    Api Layer                      │
│      Endpoints · Middleware · OpenAPI · Scalar    │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│              Infrastructure Layer                 │
│    EF Core · Identity · JWT · Cache · Tenancy    │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│              Application Layer                    │
│  CQRS Handlers · Validators · Agent Abstractions │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│                Domain Layer                       │
│   Entities · Value Objects · Result · Interfaces  │
└──────────────────────────────────────────────────┘
```

> **Dependency rule:** Each layer only depends on the layer below it. Domain has **zero** external dependencies. Architecture tests enforce this at build time (9 tests).

---

## 📁 Project Structure

```
clean-architecture-template/
├── src/
│   ├── Domain/                    # Entities, value objects, ITenantEntity
│   ├── Application/               # CQRS handlers, validators, Skill Agents
│   ├── Infrastructure/            # EF Core, Identity, JWT, caching, tenancy
│   ├── Api/                       # Minimal API endpoints, Scalar, middleware
│   ├── AppHost/                   # .NET Aspire orchestration
│   └── ServiceDefaults/           # OpenTelemetry, health checks, resilience
├── tests/
│   ├── Architecture.Tests/        # Dependency rule enforcement (9 tests)
│   └── Application.UnitTests/     # Handler unit tests (8 tests)
├── docker/
│   ├── elk/                       # Logstash pipeline config
│   ├── grafana/                   # Loki, Tempo, Prometheus, dashboards
│   └── otel/                      # OpenTelemetry Collector config
├── docs/                          # 9 comprehensive documentation guides
├── Dockerfile                     # Multi-stage build (JIT + AOT)
├── docker-compose.yml             # Full stack with 4 profiles
├── Directory.Build.props          # .NET 10, C# latest, nullable
├── Directory.Packages.props       # Central Package Management
├── CHANGELOG.md                   # Version history
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version | Required |
|:-----|:--------|:--------:|
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0+ | ✅ |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | 4.x+ | ✅ |
| IDE (VS 2026 / Rider / VS Code) | Latest | 💡 |

### Option A — Run with Aspire _(recommended for development)_

```bash
cd src/TechSpherex.CleanArchitecture.AppHost
dotnet run
```

This automatically starts:
- **PostgreSQL** database with pgAdmin
- **Redis** cache with RedisInsight
- **API** with auto-migration and seed data
- **Aspire Dashboard** for OpenTelemetry (traces, metrics, logs)

### Option B — Run with Docker Compose

> ⚠️ **Important:** All `docker compose` and `docker build` commands must be run from the **project root directory** (where `Dockerfile` and `docker-compose.yml` are located), **not** from `src/`.

```bash
# Navigate to project root
cd clean-architecture-template

# Core: API + PostgreSQL + Redis
docker compose up -d --build

# Or explicitly specify the compose file
docker compose -f docker-compose.yml up -d --build

# + ELK Stack (Elasticsearch, Logstash, Kibana)
docker compose --profile elk up -d --build

# + Grafana Stack (Loki, Grafana, Tempo, Prometheus)
docker compose --profile grafana up -d --build

# + Dev Tools (pgAdmin, RedisInsight)
docker compose --profile tools up -d

# Combine multiple profiles
docker compose --profile elk --profile tools up -d --build
```

### Option C — Standalone (without Aspire)

```bash
docker compose up -d postgres redis    # Start database & cache
cd src/Api && dotnet run               # Run the API locally
```

### 🔗 Explore the API

| Service | URL | Credentials |
|:--------|:----|:------------|
| **Scalar API Docs** | `https://localhost:7200/scalar/v1` | — |
| **API (Docker)** | `http://localhost:8080` | — |
| **Aspire Dashboard** | `https://localhost:18888` | — |

**Default admin credentials** _(seeded automatically in Development)_:
- **Email:** `admin@TechSpherex.dev`
- **Password:** `Admin@123`

### Run Tests

```bash
cd src
dotnet build TechSpherex.CleanArchitecture.slnx
dotnet test TechSpherex.CleanArchitecture.slnx
```

---

## 🐳 Docker & Native AOT

### Build Docker Image

> Run from the **project root** directory (same level as `Dockerfile`):

```bash
# Standard build (JIT)
docker build -t techspherex-api .

# Native AOT build (smaller image, faster cold start)
docker build --build-arg PUBLISH_AOT=true -t techspherex-api:aot .
```

| Build | Image Size | Startup | Use Case |
|:------|:-----------|:--------|:---------|
| JIT (Alpine) | ~120 MB | ~500ms | General purpose, full compatibility |
| AOT (Alpine) | ~35 MB | ~50ms | Serverless, edge, cold-start-sensitive |

### Docker Compose Profiles

| Profile | Command | Services Added |
|:--------|:--------|:---------------|
| _(core)_ | `docker compose up -d` | API + PostgreSQL + Redis |
| `elk` | `docker compose --profile elk up -d` | + Elasticsearch + Logstash + Kibana |
| `grafana` | `docker compose --profile grafana up -d` | + Loki + Grafana + Tempo + Prometheus |
| `tools` | `docker compose --profile tools up -d` | + pgAdmin + RedisInsight |

📖 [Full Docker Guide](docs/docker.md) · [Native AOT Guide](docs/aot.md)

---

## 🏢 Multi-Tenancy

Shared-table multi-tenancy with **automatic tenant isolation** powered by EF Core global query filters.

```bash
# Send request with tenant header
curl -H "X-Tenant-Id: acme-corp" \
     -H "Authorization: Bearer <token>" \
     http://localhost:8080/api/todos
```

**Tenant resolution order:** `X-Tenant-Id` header → JWT `tenant_id` claim → Default tenant

### Make Any Entity Tenant-Aware

Simply implement `ITenantEntity` — global query filters and auto-assignment are applied automatically:

```csharp
public sealed class Product : AuditableEntity, ITenantEntity
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string TenantId { get; set; } = default!; // Auto-set on SaveChanges
}
```

📖 [Multi-Tenancy Guide](docs/multi-tenancy.md)

---

## 🤖 Skill Agents (AI)

Built-in **provider-agnostic** AI agent pattern that integrates with your domain logic:

```bash
curl -X POST http://localhost:8080/api/agents/execute \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Show me all my todos"}'
```

| Endpoint | Method | Auth | Description |
|:---------|:-------|:----:|:------------|
| `/api/agents/execute` | POST | 🔒 | Execute with auto skill routing |
| `/api/agents/execute/{skillId}` | POST | 🔒 | Execute a specific skill by ID |
| `/api/agents/skills` | GET | 🌐 | List all available skills |

**Key features:**
- 🔌 **Provider-agnostic** — plug in OpenAI, Azure OpenAI, Ollama, or Semantic Kernel
- 🔍 **Auto-discovery** — skill agents are registered automatically via assembly scanning
- 💬 **Multi-turn** — conversation history support for contextual interactions
- 📦 **Sample included** — `TodoAgentSkill` demonstrates the full pattern

📖 [Skill Agents Guide](docs/skill-agents.md)

---

## 📊 Observability

Three levels of observability, from development to production:

### Level 1 — Aspire Dashboard _(Development)_

Built-in with `.NET Aspire`. Shows structured logs, distributed traces, and metrics out of the box.

### Level 2 — ELK Stack

```bash
docker compose --profile elk up -d --build
```

| Service | URL | Purpose |
|:--------|:----|:--------|
| **Kibana** | http://localhost:5601 | Log visualization & search |
| **Elasticsearch** | http://localhost:9200 | Log storage & indexing |
| **Logstash** | tcp://localhost:31311 | Log pipeline processing |

- Index pattern: `techspherex-logs-*`
- Structured JSON logs with trace correlation and tenant context

### Level 3 — Grafana Stack _(Recommended for Production)_

```bash
docker compose --profile grafana up -d --build
```

| Service | URL | Credentials |
|:--------|:----|:------------|
| **Grafana** | http://localhost:3000 | admin / Admin@123 |
| **Prometheus** | http://localhost:9090 | — |
| **Loki** | http://localhost:3100 | — |
| **Tempo** | http://localhost:3200 | — |

**Pre-built dashboard** with 12 panels:
- Request rate · P99 latency · Error rate (5xx) · Active connections
- Memory usage · GC collections · Thread pool · Log volume
- HTTP status distribution · Recent logs

**Cross-linked datasources:** Loki (logs) ↔ Tempo (traces) ↔ Prometheus (metrics)

📖 [Observability Guide](docs/observability.md)

---

## 🗂️ Service Ports Reference

| Service | Port | Profile |
|:--------|:-----|:--------|
| API | `8080` | core |
| PostgreSQL | `5432` | core |
| Redis | `6379` | core |
| Elasticsearch | `9200` | elk |
| Logstash (Beats / TCP) | `5044` / `31311` | elk |
| Kibana | `5601` | elk |
| Grafana | `3000` | grafana |
| Loki | `3100` | grafana |
| Tempo | `3200` | grafana |
| Prometheus | `9090` | grafana |
| OTel Collector (gRPC / HTTP) | `4320` / `4321` | elk / grafana |
| pgAdmin | `5050` | tools |
| RedisInsight | `5540` | tools |

---

## 📝 Sample: Todos Feature

The template includes a complete **Todos** CRUD feature as a reference implementation:

| Endpoint | Method | Auth | Description |
|:---------|:-------|:----:|:------------|
| `/api/todos` | GET | 🔒 | Get all todos (paginated) |
| `/api/todos/{id}` | GET | 🔒 | Get a todo by ID |
| `/api/todos` | POST | 🔒 | Create a new todo |
| `/api/todos/{id}` | PUT | 🔒 | Update a todo |
| `/api/todos/{id}/complete` | PATCH | 🔒 | Mark as completed |
| `/api/todos/{id}` | DELETE | 🔒 | Delete a todo |

### Adding a New Feature

Follow the Todos pattern — four simple steps:

1. **Domain** — Add entity in `Domain/Entities/` (implement `ITenantEntity` if multi-tenant)
2. **Application** — Create feature folder in `Application/Features/YourFeature/` with Command/Query + Handler + Validator
3. **Infrastructure** — Add EF Core configuration in `Infrastructure/Persistence/Configurations/`
4. **Api** — Add endpoint group in `Api/Endpoints/` and register in `Program.cs`

📖 [Adding Features Guide](docs/adding-features.md) — includes a complete **Product** feature walkthrough

---

## 🧠 Key Design Decisions

| Decision | Why |
|:---------|:----|
| **Manual CQRS** over MediatR | Zero licensing risk — MediatR is commercial since v13. Learn the pattern, not a library. |
| **Scalar** over Swagger UI | Modern, faster, better DX. Swagger UI is legacy. |
| **HybridCache** over IMemoryCache | Built-in stampede protection, L1+L2 cache layers, automatic serialization. |
| **Result pattern** over exceptions | Explicit error handling, no hidden control flow, better API contracts. |
| **Shared-table tenancy** | Simple, no migration complexity, cost-efficient — good for most SaaS apps. |
| **Interface-only agents** | No LLM provider lock-in. Swap OpenAI, Ollama, or Semantic Kernel freely. |
| **Assembly scanning DI** over Scrutor | Zero dependencies — 40 lines of reflection replaces an entire NuGet package. |
| **.slnx** over .sln | XML-based, merge-friendly, smaller — the future of .NET solution files. |

---

## 📚 Documentation

| Document | Description |
|:---------|:------------|
| 📘 [Getting Started](docs/getting-started.md) | Clone, configure, and run the template |
| 🏛️ [Architecture](docs/architecture.md) | Layer structure, dependency rules, CQRS pattern |
| 🔧 [Adding Features](docs/adding-features.md) | Step-by-step new feature guide (with Product example) |
| 🐳 [Docker](docs/docker.md) | Build images, profiles, Kubernetes hints |
| ⚡ [Native AOT](docs/aot.md) | AOT compilation, limitations, EF Core strategies |
| 🏢 [Multi-Tenancy](docs/multi-tenancy.md) | Tenant isolation, resolution, cross-tenant queries |
| 🤖 [Skill Agents](docs/skill-agents.md) | AI agent pattern, LLM integration guide |
| 📊 [Observability](docs/observability.md) | ELK, Grafana, OpenTelemetry, query examples |
| 🚢 [Deployment](docs/deployment.md) | Production checklist, CI/CD, scaling strategies |

---

## ❓ Troubleshooting

<details>
<summary><strong>API crashes with "Cannot resolve scoped service ITenantProvider from root provider"</strong></summary>

This is fixed in v2.0.0. The `AppDbContext` now resolves `ITenantProvider` via `DbContext.GetInfrastructure()` instead of constructor injection, which is compatible with Aspire's DbContext pooling.

</details>

<details>
<summary><strong>Docker build fails with "no such file or directory: Dockerfile"</strong></summary>

You must run `docker build` from the **project root** (where `Dockerfile` is located), not from `src/`:

```bash
cd clean-architecture-template    # ← project root
docker build -t techspherex-api .
```

</details>

<details>
<summary><strong>Redis/PostgreSQL connection fails in Docker Compose</strong></summary>

Ensure the connection string env vars match the Aspire service names in `Program.cs`:

```yaml
ConnectionStrings__TechSpherex-db: "Host=postgres;Port=5432;..."
ConnectionStrings__TechSpherex-cache: "redis:6379"
```

> Use Docker **service names** (`postgres`, `redis`), not container names.

</details>

<details>
<summary><strong>ELK/Grafana services not starting</strong></summary>

These are opt-in profiles. You must explicitly include them:

```bash
docker compose --profile elk up -d          # Start ELK
docker compose --profile grafana up -d      # Start Grafana
```

</details>

---

## 🤝 Contributing

Contributions are welcome! Feel free to:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

Please ensure:
- Code follows Clean Architecture dependency rules
- Architecture tests pass (`dotnet test`)
- New features include documentation updates

---

## 👤 About

Built by [**TechSpherex**](https://TechSpherex.com) — helping developers build production-ready .NET applications.

<p>
  <a href="https://www.linkedin.com/in/trang-dang-khai-techspherex"><img src="https://img.shields.io/badge/LinkedIn-Connect-0A66C2?style=flat-square&logo=linkedin" alt="LinkedIn" /></a>
  <a href="https://youtube.com/@TechSphereX-AI"><img src="https://img.shields.io/badge/YouTube-Subscribe-FF0000?style=flat-square&logo=youtube" alt="YouTube" /></a>
  <a href="https://github.com/KhaiTrang1995"><img src="https://img.shields.io/badge/GitHub-Follow-181717?style=flat-square&logo=github" alt="GitHub" /></a>
  <a href="https://TechSpherex.com/newsletter"><img src="https://img.shields.io/badge/Newsletter-Subscribe-8B5CF6?style=flat-square&logo=substack" alt="Newsletter" /></a>
</p>

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

Use it, modify it, ship it. Attribution appreciated but not required.

---

<p align="center">
  <sub>⭐ If you find this template useful, please give it a star — it helps others discover it!</sub>
</p>
