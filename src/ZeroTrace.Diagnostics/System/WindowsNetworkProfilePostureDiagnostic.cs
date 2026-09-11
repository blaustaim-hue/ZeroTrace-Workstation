using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsNetworkProfilePostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Network Profile Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var profiles = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles");

            var names = profiles?.GetSubKeyNames() ?? Array.Empty<string>();

            evidence.Add(new(
                "NetworkProfile",
                "ProfileCount",
                names.Length.ToString(),
                EvidenceState.Info,
                now));

            foreach (var name in names.Take(25))
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var profile = profiles?.OpenSubKey(name);
                var profileName = profile?.GetValue("ProfileName")?.ToString() ?? "Unnamed";
                var category = profile?.GetValue("Category")?.ToString() ?? "Unavailable";

                evidence.Add(new(
                    "NetworkProfile",
                    profileName,
                    $"Category={category}",
                    EvidenceState.Info,
                    now));
            }
        }
        catch
        {
            evidence.Add(new(
                "NetworkProfile",
                "PostureRead",
                "ReadFailed",
                EvidenceState.Info,
                now));
        }

        evidence.Add(new(
            "NetworkProfile",
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
}
