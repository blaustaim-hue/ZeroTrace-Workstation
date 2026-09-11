using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsSessionDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Session";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;

        var userDomain = Environment.UserDomainName;
        var userName = Environment.UserName;
        var sessionName = Environment.GetEnvironmentVariable("SESSIONNAME") ?? "Unknown";

        var evidence = new List<Evidence>
        {
            new("Session", "UserDomain", userDomain, EvidenceState.Info, now),
            new("Session", "UserName", userName, EvidenceState.Info, now),
            new("Session", "SessionName", sessionName, EvidenceState.Info, now),
            new("Session", "UserInteractive", Environment.UserInteractive.ToString(), EvidenceState.Info, now)
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
