# ZeroTrace Workstation

**Local-first Windows diagnostic and remediation platform for Support, Infrastructure and N3 troubleshooting.**

> **DIAGNOSE FIRST. CHANGE SECOND.**

ZeroTrace Workstation is designed to diagnose first, collect evidence, classify risk, and only then propose controlled remediation actions.

## V1 principles

- Local-first
- Privacy by design
- No telemetry by default
- No cloud required
- No destructive automatic actions
- Enterprise-aware
- Evidence-driven diagnostics
- Explicit risk classification: GREEN / AMBER / RED

## Foundation status

**v0.1.0 — Foundation**

This milestone establishes:
- .NET 10 / WPF project structure
- Core domain models
- Diagnostic interfaces
- Enterprise detection boundary
- Safety engine boundary
- Remediation plan boundary
- Reporting boundary
- Initial MVVM desktop shell
- Security and privacy documentation

## Security model

The V1 foundation does **not** execute destructive remediation automatically. Actions that may change the system must pass through the Safety Engine and explicit user approval.

## License

MIT
