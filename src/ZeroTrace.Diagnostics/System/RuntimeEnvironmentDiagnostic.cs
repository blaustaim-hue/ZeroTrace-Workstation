using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class RuntimeEnvironmentDiagnostic : IDiagnosticCheck
{
    public string Name => "Runtime Environment";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;

        var evidence = new List<Evidence>
        {
            new("Runtime", "FrameworkDescription", RuntimeInformation.FrameworkDescription, EvidenceState.Info, now),
            new("Runtime", "ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString(), EvidenceState.Info, now),
            new("Runtime", "OSArchitecture", RuntimeInformation.OSArchitecture.ToString(), EvidenceState.Info, now),
            new("Runtime", "Is64BitProcess", Environment.Is64BitProcess.ToString(), EvidenceState.Info, now),
            new("Runtime", "UserInteractive", Environment.UserInteractive.ToString(), EvidenceState.Info, now),
            new("Runtime", "SystemDirectory", Environment.SystemDirectory, EvidenceState.Info, now)
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
