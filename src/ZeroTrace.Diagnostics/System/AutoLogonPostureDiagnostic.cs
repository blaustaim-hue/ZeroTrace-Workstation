using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class AutoLogonPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Auto-Logon Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        const string path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(path);
            var autoAdminLogon = key?.GetValue("AutoAdminLogon")?.ToString();
            var defaultPasswordPresent = key?.GetValue("DefaultPassword") is not null;

            evidence.Add(new(
                "Authentication",
                "AutoAdminLogon",
                autoAdminLogon ?? "NotConfigured",
                EvidenceState.Info,
                now));

            evidence.Add(new(
                "Authentication",
                "DefaultPasswordPresent",
                defaultPasswordPresent ? "Yes" : "No",
                defaultPasswordPresent ? EvidenceState.Fail : EvidenceState.Pass,
                now));
        }
        catch
        {
            evidence.Add(new(
                "Authentication",
                "AutoLogonRegistry",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "Authentication",
            "SensitiveDataPolicy",
            "PasswordValueNeverCollected",
            EvidenceState.Pass,
            now));

        evidence.Add(new(
            "Authentication",
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
