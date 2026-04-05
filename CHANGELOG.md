# Changelog

All notable changes to the TechSpherex Clean Architecture Template will be documented in this file.

## [2.0.0] - 2026-04-05

### 🐳 Docker Production Support
- Multi-stage Dockerfile with Alpine base (JIT + Native AOT builds)
- `.dockerignore` for optimized build context
- Full `docker-compose.yml` with profile-based service management
- Non-root user, health checks, layer caching

### ⚡ Native AOT Support
- Conditional AOT publish configuration in Api.csproj
- Source-generated JSON serializer documentation
- AOT compatibility guide with EF Core migration strategies

### 🤖 Skill Agents Pattern
- `ISkillAgent` interface for pluggable AI capabilities
- `IAgentOrchestrator` for prompt routing (keyword-based, LLM-ready)
- `AgentContext` with conversation history, user/tenant context
- `AgentResult` with 4 status levels (Success, Failure, NeedsMoreInfo, PartialSuccess)
- Sample `TodoAgentSkill` — CRUD via natural language
- API endpoints: `POST /api/agents/execute`, `GET /api/agents/skills`
- Auto-discovery via assembly scanning (zero manual registration)

### 🏢 Multi-Tenancy
- `ITenantEntity` marker interface with automatic global query filters
- `TenantMiddleware` for request-scoped tenant resolution
- `TenantProvider` — resolves from X-Tenant-Id header → JWT claim → default
- Automatic `TenantId` assignment on entity creation
- Serilog log context enrichment with TenantId
- `TenantInfo` record with support for per-tenant connection strings

### 📊 ELK Stack (Profile: elk)
- Elasticsearch 8.17 single-node setup
- Logstash pipeline with JSON parsing, trace correlation, tenant extraction
- Kibana 8.17 for log visualization
- `Serilog.Sinks.Elasticsearch` integration

### 📈 Grafana Stack (Profile: grafana)
- Grafana 11.5 with auto-provisioned datasources
- Loki 3.4 for log aggregation (lighter than Elasticsearch)
- Tempo 2.7 for distributed tracing
- Prometheus 3.2 for metrics scraping
- Pre-built dashboard: 12 panels (request rate, P99, errors, memory, GC, logs)
- Cross-linked datasources: Loki ↔ Tempo ↔ Prometheus

### 🔧 OpenTelemetry Collector
- Unified collector config for both ELK and Grafana backends
- OTLP gRPC + HTTP receivers
- Routes to Tempo (traces), Loki (logs), Prometheus (metrics)

### 📚 Comprehensive Documentation
- `docs/getting-started.md` — Clone to deploy
- `docs/architecture.md` — Layer deep-dive with diagrams
- `docs/adding-features.md` — Complete Product feature example
- `docs/docker.md` — All profiles, K8s deployment hints
- `docs/aot.md` — AOT guide with EF Core strategies
- `docs/multi-tenancy.md` — Tenant setup, cross-tenant queries
- `docs/skill-agents.md` — Agent architecture, LLM integration
- `docs/observability.md` — ELK, Grafana, query examples
- `docs/deployment.md` — Production checklist, CI/CD pipelines

### 🔄 Improvements
- Fixed API docs branding (now TechSpherex throughout)
- PostgreSQL with Alpine image (smaller)
- Redis with data persistence volume
- Docker health checks for PostgreSQL and Redis
- Separate Docker networks for service isolation

## [1.0.0] - 2026-03-25

### Initial Release

- Clean Architecture: Domain, Application, Infrastructure, Api
- .NET 10 / C# 14 with `.slnx` solution format
- Manual CQRS (ICommand, IQuery, handlers) — zero commercial dependencies
- Minimal APIs with TypedResults
- FluentValidation 12 + Result pattern + ProblemDetails (RFC 9457)
- EF Core 10 + PostgreSQL
- Microsoft HybridCache (L1 in-memory + L2 Redis)
- ASP.NET Identity + JWT authentication with refresh tokens
- Scalar API documentation (modern OpenAPI UI)
- Serilog 10 structured logging
- .NET Aspire 13 + OpenTelemetry (traces, metrics, logs)
- Global exception handler
- Database seeder (admin user + sample todos)
- Central Package Management
- Docker Compose for standalone usage
- Architecture tests (NetArchTest — 9 tests)
- Unit tests (xUnit v3 + NSubstitute + FluentAssertions — 8 tests)
