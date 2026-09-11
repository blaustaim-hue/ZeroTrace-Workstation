using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsDefenderPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Defender Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var defender = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows Defender");

            evidence.Add(new(
                "WindowsDefender",
                "RegistryPresence",
                defender is null ? "Unavailable" : "Available",
                EvidenceState.Info,
                now));

            using var policy = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Policies\Microsoft\Windows Defender");

            var disableAntiSpyware = policy?.GetValue("DisableAntiSpyware")?.ToString();

            evidence.Add(new(
                "WindowsDefender",
                "DisableAntiSpywarePolicy",
                disableAntiSpyware ?? "NotConfigured",
                disableAntiSpyware == "1" ? EvidenceState.Fail : EvidenceState.Pass,
                now));

            using var service = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\WinDefend");

            var start = service?.GetValue("Start")?.ToString();

            evidence.Add(new(
                "WindowsDefender",
                "WinDefendService",
                start is null ? "Unavailable" : $"StartType={start}",
                EvidenceState.Info,
                now));
        }
        catch
        {
            evidence.Add(new(
                "WindowsDefender",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "WindowsDefender",
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
