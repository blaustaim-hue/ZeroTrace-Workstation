using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsLsaProtectionPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows LSA Protection Posture";

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
                @"SYSTEM\CurrentControlSet\Control\Lsa");

            var runAsPpl = key?.GetValue("RunAsPPL")?.ToString();
            var runAsPplBoot = key?.GetValue("RunAsPPLBoot")?.ToString();

            evidence.Add(new(
                "LSAProtection",
                "RunAsPPL",
                runAsPpl ?? "NotConfigured",
                EvidenceState.Info,
                now));

            evidence.Add(new(
                "LSAProtection",
                "RunAsPPLBoot",
                runAsPplBoot ?? "NotConfigured",
                EvidenceState.Info,
                now));
        }
        catch
        {
            evidence.Add(new(
                "LSAProtection",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "LSAProtection",
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
