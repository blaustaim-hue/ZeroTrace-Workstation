using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class UacPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows UAC Posture";

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
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");

            var enableLua = key?.GetValue("EnableLUA")?.ToString();
            var consent = key?.GetValue("ConsentPromptBehaviorAdmin")?.ToString();
            var secureDesktop = key?.GetValue("PromptOnSecureDesktop")?.ToString();

            evidence.Add(new(
                "UAC",
                "EnableLUA",
                enableLua ?? "Unavailable",
                enableLua == "1" ? EvidenceState.Pass : EvidenceState.Info,
                now));

            evidence.Add(new(
                "UAC",
                "ConsentPromptBehaviorAdmin",
                consent ?? "Unavailable",
                EvidenceState.Info,
                now));

            evidence.Add(new(
                "UAC",
                "PromptOnSecureDesktop",
                secureDesktop ?? "Unavailable",
                secureDesktop == "1" ? EvidenceState.Pass : EvidenceState.Info,
                now));
        }
        catch
        {
            evidence.Add(new(
                "UAC",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "UAC",
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
