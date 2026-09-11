using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class SecurityPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Security Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        AddRegistryValue(evidence, Registry.LocalMachine,
            @"SOFTWARE\Microsoft\Windows Defender", "DisableAntiSpyware",
            "WindowsDefender.DisableAntiSpyware", now);

        AddRegistryValue(evidence, Registry.LocalMachine,
            @"SYSTEM\CurrentControlSet\Control\SecureBoot\State", "UEFISecureBootEnabled",
            "SecureBoot.Enabled", now);

        AddRegistryValue(evidence, Registry.LocalMachine,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA",
            "UAC.EnableLUA", now);

        evidence.Add(new(
            "Security",
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

    private static void AddRegistryValue(
        ICollection<Evidence> evidence,
        RegistryKey hive,
        string path,
        string valueName,
        string label,
        DateTimeOffset timestamp)
    {
        try
        {
            using var key = hive.OpenSubKey(path);
            var value = key?.GetValue(valueName);
            evidence.Add(new(
                "Security",
                label,
                value?.ToString() ?? "NotAvailable",
                EvidenceState.Info,
                timestamp));
        }
        catch
        {
            evidence.Add(new(
                "Security",
                label,
                "ReadFailed",
                EvidenceState.Info,
                timestamp));
        }
    }
}
