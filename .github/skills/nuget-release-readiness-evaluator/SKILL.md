---
name: nuget-release-readiness-evaluator
description: 'Evaluate NuGet package release readiness for public and enterprise adoption. Use when scoring release quality, checking hard blockers, producing go/no-go verdicts, and creating prioritized remediation plans from repo evidence, CI logs, and test results.'
argument-hint: 'Provide package name/version, repository URL, target frameworks, known limitations, and any test/benchmark/CI evidence.'
user-invocable: true
---

# NuGet Release Readiness Evaluator

## Purpose
Evaluate a NuGet package's readiness for wide public release and enterprise integration.
Produce an evidence-based report with:
- Hard blocker status
- Per-domain score breakdown
- Total score out of 100
- Verdict tier
- Prioritized top gaps and fixes

## When To Use
Use this skill when the user asks to:
- Assess whether a package is ready to publish broadly
- Validate enterprise adoption readiness
- Run a structured go/no-go release review
- Re-score a package after fixes are applied

## Inputs To Request First
If any required input is missing, request it before evaluation:
- Package name and current version on NuGet.org
- Repository URL (GitHub, Azure DevOps, etc.)
- Declared target frameworks in the .csproj
- Known open issues or limitations
- Existing test results, benchmark reports, or CI logs

## Evaluation Workflow
1. Gather evidence
- Use repository files, issue tracker data, CI logs, and test artifacts.
- If evidence is missing, state uncertainty and ask for proof instead of assuming full credit.

2. Check hard blockers first
- Evaluate all hard blockers B1-B7 before any scoring.
- If any blocker is open, output BLOCKED and stop before scoring.

3. If not blocked, score each domain
- Score every sub-criterion with explicit points.
- Sum sub-criteria to domain totals, then compute total out of 100.

4. Derive verdict
- Map total score to the verdict table exactly.

5. Build remediation plan
- List top 3 gaps by combined consumer risk and score impact.
- Provide concrete fixes and effort estimates.

6. Re-evaluate after fixes
- When user reports a fix, recompute impacted criteria and update the full score and verdict.

## Hard Blockers (Must Pass Before Scoring)
If any blocker is true, output BLOCKED and do not assign a score.

### B1. High-severity vulnerable dependency
- Condition: Known CVE with CVSS >= 7.0 in direct or transitive dependencies
- Verification: dotnet list package --vulnerable --include-transitive

### B2. Breaking change without MAJOR bump
- Condition: Breaking API change released as MINOR/PATCH
- Verification: dotnet-apicompat against previous published version

### B3. Tests fail on any declared TFM
- Condition: Any test failure on any TFM in TargetFrameworks
- Verification: run tests across all declared TFMs

### B4. Missing SPDX license expression
- Condition: No PackageLicenseExpression in project metadata
- Verification: inspect .csproj package properties

### B5. Open P0 data-loss/data-corruption bug
- Condition: Any confirmed open P0 issue related to loss/corruption
- Verification: issue tracker review with severity labels

### B6. PackageId ownership/reservation not provable
- Condition: Package ID ownership/control cannot be confirmed, or expected prefix reservation is missing for new package families
- Verification:
  - Existing package IDs: confirm package has expected owners on NuGet.org (owner control proof)
  - New package IDs/prefixes: confirm prefix reservation in NuGet account settings or equivalent admin evidence
  - Important: do not treat the NuGet search field `verified` as prefix-reservation proof; it reflects verified profile state, not PackageId prefix reservation

### B7. Zero XML docs on public API
- Condition: Public API lacks XML documentation entirely
- Verification: inspect source and generated API docs/IntelliSense readiness

## Scoring Model (100 Points)
Only run this section when all hard blockers are clear.

### Domain 1: API Stability (20)
- [5] SemVer strictly applied across published versions
- [5] No unintended breaking changes (ApiCompat evidence)
- [5] Deprecation path exists for removed/changed API
- [5] API surface is minimal, no leaked internal implementation types

### Domain 2: Reliability and Correctness (20)
- [8] Test coverage >= 80% on public API surface
- [6] Edge cases handled (null, empty, boundaries, concurrency)
- [6] No open P0/P1 bugs

### Domain 3: Documentation Quality (15)
- [5] Complete XML docs for public types and members
- [5] README includes install, quickstart, API reference, config options
- [3] CHANGELOG current and Keep a Changelog aligned
- [2] Migration guides for each MAJOR bump

### Domain 4: Performance and Efficiency (15)
- [5] BenchmarkDotNet baselines committed
- [5] Hot paths allocation-reviewed (MemoryDiagnoser/profiler)
- [5] Minimal startup/build/publish overhead

### Domain 5: Security and Compliance (15)
- [6] No known vulnerable dependencies
- [5] Package signed and identity control proven (owner control for existing IDs or prefix reservation for new IDs)
- [4] Valid SPDX expression and LICENSE included in package

### Domain 6: Ecosystem Compatibility (10)
- [4] All declared TFMs tested and passing in CI
- [3] AOT/trimming safe if declared as supported
- [3] DI-friendly design (no global static state, no ambient context)

### Domain 7: Operational Readiness (5)
- [3] Automated CI/CD for test + pack + publish on version tag
- [2] Support channel clearly defined

## Verdict Mapping
- 0-49: NOT READY
- 50-69: LIMITED BETA
- 70-84: PRODUCTION READY
- 85-100: ENTERPRISE GRADE

Constraint: Never output PRODUCTION READY or ENTERPRISE GRADE if any hard blocker is open.

## Required Output Format
Use this exact structure:

--- RELEASE READINESS REPORT ---
Package: <name> v<version>
Evaluated: <date>

HARD BLOCKERS: <NONE | list each blocker with description>

DOMAIN SCORES:
  API Stability          X / 20   - <one-sentence reason>
  Reliability            X / 20   - <one-sentence reason>
  Documentation          X / 15   - <one-sentence reason>
  Performance            X / 15   - <one-sentence reason>
  Security               X / 15   - <one-sentence reason>
  Ecosystem Compat.      X / 10   - <one-sentence reason>
  Operational Readiness  X /  5   - <one-sentence reason>
  -----------------------------
  TOTAL                  X / 100

VERDICT: <NOT READY | LIMITED BETA | PRODUCTION READY | ENTERPRISE GRADE>

TOP 3 GAPS TO FIX (ordered by impact on score and consumer risk):
1. <Gap> - Fix: <exact action> - Effort: <Quick <1h | Medium 1-4h | Significant >4h>
2. <Gap> - Fix: <exact action> - Effort: <Quick | Medium | Significant>
3. <Gap> - Fix: <exact action> - Effort: <Quick | Medium | Significant>

WHAT IS ALREADY STRONG:
- <positive finding 1>
- <positive finding 2>

NEXT MILESTONE: "After fixing the top 3 gaps, estimated score is X/100 (<verdict>)."
--- END REPORT ---

## Scoring Discipline Rules
Always:
- Check all hard blockers before scoring.
- Provide numeric scores per sub-criterion, then domain totals.
- Explain every deduction with concrete evidence or explicit missing evidence.
- Give actionable fixes for each major gap.
- Estimate effort as Quick, Medium, or Significant.
- Re-score when new evidence or fixes are provided.

Never:
- Assign a release-ready verdict with any open hard blocker.
- Award full points without evidence.
- Use vague gap descriptions without exact remediation steps.
- Inflate scores to be encouraging.

## Decision Defaults For Missing Evidence
- If evidence is unavailable, assign conservative partial credit or zero where appropriate.
- Mark each uncertain criterion as "evidence missing" and request the exact artifact needed.
- Prioritize consumer safety over optimistic assumptions.
- For hard blockers, only mark BLOCKED when the blocker condition is explicitly true from evidence. If evidence is missing, mark as "evidence missing" and request proof instead of auto-blocking.
