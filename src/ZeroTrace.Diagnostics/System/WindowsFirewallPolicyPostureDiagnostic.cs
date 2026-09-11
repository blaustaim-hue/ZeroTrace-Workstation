using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsFirewallPolicyPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Firewall Policy Posture";

    public async Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
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
                    Arguments = "advfirewall show allprofiles",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            evidence.Add(new("FirewallPolicyPosture", "PolicyReadable",
                process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) ? "True" : "False",
                EvidenceState.Info, now));
            evidence.Add(new("FirewallPolicyPosture", "ProfilesObserved",
                new[] { "Domain Profile", "Private Profile", "Public Profile" }
                    .Count(p => output.Contains(p, StringComparison.OrdinalIgnoreCase)).ToString(),
                EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("FirewallPolicyPosture", "PolicyReadable", "False", EvidenceState.Info, now));
        }

        evidence.Add(new("FirewallPolicyPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));
        return new DiagnosticResult(Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
