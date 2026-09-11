# Architecture

## Philosophy

**Diagnose First. Change Second.**

ZeroTrace separates diagnostics from remediation.

## Layers

- **ZeroTrace.App** — WPF presentation and consent flows.
- **ZeroTrace.Core** — domain types and contracts.
- **ZeroTrace.Diagnostics** — read-only diagnostics and evidence.
- **ZeroTrace.Enterprise** — managed-environment detection.
- **ZeroTrace.Safety** — GREEN / AMBER / RED policy evaluation.
- **ZeroTrace.Remediation** — remediation planning.
- **ZeroTrace.Reporting** — reports and future .zts snapshots.

## Data flow

Discovery → Diagnostics → Evidence → Findings → Safety Classification → Remediation Plan → Explicit Approval → Execution → Verification

## Foundation non-goals

No registry modifications, service disabling, Defender changes, GPO changes, remote administration or cloud upload.
