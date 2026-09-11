using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsWinsockCatalogPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Winsock Catalog Posture";

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
                    Arguments = "winsock show catalog",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            evidence.Add(new("WinsockCatalogPosture", "CatalogReadable",
                process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) ? "True" : "False",
                EvidenceState.Info, now));
            evidence.Add(new("WinsockCatalogPosture", "OutputLength",
                output.Length.ToString(), EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("WinsockCatalogPosture", "CatalogReadable", "False", EvidenceState.Info, now));
        }

        evidence.Add(new("WinsockCatalogPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
