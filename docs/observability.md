# Observability Guide

## Overview

The template provides **three levels** of observability:

| Level | Tool | Use Case |
|-------|------|----------|
| **Development** | Aspire Dashboard | Quick local telemetry |
| **ELK Stack** | Elasticsearch + Logstash + Kibana | Centralized log management |
| **Grafana Stack** | Loki + Grafana + Tempo + Prometheus | Full observability (logs, metrics, traces) |

## Level 1: Aspire Dashboard (Development)

Already built-in. When you run with Aspire:

```bash
cd src/TechSpherex.CleanArchitecture.AppHost
dotnet run
```

The Aspire Dashboard shows:
- **Structured logs** with filtering
- **Distributed traces** with request timeline
- **Metrics** (request rate, latency, etc.)
- **Resource health** (PostgreSQL, Redis, API)

## Level 2: ELK Stack

### Start ELK

```bash
docker compose --profile elk up -d
```

### Access

| Service | URL | Credentials |
|---------|-----|-------------|
| Kibana | http://localhost:5601 | None (dev mode) |
| Elasticsearch | http://localhost:9200 | None (dev mode) |

### Setup Kibana Index Pattern

1. Open Kibana → **Stack Management** → **Data Views**
2. Create data view: `techspherex-logs-*`
3. Set time field: `@timestamp`
4. Go to **Discover** to see your logs

### Useful KQL Queries

```
# All errors
log_level: "Error" OR log_level: "Fatal"

# Specific tenant
Properties.TenantId: "acme-corp"

# Slow requests (>500ms)
Properties.ElapsedMilliseconds > 500

# Authentication failures
MessageTemplate: *login* AND log_level: "Warning"

# Trace correlation
trace_id: "abc123def456"
```

### Serilog Configuration

The API ships logs to Elasticsearch via `Serilog.Sinks.Elasticsearch`:

```json
{
  "Serilog": {
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUris": "http://localhost:9200",
          "indexFormat": "techspherex-logs-{0:yyyy.MM.dd}",
          "autoRegisterTemplate": true
        }
      }
    ]
  }
}
```

## Level 3: Grafana Stack

### Start Grafana Stack

```bash
docker compose --profile grafana up -d
```

### Access

| Service | URL | Credentials |
|---------|-----|-------------|
| Grafana | http://localhost:3000 | admin / Admin@123 |
| Prometheus | http://localhost:9090 | None |
| Loki | http://localhost:3100 | None |
| Tempo | http://localhost:3200 | None |

### Pre-Built Dashboard

The template includes a **TechSpherex API Overview** dashboard with 12 panels:

| Panel | Type | Data Source |
|-------|------|-------------|
| Request Rate (req/s) | Time Series | Prometheus |
| P99 Latency | Time Series | Prometheus |
| Error Rate (5xx) | Stat | Prometheus |
| Active Connections | Stat | Prometheus |
| Health Status | Stat | Prometheus |
| Memory Usage | Time Series | Prometheus |
| Log Volume by Level | Time Series | Loki |
| Recent Logs | Logs | Loki |
| HTTP Status Distribution | Pie Chart | Prometheus |
| Thread Pool Queue | Time Series | Prometheus |
| GC Collections | Time Series | Prometheus |

### Correlating Logs, Traces, and Metrics

The Grafana datasources are pre-configured with cross-linking:

1. **Logs → Traces**: Click a TraceId in Loki logs to jump to the full trace in Tempo
2. **Traces → Logs**: View associated logs for any trace span
3. **Traces → Metrics**: See RED metrics for any traced service
4. **Service Map**: Visualize service dependencies in Tempo

### Useful LogQL Queries (Loki)

```logql
# All logs from API
{service="techspherex-api"}

# Error logs only
{service="techspherex-api"} |= "Error"

# Filter by tenant
{service="techspherex-api"} | json | TenantId="acme-corp"

# Count errors per minute
count_over_time({service="techspherex-api"} |= "Error" [1m])

# Top 10 slowest requests
{service="techspherex-api"} | json | ElapsedMilliseconds > 1000
```

### Useful PromQL Queries (Prometheus)

```promql
# Request rate
rate(http_server_request_duration_seconds_count[5m])

# P99 latency
histogram_quantile(0.99, rate(http_server_request_duration_seconds_bucket[5m]))

# Error rate percentage
sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]))
/ sum(rate(http_server_request_duration_seconds_count[5m])) * 100

# .NET GC heap size
process_runtime_dotnet_gc_heap_size_bytes

# Active HTTP connections
http_server_active_requests
```

## OpenTelemetry Configuration

The `ServiceDefaults` project configures OpenTelemetry:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource(builder.Environment.ApplicationName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });
```

The OTLP exporter sends telemetry to the OpenTelemetry Collector when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.

## Alerting (Production)

### Grafana Alerts

In Grafana, create alert rules for:

| Alert | Condition | Severity |
|-------|-----------|----------|
| High Error Rate | 5xx rate > 5% for 5min | Critical |
| High Latency | P99 > 2s for 5min | Warning |
| Service Down | up == 0 for 1min | Critical |
| High Memory | Heap > 80% limit for 10min | Warning |

### Prometheus Alertmanager

For production, add Alertmanager to your Prometheus config for Slack/PagerDuty/email notifications.

## Choosing the Right Stack

| Factor | ELK | Grafana |
|--------|-----|---------|
| **Primary focus** | Log search & analysis | Full observability (logs + metrics + traces) |
| **Resource usage** | High (ES is memory-hungry) | Low (Loki is log-append only) |
| **Query language** | KQL (simple) | LogQL + PromQL (powerful) |
| **Trace support** | Basic (via APM) | Excellent (Tempo) |
| **Dashboard** | Kibana | Grafana (more flexible) |
| **Best for** | Log-heavy workloads | Microservices, cloud-native |

**Recommendation**: Use **Grafana Stack** for new projects. Use **ELK** if your team already has ELK expertise.
