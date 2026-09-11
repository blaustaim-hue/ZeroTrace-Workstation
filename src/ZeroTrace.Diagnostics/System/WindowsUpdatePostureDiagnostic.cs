using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsUpdatePostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Update Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            const string auPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update";
            using var au = Registry.LocalMachine.OpenSubKey(auPath);

            var rebootRequired = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired") is not null;

            evidence.Add(new(
                "WindowsUpdate",
                "RegistryAccess",
                au is null ? "Unavailable" : "Available",
                EvidenceState.Info,
                now));

            evidence.Add(new(
                "WindowsUpdate",
                "RebootRequired",
                rebootRequired ? "Yes" : "No",
                rebootRequired ? EvidenceState.Info : EvidenceState.Pass,
                now));

            using var policy = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU");

            var noAutoUpdate = policy?.GetValue("NoAutoUpdate")?.ToString();

            evidence.Add(new(
                "WindowsUpdate",
                "NoAutoUpdatePolicy",
                noAutoUpdate ?? "NotConfigured",
                noAutoUpdate == "1" ? EvidenceState.Fail : EvidenceState.Pass,
                now));
        }
        catch
        {
            evidence.Add(new(
                "WindowsUpdate",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "WindowsUpdate",
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
