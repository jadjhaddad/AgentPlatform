using System.CommandLine;
using DAR.Cli.Brand;
using DAR.Cli.Commands;
using DAR.Cli.Tests;
using Spectre.Console;

var root = new RootCommand($"{BrandConfig.ToolName} — AEC plugin project scaffolding tool");

root.AddCommand(NewCommand.Build());
root.AddCommand(ListCommand.Build());
root.AddCommand(VersionCommand.Build());
root.AddCommand(DeployCommand.Build());
root.AddCommand(UpgradeCommand.Build());
root.AddCommand(BrandCommand.Build());
root.AddCommand(TestConnectionCommand.Build());

// Dev-only test command
var testCmd = new Command("test-scaffold", "Run scaffold smoke tests");
testCmd.SetHandler(() =>
{
    var output = Path.Combine(Path.GetTempPath(), "dar-scaffold-test");
    Console.WriteLine($"Scaffolding test projects to: {output}");
    ScaffoldTest.RunAll(output);
});
root.AddCommand(testCmd);

// Default: show help if no subcommand given
root.SetHandler(() =>
{
    var t = BrandConfig.ToolName;
    AnsiConsole.MarkupLine($"[grey]Usage:[/]  [white]{t} new[/]       — scaffold a new project");
    AnsiConsole.MarkupLine($"        [white]{t} list[/]      — list available templates");
    AnsiConsole.MarkupLine($"        [white]{t} version[/]   — show detected installed products");
    AnsiConsole.MarkupLine($"        [white]{t} deploy[/]    — build and deploy a project");
    AnsiConsole.MarkupLine($"        [white]{t} upgrade[/]   — patch an existing project to latest conventions");
    AnsiConsole.MarkupLine($"        [white]{t} brand[/]     — create a branded fork of this tool");
    AnsiConsole.MarkupLine($"        [white]{t} test-connection[/] — probe running COM hosts");
    AnsiConsole.WriteLine();
});

return await root.InvokeAsync(args);
