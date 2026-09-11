using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class StartupEnvironmentDiagnostic : IDiagnosticCheck
{
    public string Name => "Startup Environment";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;
        var entries = new List<string>();

        ReadRunKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKCU", entries);
        ReadRunKey(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM", entries);

        evidence.Add(new(
            "Startup",
            "RegistryRunEntryCount",
            entries.Count.ToString(),
            EvidenceState.Info,
            now));

        evidence.Add(new(
            "Startup",
            "RegistryRunEntries",
            entries.Count == 0 ? "NoneDetected" : string.Join(" | ", entries),
            EvidenceState.Info,
            now));

        evidence.Add(new(
            "Startup",
            "MutationPolicy",
            "ReadOnlyDiagnostic",
            EvidenceState.Pass,
            now));

        if (entries.Count >= 10)
        {
            findings.Add(new Finding(
                "STARTUP-001",
                "Many startup applications detected",
                $"At least {entries.Count} registry startup entries were detected.",
                "Info",
                80,
                evidence.Where(e => e.Source == "Startup").ToArray(),
                "Review startup applications and disable only items that are unnecessary. Do not disable security, management, backup, accessibility, or business-critical software without validation."));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings));
    }

    private static void ReadRunKey(RegistryKey hive, string path, string hiveName, List<string> entries)
    {
        try
        {
            using var key = hive.OpenSubKey(path);
            if (key is null) return;

            foreach (var valueName in key.GetValueNames())
            {
                entries.Add($"{hiveName}:{valueName}");
            }
        }
        catch
        {
            // Discovery is best-effort and remains non-mutating.
        }
    }
}
