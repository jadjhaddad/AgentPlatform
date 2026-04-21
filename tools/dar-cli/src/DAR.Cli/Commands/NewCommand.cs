using System.CommandLine;
using System.Diagnostics;
using DAR.Cli.Models;
using DAR.Cli.Prompts;
using DAR.Cli.Scaffolding;
using Spectre.Console;

namespace DAR.Cli.Commands;

public static class NewCommand
{
    public static Command Build()
    {
        var cmd = new Command("new", "Scaffold a new DAR plugin project");

        var nameArg         = new Argument<string?>("name", () => null, "Optional project name — skips the name prompt");
        var outputOpt       = new Option<string?>("--output", () => null, "Output directory (defaults to ./<name>)");
        var nonInteractive  = new Option<bool>("--non-interactive", "Skip all prompts — requires --host and --plugin-type");
        var hostOpt         = new Option<string?>("--host", () => null, "Host app: Revit|Civil3D|CSiBridge|SAP2000|ETABS|DynamoZeroTouch|ComCivil3D|ComSAP2000|ComETABS|ComCSiBridge|MultiCom");
        var pluginTypeOpt   = new Option<string?>("--plugin-type", () => null, "Plugin type: RibbonModal|RibbonModeless|CommandOnly|EmbeddedServer|CsiStandard|CsiStandalone|ComClient|ZeroTouchLibrary|ZeroTouchWithUI|MultiCom");
        var versionsOpt     = new Option<string?>("--versions", () => null, "Comma-separated versions, e.g. 2024,2025 or v24,v25");
        var comHostsOpt     = new Option<string?>("--com-hosts", () => null, "Comma-separated COM hosts for MultiCom: Civil3D,SAP2000,ETABS,CSiBridge");
        var authorOpt       = new Option<string?>("--author", () => null, "Author / vendor name");
        var descriptionOpt  = new Option<string?>("--description", () => null, "One-line project description");

        outputOpt.AddAlias("-o");

        cmd.AddArgument(nameArg);
        cmd.AddOption(outputOpt);
        cmd.AddOption(nonInteractive);
        cmd.AddOption(hostOpt);
        cmd.AddOption(pluginTypeOpt);
        cmd.AddOption(versionsOpt);
        cmd.AddOption(comHostsOpt);
        cmd.AddOption(authorOpt);
        cmd.AddOption(descriptionOpt);

        cmd.SetHandler((
            string? name, string? output, bool ni,
            string? host, string? pluginType, string? versions,
            string? comHosts, string? author, string? description) =>
        {
            try
            {
                ProjectConfig config;

                if (ni)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        throw new InvalidOperationException("--non-interactive requires a project name argument.");
                    if (string.IsNullOrWhiteSpace(host))
                        throw new InvalidOperationException("--non-interactive requires --host.");
                    if (string.IsNullOrWhiteSpace(pluginType))
                        throw new InvalidOperationException("--non-interactive requires --plugin-type.");

                    config = BuildConfig(name, output, host, pluginType, versions, comHosts, author, description);
                }
                else
                {
                    config = NewProjectPrompt.Run(
                        string.IsNullOrWhiteSpace(name)   ? null : name,
                        string.IsNullOrWhiteSpace(output) ? null : output);
                }

                var scaffolder = ScaffolderFactory.Create(config);

                AnsiConsole.WriteLine();
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("teal"))
                    .Start($"Scaffolding [teal]{config.ProjectName}[/]...", ctx =>
                    {
                        scaffolder.Scaffold();
                        ctx.Status("Initialising git repository...");
                        InitGit(config);
                    });

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[teal]✓[/] Done! Project created at [grey]{config.OutputPath}[/]");
                AnsiConsole.MarkupLine($"[grey]  Open [white]{config.ProjectName}.sln[/] in Visual Studio.[/]");
                AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                Environment.Exit(1);
            }
        }, nameArg, outputOpt, nonInteractive, hostOpt, pluginTypeOpt, versionsOpt, comHostsOpt, authorOpt, descriptionOpt);

        return cmd;
    }

    private static ProjectConfig BuildConfig(
        string name, string? output,
        string host, string pluginType,
        string? versions, string? comHosts,
        string? author, string? description)
    {
        var config = new ProjectConfig
        {
            ProjectName = name.Trim(),
            OutputPath  = !string.IsNullOrWhiteSpace(output)
                ? Path.GetFullPath(output)
                : Path.Combine(Directory.GetCurrentDirectory(), name.Trim()),
            Author      = author?.Trim() ?? "",
            VendorId    = author?.Trim() ?? "",
            Description = description?.Trim() ?? name.Trim(),

            HostApp = host.Trim() switch
            {
                "Revit"          => HostApp.Revit,
                "Civil3D"        => HostApp.Civil3D,
                "CSiBridge"      => HostApp.CSiBridge,
                "SAP2000"        => HostApp.SAP2000,
                "ETABS"          => HostApp.ETABS,
                "DynamoZeroTouch"=> HostApp.DynamoZeroTouch,
                "ComCivil3D"     => HostApp.ComCivil3D,
                "ComSAP2000"     => HostApp.ComSAP2000,
                "ComETABS"       => HostApp.ComETABS,
                "ComCSiBridge"   => HostApp.ComCSiBridge,
                "MultiCom"       => HostApp.MultiCom,
                var h => throw new InvalidOperationException($"Unknown host: {h}"),
            },

            PluginType = pluginType.Trim() switch
            {
                "RibbonModal"       => PluginType.RibbonModal,
                "RibbonModeless"    => PluginType.RibbonModeless,
                "CommandOnly"       => PluginType.CommandOnly,
                "EmbeddedServer"    => PluginType.EmbeddedServer,
                "CsiStandard"       => PluginType.CsiStandard,
                "CsiStandalone"     => PluginType.CsiStandalone,
                "ComClient"         => PluginType.ComClient,
                "ZeroTouchLibrary"  => PluginType.ZeroTouchLibrary,
                "ZeroTouchWithUI"   => PluginType.ZeroTouchWithUI,
                "MultiCom"          => PluginType.MultiCom,
                var p => throw new InvalidOperationException($"Unknown plugin type: {p}"),
            },

            Versions = versions?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                       ?? DefaultVersions(host.Trim()),

            ComHosts = comHosts?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(h => h switch
                {
                    "Civil3D"   => ComHost.Civil3D,
                    "SAP2000"   => ComHost.SAP2000,
                    "ETABS"     => ComHost.ETABS,
                    "CSiBridge" => ComHost.CSiBridge,
                    var x => throw new InvalidOperationException($"Unknown COM host: {x}"),
                }).ToList() ?? [],
        };

        return config;
    }

    private static List<string> DefaultVersions(string host) => host switch
    {
        "Revit" or "Civil3D"   => ["2024", "2025", "2026"],
        "CSiBridge"            => ["v24", "v25", "v26"],
        "SAP2000"              => ["v24", "v25", "v26"],
        "ETABS"                => ["v21", "v22"],
        "DynamoZeroTouch"      => ["2024", "2025", "2026"],
        _                      => [],
    };

    private static void InitGit(ProjectConfig config)
    {
        try
        {
            Run("git", "init", config.OutputPath);
            Run("git", "add .", config.OutputPath);
            Run("git", $"commit -m \"chore: scaffold {config.ProjectName}\"", config.OutputPath);
        }
        catch
        {
            // git not available or failed — non-fatal
        }
    }

    private static void Run(string exe, string args, string workDir)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            WorkingDirectory       = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };
        using var proc = Process.Start(psi)!;
        proc.WaitForExit(10_000);
    }
}
