using System.CommandLine;
using DAR.Cli.Brand;
using DAR.Cli.Detection;
using Spectre.Console;

namespace DAR.Cli.Commands;

public static class VersionCommand
{
    public static Command Build()
    {
        var cmd = new Command("version", "Show detected installed Autodesk / CSi products");

        cmd.SetHandler(() =>
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[teal]{BrandConfig.ToolName} — Detected installations:[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(InstalledVersions.Summary());
            AnsiConsole.WriteLine();
        });

        return cmd;
    }
}
