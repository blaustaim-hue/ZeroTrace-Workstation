using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class HardwareIdentityDiagnostic : IDiagnosticCheck
{
    public string Name => "Hardware Identity";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var now = DateTimeOffset.UtcNow;

        string? processorName = null;
        string? biosVendor = null;
        string? biosVersion = null;
        string? baseBoardManufacturer = null;
        string? baseBoardProduct = null;

        try
        {
            using var cpu = Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            processorName = cpu?.GetValue("ProcessorNameString") as string;

            evidence.Add(new(
                "Hardware",
                "ProcessorName",
                string.IsNullOrWhiteSpace(processorName) ? "Unknown" : processorName.Trim(),
                EvidenceState.Info,
                now));
        }
        catch (Exception ex)
        {
            evidence.Add(new(
                "Hardware",
                "ProcessorDiscovery",
                $"Unavailable:{ex.GetType().Name}",
                EvidenceState.Warning,
                now));
        }

        try
        {
            using var bios = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");

            biosVendor = bios?.GetValue("BIOSVendor") as string;
            biosVersion = bios?.GetValue("BIOSVersion") as string;
            baseBoardManufacturer = bios?.GetValue("BaseBoardManufacturer") as string;
            baseBoardProduct = bios?.GetValue("BaseBoardProduct") as string;

            evidence.Add(new("Hardware", "BiosVendor", biosVendor ?? "Unknown", EvidenceState.Info, now));
            evidence.Add(new("Hardware", "BiosVersion", biosVersion ?? "Unknown", EvidenceState.Info, now));
            evidence.Add(new("Hardware", "BaseBoardManufacturer", baseBoardManufacturer ?? "Unknown", EvidenceState.Info, now));
            evidence.Add(new("Hardware", "BaseBoardProduct", baseBoardProduct ?? "Unknown", EvidenceState.Info, now));
        }
        catch (Exception ex)
        {
            evidence.Add(new(
                "Hardware",
                "BiosDiscovery",
                $"Unavailable:{ex.GetType().Name}",
                EvidenceState.Warning,
                now));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            Array.Empty<Finding>()));
    }
}
