# Security

## Foundation posture

- diagnostics only
- no automatic remediation
- no privilege escalation workflow
- no Defender/firewall/GPO modification
- no certificate deletion
- no telemetry

## Enterprise Safe Mode

Planned indicators include Active Directory domain join, Microsoft Entra join, MDM, Group Policy, enterprise EDR/AV, proxy and VPN indicators.

## Risk classification

| Level | Meaning |
|---|---|
| GREEN | Safe/read-only or routine reversible operation |
| AMBER | System change requiring explicit approval |
| RED | Blocked by default or requires enterprise review |

Diagnostics should run without elevation whenever possible.
