# ZeroTrace Workstation V1 — Technical Specification

## Product statement

Windows diagnostic and controlled remediation platform for support technicians, infrastructure analysts and advanced users.

## Core doctrine

> DIAGNOSE FIRST. CHANGE SECOND.

No remediation is proposed without evidence. No change is executed without risk classification and user approval.

## Core domain

- Evidence
- Finding
- DiagnosticResult
- RemediationAction
- OptimizationRecommendation
- WorkstationProfile

## Operating modes

### Personal Mode
For unmanaged personal devices.

### Performance Mode
For users who explicitly want more aggressive but reversible performance tuning.

### Enterprise Safe Mode
Activated when managed-environment signals are detected. Policy-sensitive actions are restricted or blocked.

## Performance Center

The Performance Center is advisory-first. It analyzes current state before proposing any change.

### Startup apps
ZeroTrace detects high-impact startup applications and offers a safe launcher to Windows startup settings.

### Game Mode
ZeroTrace reports the current status and offers a direct launcher to Windows Game Mode settings.

### Visual effects
ZeroTrace explains the performance/UX trade-off and treats aggressive visual-effect changes as AMBER.

### Temporary files
ZeroTrace estimates recoverable space, shows a preview, and only removes explicitly approved safe targets.

### Storage optimization
ZeroTrace distinguishes SSD/NVMe from HDD and recommends TRIM/Optimize or defragmentation only when appropriate.

### Power plans
ZeroTrace inspects the active power plan and may offer Ultimate Performance where appropriate.

Ultimate Performance is classified as AMBER because it can:
- increase power consumption;
- increase heat;
- reduce notebook battery life;
- conflict with enterprise policy.

Managed devices default to Enterprise Safe Mode and can block power-policy changes.

## Optimization Advisor

Each recommendation must include:
- reason;
- expected benefit;
- risk level;
- reversibility;
- enterprise compatibility;
- direct action or settings launcher.

Example:
- Startup cleanup — GREEN
- Temporary-file cleanup — GREEN
- Game Mode — GREEN
- Ultimate Performance — AMBER
- Visual-effects adjustment — AMBER

## Safety model

### GREEN
Read-only, low-risk or routine reversible action.

### AMBER
System change requiring explicit approval.

### RED
Blocked by default or requires enterprise review.

## PowerShell boundary

PowerShell is a controlled execution backend, not an unrestricted command shell.

Rules:
- no arbitrary user-provided command execution;
- actions must be predefined and mapped to a known remediation ID;
- preview before execution;
- explicit approval;
- post-action verification;
- rollback where feasible;
- enterprise restrictions enforced before elevation.

## WinUtil integration

ZeroTrace may guide the user toward Chris Titus Tech WinUtil but must not depend on it.

In managed environments, ZeroTrace may explicitly recommend avoiding:
- security-policy changes;
- Defender changes;
- service disabling;
- enterprise-sensitive tweaks.

## ZeroTrace Companion architecture

Post-V1, an Android companion may communicate directly with the Windows application.

Initial transport model:
1. Windows app generates a QR pairing payload.
2. Android app scans the code.
3. A short-lived pairing key is established.
4. Communication occurs on the local network.
5. Transport uses HTTPS and/or secure WebSocket.
6. Companion can receive diagnostics, findings, snapshots and incident passports.
7. Companion can approve only predefined actions already classified by the Safety Engine.
8. Arbitrary remote command execution is prohibited.

The companion remains optional and does not become a cloud dependency.

## Foundation acceptance criteria

1. Solution opens with .NET 10 SDK.
2. Project references are valid.
3. WPF shell starts.
4. Main dashboard displays project status.
5. Architectural boundaries compile.
6. No remediation runs automatically.
7. Security and privacy documentation exists.
8. GitHub Actions Windows build passes.
