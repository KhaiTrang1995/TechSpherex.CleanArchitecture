# TechSpherex Clean Architecture Template

A Clean Architecture starter template for **.NET 11 / C# 15**. Ship features from day one — not boilerplate.

## Install and create a project

```
dotnet new install TechSpherex.CleanArchitecture.Template
dotnet new ts-clean-arch -n MyCompany.MyApp
```

Requires the [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0).

## Run

```
cd MyCompany.MyApp
dotnet run --project src/AppHost
```

On Windows, `.\scripts\start.ps1 -InMemory` starts the API and the Aspire dashboard with an in-memory database — no Docker needed.

## What's included

- **Clean Architecture** — Domain, Application, Infrastructure and Api, with architecture tests that enforce the dependency rules
- **Minimal APIs + gRPC** over the same manual CQRS handlers (no mediator library)
- **Result pattern** with a C# 15 closed `Error` hierarchy and ProblemDetails
- **EF Core + PostgreSQL**, ASP.NET Core Identity, JWT with refresh tokens
- **HybridCache** (in-process L1 + Redis L2)
- **Aspire + OpenTelemetry**, optional ELK and Grafana stacks via Docker Compose
- **Docker** multi-stage Alpine image with a Native AOT option
- Multi-tenancy scaffold, skill-agent pattern, config-driven rule engine
- xUnit v3, NetArchTest, SonarAnalyzer

## Links

- Documentation and source: https://github.com/KhaiTrang1995/TechSpherex.CleanArchitecture
- Changelog: https://github.com/KhaiTrang1995/TechSpherex.CleanArchitecture/blob/main/CHANGELOG.md

Released under the MIT License.
