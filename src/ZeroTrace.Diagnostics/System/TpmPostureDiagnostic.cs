using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class TpmPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows TPM Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var service = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\TPM");

            evidence.Add(new(
                "TPM",
                "DriverService",
                service is null ? "Unavailable" : "Present",
                EvidenceState.Info,
                now));

            var start = service?.GetValue("Start")?.ToString();

            evidence.Add(new(
                "TPM",
                "DriverStartType",
                start ?? "Unavailable",
                EvidenceState.Info,
                now));

            using var wmi = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\Winmgmt");

            evidence.Add(new(
                "TPM",
                "ManagementInfrastructure",
                wmi is null ? "Unavailable" : "Present",
                EvidenceState.Info,
                now));
        }
        catch
        {
            evidence.Add(new(
                "TPM",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "TPM",
            "SensitiveMaterial",
            "KeysAndSecretsNotCollected",
            EvidenceState.Pass,
            now));

        evidence.Add(new(
            "TPM",
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
