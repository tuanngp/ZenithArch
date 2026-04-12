# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

SemVer baseline decision:
- Published versions before 1.0.8 are treated as legacy migration history.
- Formal SemVer governance starts at 1.0.8 and is enforced for all future releases.
- Existing published versions remain immutable and will not be altered or removed.

## [Unreleased]

### Added
- NuGet migration baseline: centralized SemVer governance beginning at 1.0.8.
- XML documentation enforcement for public Abstractions API with CS1591 regression guard.
- XML documentation enforcement for public Generator API models with CS1591 regression guard.
- Characterization and public API tests for Abstractions behavior.
- Local-feed NuGet package integration test project to validate packed artifacts before release.

### Changed
- Package metadata hardening: package icon, release notes normalization, deterministic symbol package settings.
- README standardized to a consumer-first package format with API and configuration coverage.
- CI release gates updated so publish runs only on version tags and fails if NuGet API key is missing.

### Fixed
- Changelog structure aligned to Keep a Changelog with dated historical entries.

### Security
- No security fixes in this cycle.

## [1.0.7] - 2026-04-11

### Added
- Automated generator regression tests for diagnostics and generated outputs.
- GitHub Actions CI and tag-based release workflows.
- Troubleshooting, compatibility, upgrade, contribution, and security documentation.
- New enterprise operation docs: endpoint hardening, caching operations, and profile migration guidance.
- AI-agent playbook documenting deterministic task contracts and verification rules.
- New CLI `doctor` command for readiness checks with actionable fix steps.
- E2E smoke test project for CLI workflow validation and CI/release quality gates.
- Runtime integration test suite (`tests/RynorArch.Integration.Tests`) covering CRUD, soft-delete, audit stamping, validation gating, transaction rollback, and cache invalidation semantics on SQLite in-memory.
- Runtime validation guide: `docs/RUNTIME_TESTING.md`.
- Endpoint E2E semantics tests (`tests/RynorArch.E2E.Tests/EndpointSemanticsTests.cs`) covering `201/200/404/400/204` contracts for generated minimal APIs.

### Changed
- Centralized package metadata and versioning in `Directory.Build.props`.
- Improved generator diagnostics for missing architecture configuration, non-partial entities, and unsupported query filter types.
- Updated generated sources to include the `using` directives they rely on explicitly.
- Expanded the solution to include the CLI and generator test project.
- Updated the CLI support matrix to `net8.0`, `net9.0`, and `net10.0`.
- Refactored repository generation to emit thin per-entity wrappers over a shared generated `CrudRepository<TEntity>` base.
- Refactored CQRS handler persistence logic to delegate shared EF/Core CRUD work into generated generic runtime helpers emitted once per compilation.
- Optimized the shared CRUD runtime to cache per-entity soft-delete traits and clarify specification application for list versus count operations.
- Moved `IUnitOfWork` emission to a one-per-compilation path instead of repeating the source add per entity.
- Centralized QueryFilter generation rules so CQRS list handlers and generated specifications stay aligned.
- Enforced fail-fast behavior when `[assembly: Architecture(...)]` is missing (`RYNOR006` now blocks generation).
- Replaced hard `AppDbContext` convention with configurable `DbContextType` validation for CQRS handlers (`RYNOR008`).
- Added cache invalidation contracts and default distributed-cache invalidator implementations for generated query caches.
- Added `CqrsSaveMode` with per-request transaction save behavior and generated MediatR pipeline support.
- Gated endpoint generation behind explicit experimental opt-in (`EnableExperimentalEndpoints`, `RYNOR012`).
- Added `ArchitectureProfile` quick-start presets to reduce first-run configuration friction.
- Upgraded generated DI extension to auto-register CQRS handlers, validators, and cache query behaviors.
- Added actionable dependency hints (`RYNOR007`) and save-mode wiring warning (`RYNOR013`).
- Expanded CLI with `init` command, profile-based setup prompts, automatic `AssemblyConfig.cs`, and next-step guidance output.
- Added low-touch UnitOfWork wiring via generated `AddRynorArchDependencies<TDbContext>()` overload and generated runtime adapter.
- Made `RynorArch.Sample` runnable end-to-end as a minimal web app using in-memory EF Core.
- Added profile migration recommendation diagnostic (`RYNOR014`) and linked behavior notices to hardening docs.
- Added generated `RynorArchValidationBehavior<,>` and DI wiring so `EnableValidation` now enforces command validators automatically at runtime.
- Added `RYNOR015` warning to flag `EnableValidation` with `GenerateDependencyInjection = false` when validation pipeline wiring must be registered manually.
- Corrected generated endpoint write semantics: `POST` now returns `{ id = ... }`, while `PUT` and `DELETE` now return `404` when target entities are missing.
- Updated generated CQRS write handlers for aggregate roots to raise generated domain events (`Created`, `Updated`, `Deleted`) before persistence.
- Hardened generated EF configuration string detection to support fully-qualified string type names.
- Optimized generated validation behavior to avoid unnecessary allocations when no validators are registered and to lazily allocate validation failure buffers.
- Made generated endpoint and DI outputs deterministic by sorting namespaces/entities, reducing incremental build churn and cache invalidation noise.
- Added `RYNOR016` diagnostic to surface endpoint hardening checklist reminders when endpoint generation is enabled.

### Fixed
- Resolved endpoint semantics mismatch for write operations by returning expected HTTP contracts.
- Reduced incremental generation churn by enforcing stable ordering in generated outputs.

### Security
- No security fixes in this release.

## [1.0.6] - 2026-04-05

### Added
- Initial published package set for `RynorArch.Abstractions`, `RynorArch.Generator`, and `RynorArch.Cli`.

### Changed
- Legacy release from pre-standardized NuGet workflow period.

### Fixed
- Legacy release details not fully retained.

### Security
- No known security fixes documented.

## [1.0.5] - 2026-04-05

### Added
- Legacy patch release (historical details unavailable).

### Changed
- Package set publication alignment during early rollout.

### Fixed
- Legacy patch corrections (details unavailable).

### Security
- No known security fixes documented.

## [1.0.4] - 2026-04-05

### Added
- Legacy patch release (historical details unavailable).

### Changed
- Package set publication alignment during early rollout.

### Fixed
- Legacy patch corrections (details unavailable).

### Security
- No known security fixes documented.

## [1.0.3] - 2026-04-05

### Added
- Legacy patch release (historical details unavailable).

### Changed
- Package set publication alignment during early rollout.

### Fixed
- Legacy patch corrections (details unavailable).

### Security
- No known security fixes documented.

## [1.0.2] - 2026-04-05

### Added
- Legacy patch release introducing multi-package publication for Abstractions and CLI alongside Generator.

### Changed
- Package version track divergence began (Generator already had 1.0.0 and 1.0.1).

### Fixed
- Legacy packaging fixes (details unavailable).

### Security
- No known security fixes documented.

## [1.0.1] - 2026-04-05

### Added
- Legacy patch release for `RynorArch.Generator` (historical details unavailable).

### Changed
- Generator-only package iteration before full package set alignment.

### Fixed
- Legacy generator patch corrections (details unavailable).

### Security
- No known security fixes documented.

## [1.0.0] - 2026-04-05

### Added
- Initial documented release of `RynorArch.Generator` on NuGet.org.

### Changed
- Pre-standardization release from legacy process.

### Fixed
- Legacy release details unavailable.

### Security
- No known security fixes documented.
