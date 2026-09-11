using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class BitLockerPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "BitLocker Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var fve = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Policies\Microsoft\FVE");

            evidence.Add(new(
                "BitLocker",
                "PolicyRegistry",
                fve is null ? "NotConfigured" : "Configured",
                EvidenceState.Info,
                now));

            using var service = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\BDESVC");

            var start = service?.GetValue("Start")?.ToString();

            evidence.Add(new(
                "BitLocker",
                "BDESVC",
                start is null ? "Unavailable" : $"StartType={start}",
                EvidenceState.Info,
                now));
        }
        catch
        {
            evidence.Add(new(
                "BitLocker",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "BitLocker",
            "SensitiveMaterial",
            "RecoveryKeysNotCollected",
            EvidenceState.Pass,
            now));

        evidence.Add(new(
            "BitLocker",
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
