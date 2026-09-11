using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class SecureBootPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Secure Boot Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\SecureBoot\State");

            var value = key?.GetValue("UEFISecureBootEnabled")?.ToString();

            evidence.Add(new(
                "SecureBoot",
                "UEFISecureBootEnabled",
                value ?? "Unavailable",
                value == "1" ? EvidenceState.Pass : EvidenceState.Info,
                now));
        }
        catch
        {
            evidence.Add(new(
                "SecureBoot",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "SecureBoot",
            "SensitiveMaterial",
            "FirmwareKeysNotCollected",
            EvidenceState.Pass,
            now));

        evidence.Add(new(
            "SecureBoot",
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
