using System.Text;
using ZeroTrace.Core.Models;
namespace ZeroTrace.Reporting;
public sealed class MarkdownReportBuilder { public string Build(DiagnosticResult r) { var s=new StringBuilder(); s.AppendLine($"# ZeroTrace Report — {r.DiagnosticName}"); s.AppendLine($"- Success: {r.Success}"); s.AppendLine("## Evidence"); foreach(var e in r.Evidence) s.AppendLine($"- **{e.Source}/{e.Key}**: {e.Value} ({e.State})"); s.AppendLine("## Findings"); foreach(var f in r.Findings) s.AppendLine($"- **{f.Id} — {f.Title}** ({f.Confidence}% confidence)"); return s.ToString(); } }
