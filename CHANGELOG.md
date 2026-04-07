# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Automated generator regression tests for diagnostics and generated outputs.
- GitHub Actions CI and tag-based release workflows.
- Troubleshooting, compatibility, upgrade, contribution, and security documentation.

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

## [1.0.6]

### Added
- Initial published package set for `RynorArch.Abstractions`, `RynorArch.Generator`, and `RynorArch.Cli`.
