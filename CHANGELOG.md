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

## [1.0.6]

### Added
- Initial published package set for `RynorArch.Abstractions`, `RynorArch.Generator`, and `RynorArch.Cli`.
