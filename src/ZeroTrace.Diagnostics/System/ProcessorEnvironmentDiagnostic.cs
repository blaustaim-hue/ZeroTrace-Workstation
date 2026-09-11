using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class ProcessorEnvironmentDiagnostic : IDiagnosticCheck
{
    public string Name => "Processor Environment";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;

        var processorIdentifier = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown";
        var processorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "Unknown";
        var processorLevel = Environment.GetEnvironmentVariable("PROCESSOR_LEVEL") ?? "Unknown";
        var processorRevision = Environment.GetEnvironmentVariable("PROCESSOR_REVISION") ?? "Unknown";

        var evidence = new List<Evidence>
        {
            new("Processor", "LogicalProcessorCount", Environment.ProcessorCount.ToString(), EvidenceState.Info, now),
            new("Processor", "Identifier", processorIdentifier, EvidenceState.Info, now),
            new("Processor", "Architecture", processorArchitecture, EvidenceState.Info, now),
            new("Processor", "Level", processorLevel, EvidenceState.Info, now),
            new("Processor", "Revision", processorRevision, EvidenceState.Info, now)
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
