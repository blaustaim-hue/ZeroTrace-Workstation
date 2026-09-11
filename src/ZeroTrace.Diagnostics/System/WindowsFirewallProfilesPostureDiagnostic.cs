using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsFirewallProfilesPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Firewall Profiles Posture";

    public async Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = "advfirewall show allprofiles state",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var readable = process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
            var enabledStates = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Count(line => line.Contains("State", StringComparison.OrdinalIgnoreCase) &&
                               line.Contains("ON", StringComparison.OrdinalIgnoreCase));

            evidence.Add(new("FirewallProfilesPosture", "ProfilesReadable",
                readable ? "True" : "False", EvidenceState.Info, now));
            evidence.Add(new("FirewallProfilesPosture", "EnabledProfileStates",
                enabledStates.ToString(), EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("FirewallProfilesPosture", "ProfilesReadable", "False", EvidenceState.Info, now));
        }

        evidence.Add(new("FirewallProfilesPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
