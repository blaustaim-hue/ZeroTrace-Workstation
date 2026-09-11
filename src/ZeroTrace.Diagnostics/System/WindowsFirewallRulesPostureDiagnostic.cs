using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsFirewallRulesPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Firewall Rules Posture";

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
                    Arguments = "advfirewall firewall show rule name=all",
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
            var ruleCount = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Count(line => line.TrimStart().StartsWith("Rule Name:", StringComparison.OrdinalIgnoreCase));

            evidence.Add(new("FirewallRulesPosture", "RulesReadable",
                readable ? "True" : "False", EvidenceState.Info, now));
            evidence.Add(new("FirewallRulesPosture", "ObservedRuleCount",
                ruleCount.ToString(), EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("FirewallRulesPosture", "RulesReadable", "False", EvidenceState.Info, now));
        }

        evidence.Add(new("FirewallRulesPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
