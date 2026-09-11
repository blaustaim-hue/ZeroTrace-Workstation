using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class InstalledSoftwareDiagnostic : IDiagnosticCheck
{
    public string Name => "Installed Software";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;
        var software = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        ReadUninstallKey(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", software);
        ReadUninstallKey(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", software);
        ReadUninstallKey(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", software);

        evidence.Add(new(
            "Software",
            "InstalledApplicationCount",
            software.Count.ToString(),
            EvidenceState.Info,
            now));

        evidence.Add(new(
            "Software",
            "InstalledApplications",
            software.Count == 0 ? "NoneDetected" : string.Join(" | ", software),
            EvidenceState.Info,
            now));

        evidence.Add(new(
            "Software",
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

    private static void ReadUninstallKey(RegistryKey hive, string path, ISet<string> software)
    {
        try
        {
            using var root = hive.OpenSubKey(path);
            if (root is null) return;

            foreach (var subKeyName in root.GetSubKeyNames())
            {
                using var app = root.OpenSubKey(subKeyName);
                var displayName = app?.GetValue("DisplayName") as string;
                if (!string.IsNullOrWhiteSpace(displayName))
                    software.Add(displayName.Trim());
            }
        }
        catch
        {
            // Inventory is best-effort and strictly read-only.
        }
    }
}
