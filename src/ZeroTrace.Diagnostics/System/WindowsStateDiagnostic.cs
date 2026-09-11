using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsStateDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows State";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        string? productName = null;
        string? displayVersion = null;
        string? currentBuild = null;
        string? ubr = null;

        try
        {
            using var currentVersion = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            productName = currentVersion?.GetValue("ProductName") as string;
            displayVersion = currentVersion?.GetValue("DisplayVersion") as string;
            currentBuild = currentVersion?.GetValue("CurrentBuildNumber") as string;
            ubr = currentVersion?.GetValue("UBR")?.ToString();

            evidence.Add(new("Windows", "ProductName", productName ?? "Unknown", EvidenceState.Info, now));
            evidence.Add(new("Windows", "DisplayVersion", displayVersion ?? "Unknown", EvidenceState.Info, now));
            evidence.Add(new("Windows", "CurrentBuild", currentBuild ?? "Unknown", EvidenceState.Info, now));
            evidence.Add(new("Windows", "UBR", ubr ?? "Unknown", EvidenceState.Info, now));
        }
        catch (Exception ex)
        {
            evidence.Add(new(
                "Windows",
                "VersionDiscovery",
                $"Unavailable:{ex.GetType().Name}",
                EvidenceState.Warning,
                now));
        }

        var pendingRebootReasons = new List<string>();

        if (RegistryKeyExists(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending"))
        {
            pendingRebootReasons.Add("ComponentBasedServicing");
        }

        if (RegistryKeyExists(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"))
        {
            pendingRebootReasons.Add("WindowsUpdate");
        }

        if (HasPendingFileRenameOperations())
        {
            pendingRebootReasons.Add("PendingFileRenameOperations");
        }

        var rebootPending = pendingRebootReasons.Count > 0;

        var rebootEvidence = new Evidence(
            "Windows",
            "PendingReboot",
            rebootPending ? string.Join(",", pendingRebootReasons) : "False",
            rebootPending ? EvidenceState.Warning : EvidenceState.Pass,
            DateTimeOffset.UtcNow);

        evidence.Add(rebootEvidence);

        if (rebootPending)
        {
            findings.Add(new Finding(
                "WIN-001",
                "Windows restart pending",
                "Windows reports one or more indicators that a restart may be required.",
                "Warning",
                95,
                new[] { rebootEvidence },
                "Schedule a restart when operationally safe. On managed devices, follow the organization's maintenance and change-control policy."));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings));
    }

    private static bool RegistryKeyExists(string path)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(path);
            return key is not null;
        }
        catch
        {
            return false;
        }
    }

    private static bool HasPendingFileRenameOperations()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\Session Manager");

            var value = key?.GetValue("PendingFileRenameOperations") as string[];
            return value is { Length: > 0 };
        }
        catch
        {
            return false;
        }
    }
}
