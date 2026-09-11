using System.Runtime.InteropServices;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

public sealed class SystemProfileDiagnostic : IDiagnosticCheck
{
    public string Name => "System Profile";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;

        var profile = new SystemProfile(
            Environment.MachineName,
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture.ToString(),
            RuntimeInformation.ProcessArchitecture.ToString(),
            RuntimeInformation.FrameworkDescription,
            Environment.ProcessorCount,
            Environment.WorkingSet,
            TimeSpan.FromMilliseconds(Environment.TickCount64));

        var evidence = new List<Evidence>
        {
            new("System","MachineName",profile.MachineName,EvidenceState.Info,DateTimeOffset.UtcNow),
            new("System","OS",profile.OsDescription,EvidenceState.Info,DateTimeOffset.UtcNow),
            new("System","OSArchitecture",profile.OsArchitecture,EvidenceState.Info,DateTimeOffset.UtcNow),
            new("System","ProcessArchitecture",profile.ProcessArchitecture,EvidenceState.Info,DateTimeOffset.UtcNow),
            new("System","Framework",profile.FrameworkDescription,EvidenceState.Info,DateTimeOffset.UtcNow),
            new("System","ProcessorCount",profile.ProcessorCount.ToString(),EvidenceState.Info,DateTimeOffset.UtcNow),
            new("System","WorkingSetBytes",profile.WorkingSetBytes.ToString(),EvidenceState.Info,DateTimeOffset.UtcNow),
            new("System","Uptime",profile.Uptime.ToString(),EvidenceState.Info,DateTimeOffset.UtcNow)
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
