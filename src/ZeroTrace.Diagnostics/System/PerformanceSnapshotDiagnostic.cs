using System.Runtime.InteropServices;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

public sealed class PerformanceSnapshotDiagnostic : IDiagnosticCheck
{
    public string Name => "Performance Snapshot";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        var evidence = new List<Evidence>
        {
            new("Performance","LogicalProcessorCount",Environment.ProcessorCount.ToString(),EvidenceState.Info,now),
            new("Performance","ProcessWorkingSetBytes",Environment.WorkingSet.ToString(),EvidenceState.Info,now),
            new("Performance","SystemUptime",TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(),EvidenceState.Info,now),
            new("Performance","OSArchitecture",RuntimeInformation.OSArchitecture.ToString(),EvidenceState.Info,now),
            new("Performance","ProcessArchitecture",RuntimeInformation.ProcessArchitecture.ToString(),EvidenceState.Info,now),
            new("Performance","Is64BitOperatingSystem",Environment.Is64BitOperatingSystem.ToString(),EvidenceState.Info,now),
            new("Performance","Is64BitProcess",Environment.Is64BitProcess.ToString(),EvidenceState.Info,now)
        };

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            Array.Empty<Finding>()));
    }
}
