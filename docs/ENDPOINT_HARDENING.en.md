# Endpoint Hardening Guide

[Tiếng Việt](ENDPOINT_HARDENING.md) | [English](ENDPOINT_HARDENING.en.md)


Generated endpoints are intentionally minimal. Use this checklist before production rollout.

## 1. Add authorization boundaries

- Apply `RequireAuthorization()` per route group or endpoint.
- Split read and write policies (for example `Trips.Read`, `Trips.Write`).
- Enforce tenant constraints in handlers, not only in endpoint filters.

## 2. Return consistent problem details

- Configure `UseExceptionHandler()` and `AddProblemDetails()` globally.
- Map domain and validation exceptions to RFC 7807 responses.
- Keep a stable error contract for client teams.

## 3. Make not-found and conflict semantics explicit

- Preserve `404` for missing resources.
- Return `409` for optimistic concurrency or invariant conflicts.
- Return `422` when validation rules fail after request binding.

## 4. Validate request contracts at the edge

- Keep FluentValidation in handlers and add endpoint-level payload checks where needed.
- Reject oversized payloads and unsupported content types.
- Prefer explicit DTO contracts over exposing EF entities directly.

## 5. Add observability

- Add structured logs for route, entity id, tenant id, and correlation id.
- Emit traces around each endpoint and handler execution.
- Track metrics for p95 latency and non-2xx rates per route.

## 6. Harden API lifecycle

- Add API versioning strategy before external consumers onboard.
- Use idempotency keys for create/update flows that can be retried.
- Add integration tests for all generated route paths.

## Recommended startup pattern

```csharp
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapGroup("/api")
   .RequireAuthorization()
   .MapRynorArchEndpoints();
```
