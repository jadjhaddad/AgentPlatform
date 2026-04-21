using System.CommandLine;
using DAR.Cli.Detection;
using Spectre.Console;

namespace DAR.Cli.Commands;

public static class ListCommand
{
    public static Command Build()
    {
        var cmd = new Command("list", "List all available project templates");

        cmd.SetHandler(() =>
        {
            static string Versions(IReadOnlyList<string> v)
                => v.Count == 0 ? "[grey]not installed[/]" : $"[teal]{string.Join(", ", v)}[/]";

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn("[teal]Host[/]")
                .AddColumn("[teal]Type[/]")
                .AddColumn("[teal]Installed[/]")
                .AddColumn("[teal]Description[/]");

            var rvt  = Versions(InstalledVersions.Revit);
            var c3d  = Versions(InstalledVersions.Civil3D);
            var csib = Versions(InstalledVersions.CSiBridge);
            var sap  = Versions(InstalledVersions.SAP2000);
            var etbs = Versions(InstalledVersions.ETABS);
            var dyn  = Versions(InstalledVersions.DynamoC3D);

            table.AddRow("Revit",      "Ribbon — Modal",         rvt,  "IExternalApplication + ribbon + modal WPF window");
            table.AddRow("Revit",      "Ribbon — Modeless",      rvt,  "Ribbon + persistent WPF window via ExternalEvent");
            table.AddRow("Revit",      "Command Only",           rvt,  "Bare IExternalCommand, no ribbon");
            table.AddRow("Revit",      "Embedded Server",        rvt,  "Plugin hosts local HTTP API (2025+ only)");
            table.AddRow("Civil 3D",   "Ribbon — Modal",         c3d,  "IExtensionApplication + ribbon + modal WPF window");
            table.AddRow("Civil 3D",   "Ribbon — Modeless",      c3d,  "Ribbon + persistent window via LockDocument");
            table.AddRow("Civil 3D",   "Command Only",           c3d,  "Bare [[CommandMethod]], no ribbon");
            table.AddRow("Civil 3D",   "Embedded Server",        c3d,  "Plugin hosts local HTTP API");
            table.AddRow("CSiBridge",  "Standard",               csib, "cPlugin entry point, WinForms, in-process");
            table.AddRow("CSiBridge",  "Standalone",             csib, "Shim DLL + separate WPF exe");
            table.AddRow("SAP2000",    "Standard",               sap,  "cPlugin entry point, WinForms, in-process");
            table.AddRow("SAP2000",    "Standalone",             sap,  "Shim DLL + separate WPF exe");
            table.AddRow("ETABS",      "Standard",               etbs, "cPlugin entry point, WinForms, in-process");
            table.AddRow("ETABS",      "Standalone",             etbs, "Shim DLL + separate WPF exe");
            table.AddRow("COM Client", "Civil 3D",               c3d,  "WPF exe, connects to running Civil 3D via COM");
            table.AddRow("COM Client", "SAP2000 / ETABS / CSiB", sap,  "WPF exe, connects via cHelper.GetObject");
            table.AddRow("Dynamo",     "Zero-Touch Library",     dyn,  "Static node library for Civil 3D Dynamo");
            table.AddRow("Dynamo",     "Zero-Touch + UI",        dyn,  "Node library + WPF dialog nodes");
            table.AddRow("Multi-COM",  "Multi-COM Client",       "[teal]any[/]", "WPF exe connecting to 2+ hosts simultaneously");

            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        });

        return cmd;
    }
}
