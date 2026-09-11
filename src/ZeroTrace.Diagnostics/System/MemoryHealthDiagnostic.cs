using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class MemoryHealthDiagnostic : IDiagnosticCheck
{
    public string Name => "Memory Health";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var status = new MemoryStatusEx();
        if (!GlobalMemoryStatusEx(status))
        {
            var errorEvidence = new List<Evidence>
            {
                new("Memory", "GlobalMemoryStatusEx", "Failed", EvidenceState.Fail, DateTimeOffset.UtcNow)
            };

            return Task.FromResult(new DiagnosticResult(
                Name,
                started,
                DateTimeOffset.UtcNow,
                false,
                errorEvidence,
                Array.Empty<Finding>()));
        }

        var total = status.TotalPhysical;
        var available = status.AvailablePhysical;
        var used = total >= available ? total - available : 0;
        var availablePercent = total > 0
            ? Math.Round((double)available / total * 100, 2)
            : 0;

        var state = availablePercent < 10
            ? EvidenceState.Fail
            : availablePercent < 20
                ? EvidenceState.Warning
                : EvidenceState.Pass;

        var now = DateTimeOffset.UtcNow;
        var evidence = new List<Evidence>
        {
            new("Memory", "TotalPhysicalBytes", total.ToString(), EvidenceState.Info, now),
            new("Memory", "AvailablePhysicalBytes", available.ToString(), state, now),
            new("Memory", "UsedPhysicalBytes", used.ToString(), EvidenceState.Info, now),
            new("Memory", "AvailablePercent", availablePercent.ToString("F2"), state, now),
            new("Memory", "SystemMemoryLoadPercent", status.MemoryLoadPercent.ToString(), state, now)
        };

        var findings = new List<Finding>();

        if (availablePercent < 10)
        {
            findings.Add(new Finding(
                "MEM-001",
                "Critically low available memory",
                $"Only {availablePercent:F2}% of physical memory is currently available.",
                "Critical",
                95,
                evidence,
                "Close unnecessary high-memory applications and investigate sustained memory pressure."));
        }
        else if (availablePercent < 20)
        {
            findings.Add(new Finding(
                "MEM-002",
                "Low available memory",
                $"Only {availablePercent:F2}% of physical memory is currently available.",
                "Warning",
                90,
                evidence,
                "Review high-memory processes and startup applications before applying any optimization."));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings));
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx status);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        public uint Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
        public uint MemoryLoadPercent;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }
}
