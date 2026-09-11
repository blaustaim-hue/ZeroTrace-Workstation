using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class FirewallProfileDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Firewall Profiles";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        ReadProfile(evidence, "DomainProfile", now);
        ReadProfile(evidence, "PrivateProfile", now);
        ReadProfile(evidence, "PublicProfile", now);

        evidence.Add(new(
            "Firewall",
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

    private static void ReadProfile(
        ICollection<Evidence> evidence,
        string profile,
        DateTimeOffset timestamp)
    {
        const string basePath =
            @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\";

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(basePath + profile);
            var enabled = key?.GetValue("EnableFirewall");

            evidence.Add(new(
                "Firewall",
                profile + ".Enabled",
                enabled?.ToString() ?? "NotAvailable",
                EvidenceState.Info,
                timestamp));
        }
        catch
        {
            evidence.Add(new(
                "Firewall",
                profile + ".Enabled",
                "ReadFailed",
                EvidenceState.Info,
                timestamp));
        }
    }
}
