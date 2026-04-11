# Caching Operations Guide

[Tiếng Việt](CACHING_OPERATIONS.md) | [English](CACHING_OPERATIONS.en.md)


This guide covers runtime expectations when `GenerateCachingDecorators = true`.

## Runtime prerequisites

- Register a distributed cache provider (`AddDistributedMemoryCache` for dev or Redis in production).
- Ensure generated DI wiring is enabled via `AddRynorArchDependencies()`.
- Verify cache keys are stable across deployments.

## Default generated behavior

- Read query responses are cached by generated query cache behaviors.
- Per-entity invalidation interfaces are generated.
- Create/update/delete handlers call generated invalidators when DI wiring is active.

## Production guardrails

- Set explicit TTL policy per entity/query (hot reads vs cold reads).
- Add jitter to TTL for high-traffic keys to reduce synchronized expirations.
- Use namespaced keys (`service:entity:id`) to prevent collisions.
- Monitor cache hit ratio, evictions, and backend latency.

## Common rollout pattern

1. Start with distributed memory cache in lower environments.
2. Promote to Redis with the same key schema.
3. Add dashboards and alerts before enabling in high-traffic modules.

## Troubleshooting quick checks

- Verify `Get{Entity}ByIdQueryCacheBehavior.g.cs` was generated.
- Ensure DI extension registers both invalidator and cache pipeline behavior.
- Confirm write handlers execute invalidation paths in integration tests.
