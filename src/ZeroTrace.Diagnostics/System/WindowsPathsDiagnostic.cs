using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsPathsDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Paths";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;

        var evidence = new List<Evidence>
        {
            new("Paths", "SystemDirectory", Environment.SystemDirectory, EvidenceState.Info, now),
            new("Paths", "WindowsDirectory", Environment.GetFolderPath(Environment.SpecialFolder.Windows), EvidenceState.Info, now),
            new("Paths", "ProgramFiles", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), EvidenceState.Info, now),
            new("Paths", "CommonApplicationData", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), EvidenceState.Info, now),
            new("Paths", "LocalApplicationData", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), EvidenceState.Info, now)
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
