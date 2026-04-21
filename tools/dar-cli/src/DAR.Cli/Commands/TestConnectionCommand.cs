using System.CommandLine;
using System.Diagnostics;
using DAR.Cli.Brand;
using Spectre.Console;

namespace DAR.Cli.Commands;

/// <summary>
/// aec test-connection — probe running COM hosts to verify they're connectable.
///
/// Uses PowerShell on Windows (via WSL interop if on Linux) to test
/// Marshal.GetActiveObject / CreateObject for each known COM ProgID.
/// </summary>
public static class TestConnectionCommand
{
    // Known COM ProgIDs for each host
    private static readonly (string Name, string ProgId, string? DllCheck)[] Hosts =
    [
        ("Civil 3D",  "AutoCAD.Application",              null),
        ("CSiBridge",  "CSI.CSiBridge.API.SapObject",      "CSiBridge1"),
        ("SAP2000",    "CSI.SAP2000.API.SapObject",        "SAP2000v1"),
        ("ETABS",      "CSI.ETABS.API.ETABSObject",        "ETABSv1"),
    ];

    public static Command Build()
    {
        var cmd = new Command("test-connection", "Probe running CAD/analysis applications for COM connectivity");

        cmd.SetHandler(() =>
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[teal]{BrandConfig.ToolName} test-connection[/] — probing COM hosts...\n");

            // Build a PowerShell script that tests each ProgID
            var psScript = BuildPowerShellScript();

            // Execute via powershell.exe (works from WSL too via interop)
            var psExe = OperatingSystem.IsLinux()
                ? "/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"
                : "powershell.exe";

            if (OperatingSystem.IsLinux() && !File.Exists(psExe))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] PowerShell not found at expected path.");
                AnsiConsole.MarkupLine("[grey]This command requires Windows PowerShell (available via WSL interop).[/]");
                Environment.Exit(1);
                return;
            }

            try
            {
                // Pass script inline via -Command (avoids WSL path issues with -File)
                // Encode as base64 to avoid quoting nightmares
                var bytes    = System.Text.Encoding.Unicode.GetBytes(psScript);
                var encoded  = Convert.ToBase64String(bytes);

                var psi = new ProcessStartInfo
                {
                    FileName               = psExe,
                    Arguments              = $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                };

                using var proc = Process.Start(psi)!;
                var output = proc.StandardOutput.ReadToEnd();
                var errors = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                // Parse results — each line is: STATUS|Name|Detail
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Grey)
                    .AddColumn("[teal]Host[/]")
                    .AddColumn("[teal]Status[/]")
                    .AddColumn("[teal]Detail[/]");

                foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Trim().Split('|', 3);
                    if (parts.Length < 3) continue;

                    var status = parts[0] switch
                    {
                        "OK"   => "[teal]● Connected[/]",
                        "FAIL" => "[red]○ Not running[/]",
                        "ERR"  => "[yellow]⚠ Error[/]",
                        _      => "[grey]? Unknown[/]"
                    };

                    table.AddRow(
                        Markup.Escape(parts[1]),
                        status,
                        Markup.Escape(parts[2]));
                }

                AnsiConsole.Write(table);

                // Filter out PowerShell progress CLIXML noise
                var realErrors = errors
                    .Split('\n')
                    .Where(l => !string.IsNullOrWhiteSpace(l) && !l.Contains("CLIXML") && !l.Contains("<Objs") && !l.Contains("</Objs>") && !l.Contains("<TN") && !l.Contains("</TN>") && !l.Contains("<MS>") && !l.Contains("</MS>") && !l.Contains("SourceId") && !l.Contains("Preparing modules"))
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .ToList();

                if (realErrors.Count > 0)
                    AnsiConsole.MarkupLine($"\n[yellow]Errors:[/]\n[grey]{Markup.Escape(string.Join("\n", realErrors))}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                Environment.Exit(1);
            }

            AnsiConsole.WriteLine();
        });

        return cmd;
    }

    private static string BuildPowerShellScript()
    {
        // PowerShell script that tries to get each COM object and reports status
        var lines = new List<string>
        {
            "$ErrorActionPreference = 'SilentlyContinue'",
            ""
        };

        foreach (var (name, progId, _) in Hosts)
        {
            lines.Add($@"try {{");
            lines.Add($@"    $obj = [System.Runtime.InteropServices.Marshal]::GetActiveObject('{progId}')");
            lines.Add($@"    if ($obj -ne $null) {{");

            if (name == "Civil 3D")
            {
                lines.Add($@"        $doc = $obj.ActiveDocument.Name");
                lines.Add($@"        Write-Output ""OK|{name}|Active document: $doc""");
            }
            else
            {
                // CSi products — try to get model info
                lines.Add($@"        try {{");
                lines.Add($@"            $model = $obj.SapModel");
                lines.Add($@"            $file = $model.GetModelFilename()");
                lines.Add($@"            Write-Output ""OK|{name}|Model: $file""");
                lines.Add($@"        }} catch {{");
                lines.Add($@"            Write-Output ""OK|{name}|Connected (no model info)""");
                lines.Add($@"        }}");
            }

            lines.Add($@"    }} else {{");
            lines.Add($@"        Write-Output ""FAIL|{name}|Not running""");
            lines.Add($@"    }}");
            lines.Add($@"}} catch {{");
            lines.Add($@"    Write-Output ""FAIL|{name}|Not running or COM disabled""");
            lines.Add($@"}}");
            lines.Add("");
        }

        return string.Join("\r\n", lines);
    }

    /// <summary>Convert a Linux path to Windows path for PowerShell.</summary>
    private static string ConvertPath(string path)
    {
        if (!OperatingSystem.IsLinux()) return path;
        // /mnt/c/Temp/foo.ps1 → C:\Temp\foo.ps1
        if (path.StartsWith("/mnt/") && path.Length > 6)
            return char.ToUpper(path[5]) + ":\\" + path[7..].Replace('/', '\\');
        return path;
    }
}
