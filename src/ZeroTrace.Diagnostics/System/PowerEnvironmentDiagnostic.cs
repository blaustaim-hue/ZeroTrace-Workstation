using System.Diagnostics;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

public sealed class PowerEnvironmentDiagnostic : IDiagnosticCheck
{
    public string Name => "Power Environment";

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
                    FileName = "powercfg.exe",
                    Arguments = "/getactivescheme",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var output = (await outputTask).Trim();
            var error = (await errorTask).Trim();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                evidence.Add(new("Power", "ActiveScheme", output, EvidenceState.Info, now));
            }
            else
            {
                evidence.Add(new(
                    "Power",
                    "ActiveScheme",
                    string.IsNullOrWhiteSpace(error) ? $"Unavailable:ExitCode{process.ExitCode}" : $"Unavailable:{error}",
                    EvidenceState.Warning,
                    now));
            }
        }
        catch (Exception ex)
        {
            evidence.Add(new(
                "Power",
                "ActiveScheme",
                $"Unavailable:{ex.GetType().Name}",
                EvidenceState.Warning,
                now));
        }

        evidence.Add(new(
            "Power",
            "MutationPolicy",
            "ReadOnlyDiagnostic",
            EvidenceState.Pass,
            DateTimeOffset.UtcNow));

        return new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings);
    }
}
