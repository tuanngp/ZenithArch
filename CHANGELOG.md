# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Automated generator regression tests for diagnostics and generated outputs.
- GitHub Actions CI and tag-based release workflows.
- Troubleshooting, compatibility, upgrade, contribution, and security documentation.
- New enterprise operation docs: endpoint hardening, caching operations, and profile migration guidance.
- AI-agent playbook documenting deterministic task contracts and verification rules.
- New CLI `doctor` command for readiness checks with actionable fix steps.
- E2E smoke test project for CLI workflow validation and CI/release quality gates.

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
- Corrected generated endpoint write semantics: `POST` now returns `{ id = ... }`, while `PUT`/`DELETE` now return `404` when target entities are missing.
- Updated generated CQRS write handlers for aggregate roots to raise generated domain events (`Created`, `Updated`, `Deleted`) before persistence.
- Hardened generated EF configuration string detection to support fully-qualified string type names.

## [1.0.6]

### Added
- Initial published package set for `RynorArch.Abstractions`, `RynorArch.Generator`, and `RynorArch.Cli`.
