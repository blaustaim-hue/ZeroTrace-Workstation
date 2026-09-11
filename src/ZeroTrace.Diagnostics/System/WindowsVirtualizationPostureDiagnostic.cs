using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsVirtualizationPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Virtualization Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var dg = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard");

            var vbs = dg?.GetValue("EnableVirtualizationBasedSecurity")?.ToString();

            evidence.Add(new(
                "Virtualization",
                "VirtualizationBasedSecurity",
                vbs ?? "Unavailable",
                EvidenceState.Info,
                now));

            using var hvci = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity");

            var enabled = hvci?.GetValue("Enabled")?.ToString();

            evidence.Add(new(
                "Virtualization",
                "HypervisorEnforcedCodeIntegrity",
                enabled ?? "Unavailable",
                EvidenceState.Info,
                now));
        }
        catch
        {
            evidence.Add(new(
                "Virtualization",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "Virtualization",
            "MutationPolicy",
            "ReadOnlyDiagnostic",
            EvidenceState.Pass,
            now));

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings));
    }
}
