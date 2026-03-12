<!--
Sync Impact Report
Version change: template -> 1.0.0
Modified principles:
- Template Principle 1 -> I. User Value First
- Template Principle 2 -> II. Clear Web/API Boundaries
- Template Principle 3 -> III. Graceful Job Data Handling
- Template Principle 4 -> IV. Verification Before Merge
- Template Principle 5 -> V. Simplicity and Privacy
Added sections:
- Product Constraints
- Development Workflow
Removed sections:
- None
Templates requiring updates:
- ✅ .specify/templates/plan-template.md
- ✅ .specify/templates/spec-template.md
- ✅ .specify/templates/tasks-template.md
Follow-up TODOs:
- None
-->

# JobSeeker Constitution

## Core Principles

### I. User Value First
Every planned feature MUST directly support at least one core user goal: discovering
job listings, reviewing listing details, or tracking a next action. Work that does
not produce clear user value or required platform support MUST NOT be prioritized.
Rationale: this project is an MVP job-search application and must stay focused on
useful job discovery over speculative features.

### II. Clear Web/API Boundaries
The Angular frontend MUST access job data through explicit frontend services and the
ASP.NET Core API; it MUST NOT call external feeds directly. Business rules, feed
parsing, and integration logic MUST remain in backend application or infrastructure
layers, and API request/response shapes MUST be defined before implementation.
Rationale: stable boundaries keep the UI replaceable and reduce coupling to external
sources.

### III. Graceful Job Data Handling
Any job listing experience MUST expose the minimum usable fields available from the
source: title, source, destination link, and publication or update date when
provided. Missing, empty, or failed upstream data MUST result in a safe loading,
empty, or error state rather than an unhandled exception or broken page. Rationale:
external job feeds are inherently unreliable and the product must degrade safely.

### IV. Verification Before Merge
Every change to job retrieval, filtering, presentation, or API contracts MUST include
verification proportional to risk: automated tests when practical in the touched
layer, otherwise documented manual verification steps covering success, empty, and
failure states. Bugs SHOULD be reproduced before fixing when feasible. Rationale:
even a small app needs repeatable confidence on its core user journeys.

### V. Simplicity and Privacy
The system MUST collect, store, and expose only data required for the current job
search scope. Secrets MUST NOT be committed to source control, speculative
architecture MUST be avoided, and each new dependency MUST have a clear need.
Rationale: a lean architecture and minimal data handling reduce delivery and privacy
risk.

## Product Constraints

- The MVP MUST support retrieving job listings from at least one configured source
	and displaying them in the web application.
- Each displayed listing MUST provide a way to open the original job posting.
- User-facing screens MUST define loading, empty, and error states.
- Any persistence introduced beyond application configuration MUST document why it is
	needed and what data is retained.
- New interactive UI elements MUST be keyboard reachable and use human-readable
	labels.

## Development Workflow

- Specifications MUST identify the primary user story, affected API and UI surfaces,
	and verification steps before implementation begins.
- Plans MUST pass a Constitution Check for user value, layer ownership, upstream
	failure handling, and verification coverage.
- Tasks MUST be grouped by user story and include required contract, UI state, and
	validation work.
- Reviews MUST confirm that changed behavior is demonstrable in the web app or API
	and that configuration changes are documented.

## Governance

- This constitution overrides conflicting informal practices for this repository.
- An amendment MUST update this file and any impacted templates or guidance docs in
	the same change.
- Versioning follows semantic versioning for governance documents: MAJOR for
	incompatible principle or governance changes, MINOR for new principles or materially
	expanded guidance, and PATCH for clarifications that do not change intent.
- Compliance MUST be checked during planning and review. A feature that does not pass
	the Constitution Check MUST either be revised or explicitly justified in the plan.

**Version**: 1.0.0 | **Ratified**: 2026-03-11 | **Last Amended**: 2026-03-11
