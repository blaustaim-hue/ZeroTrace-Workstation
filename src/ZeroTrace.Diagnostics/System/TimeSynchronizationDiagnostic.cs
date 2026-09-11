using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class TimeSynchronizationDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Time Synchronization";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        ReadValue(evidence,
            @"SYSTEM\CurrentControlSet\Services\W32Time\Parameters",
            "Type", "SynchronizationType", now);

        ReadValue(evidence,
            @"SYSTEM\CurrentControlSet\Services\W32Time\Parameters",
            "NtpServer", "NtpServer", now);

        ReadValue(evidence,
            @"SYSTEM\CurrentControlSet\Services\W32Time",
            "Start", "ServiceStartMode", now);

        evidence.Add(new(
            "Time",
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

    private static void ReadValue(
        ICollection<Evidence> evidence,
        string path,
        string valueName,
        string label,
        DateTimeOffset timestamp)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(path);
            var value = key?.GetValue(valueName);

            evidence.Add(new(
                "Time",
                label,
                value?.ToString() ?? "NotAvailable",
                EvidenceState.Info,
                timestamp));
        }
        catch
        {
            evidence.Add(new(
                "Time",
                label,
                "ReadFailed",
                EvidenceState.Info,
                timestamp));
        }
    }
}
