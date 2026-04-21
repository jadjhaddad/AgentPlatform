using System.CommandLine;
using System.Text.RegularExpressions;
using DAR.Cli.Scaffolding;
using DAR.Cli.Templates.Revit;
using Spectre.Console;

namespace DAR.Cli.Commands;

/// <summary>
/// dar upgrade &lt;project-dir&gt;
/// Patches an existing DAR-scaffolded project:
///   - Replaces Directory.Build.props with the latest version
///   - Updates Dynamo AfterBuild deploy paths in *.csproj files
///   - Updates DynamoVisualProgramming NuGet versions to latest known
/// </summary>
public static class UpgradeCommand
{
    // Latest Dynamo NuGet versions per C3D year
    private static readonly Dictionary<string, string> DynaVersions = new()
    {
        ["C3D2023"] = "2.16.1",
        ["C3D2024"] = "2.19.4",
        ["C3D2025"] = "3.1.0",
        ["C3D2026"] = "3.4.0",
    };

    public static Command Build()
    {
        var cmd = new Command("upgrade", "Patch an existing DAR project to the latest scaffold conventions");

        var dirArg = new Argument<string>("directory", () => ".", "Project root directory (defaults to current directory)");
        cmd.AddArgument(dirArg);

        var dryRunOpt = new Option<bool>("--dry-run", "Show what would change without writing anything");
        cmd.AddOption(dryRunOpt);

        cmd.SetHandler((string dir, bool dryRun) =>
        {
            var root = Path.GetFullPath(dir);
            if (!Directory.Exists(root))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Directory not found: {root}");
                Environment.Exit(1);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[teal]Upgrading[/] [white]{root}[/]" + (dryRun ? " [grey](dry run)[/]" : ""));
            AnsiConsole.WriteLine();

            var changes = 0;

            // ── 1. Patch Dynamo pkg.json node_libraries ───────────────────
            foreach (var pkgJson in Directory.GetFiles(root, "pkg.json", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(pkgJson);
                if (!content.Contains("\"engine\": \"dynamo\"")) continue;

                // Find assembly name from nearest csproj
                var projDir  = Path.GetDirectoryName(pkgJson)!;
                var csproj   = Directory.GetFiles(projDir, "*.csproj").FirstOrDefault();
                if (csproj is null) continue;

                var asmName = Path.GetFileNameWithoutExtension(csproj);
                var hasUi   = Directory.Exists(Path.Combine(root, asmName + ".UI"));

                var expected = hasUi
                    ? $"""
                       "{asmName}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                        "{asmName}.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
                       """
                    : $"\"{asmName}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"";

                // Check if node_libraries already has the UI entry
                if (hasUi && content.Contains($"{asmName}.UI"))
                    continue;

                AnsiConsole.MarkupLine($"  [yellow]~[/] {Path.GetRelativePath(root, pkgJson)} — adding UI DLL to node_libraries");
                if (!dryRun)
                {
                    var updated = Regex.Replace(content,
                        @"""node_libraries""\s*:\s*\[[\s\S]*?\]",
                        $"\"node_libraries\": [\n    {expected}\n  ]");
                    File.WriteAllText(pkgJson, updated);
                }
                changes++;
            }

            // ── 2. Update Dynamo NuGet versions in *.csproj ───────────────
            foreach (var csproj in Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(csproj);
                if (!content.Contains("DynamoVisualProgramming")) continue;

                var modified = content;
                foreach (var (config, ver) in DynaVersions)
                {
                    // Match any old version for this config condition
                    modified = Regex.Replace(modified,
                        $@"(Condition=""'\$\(Configuration\)'=='{config}'"">[\s\S]*?DynamoVisualProgramming\.\w+ Version="")[\d\.]+("")",
                        $"${{1}}{ver}${{2}}",
                        RegexOptions.None);
                }

                if (modified == content) continue;

                AnsiConsole.MarkupLine($"  [yellow]~[/] {Path.GetRelativePath(root, csproj)} — updated Dynamo NuGet versions");
                if (!dryRun) File.WriteAllText(csproj, modified);
                changes++;
            }

            // ── 3. Patch old Dynamo AfterBuild deploy path ────────────────
            foreach (var csproj in Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(csproj);
                if (!content.Contains("DynamoMajorVersion")) continue;

                // Old pattern: Dynamo\Dynamo Civil 3D\...
                if (!content.Contains(@"Dynamo\Dynamo Civil 3D")) continue;

                AnsiConsole.MarkupLine($"  [yellow]~[/] {Path.GetRelativePath(root, csproj)} — fixing Dynamo deploy path");
                if (!dryRun)
                {
                    var updated = content.Replace(
                        @"$(AppData)\Dynamo\Dynamo Civil 3D\$(DynamoMajorVersion)\packages",
                        @"$(DynamoPkgRootActive)");
                    File.WriteAllText(csproj, updated);
                }
                changes++;
            }

            // ── 4. Add runtimeconfig.json to Revit 2025+ projects ────────
            foreach (var csproj in Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(csproj);
                // Only Revit projects (have RevitAPI reference)
                if (!content.Contains("RevitAPI")) continue;

                // Check if any net8.0-windows config exists (RVT2025/RVT2026)
                if (!Regex.IsMatch(content, @"RVT202[5-9]")) continue;

                var projDir   = Path.GetDirectoryName(csproj)!;
                var asmName   = Path.GetFileNameWithoutExtension(csproj);
                var rcPath    = Path.Combine(projDir, $"{asmName}.runtimeconfig.json");

                if (File.Exists(rcPath)) continue;

                AnsiConsole.MarkupLine($"  [yellow]+[/] {Path.GetRelativePath(root, rcPath)} — adding runtimeconfig.json for Revit 2025+");
                if (!dryRun)
                {
                    // Write runtimeconfig.json
                    File.WriteAllText(rcPath, RevitTemplates.RuntimeConfig.Trim());

                    // Add Content item to csproj if not already there
                    if (!content.Contains("runtimeconfig.json"))
                    {
                        var rcItem =
                            $"\n  <ItemGroup>\n" +
                            $"    <Content Include=\"{asmName}.runtimeconfig.json\">\n" +
                            $"      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\n" +
                            $"    </Content>\n" +
                            $"  </ItemGroup>\n\n" +
                            $"</Project>";
                        File.WriteAllText(csproj, content.Replace("</Project>", rcItem));
                    }
                }
                changes++;
            }

            // ── 5. Patch Directory.Build.props ──────────────────────────── 
            var dbPropsPath = Path.Combine(root, "Directory.Build.props");
            if (File.Exists(dbPropsPath))
            {
                var content  = File.ReadAllText(dbPropsPath);
                var isRevit  = content.Contains("RevitDir");
                var isCivil  = content.Contains("AcadDir") || content.Contains("CivilDir");
                var isCsi    = content.Contains("CSiBridgeDir") || content.Contains("SAP2000Dir");

                // Detect outdated props — missing recently added properties
                var needsUpdate = false;
                string? newContent = null;

                var hasOldProgramFiles = content.Contains("$(ProgramFiles)\\");

                if (isCivil && (!content.Contains("DynamoPkgRoot2024") || hasOldProgramFiles))
                {
                    newContent   = SharedTemplates.DirectoryBuildPropsCivil3D;
                    needsUpdate  = true;
                }
                else if (isRevit && (!content.Contains("RevitDir2026") || hasOldProgramFiles))
                {
                    newContent   = SharedTemplates.DirectoryBuildPropsRevit;
                    needsUpdate  = true;
                }
                else if (isCsi && (!content.Contains("ETABSDir22") || hasOldProgramFiles))
                {
                    newContent   = SharedTemplates.DirectoryBuildPropsCsi;
                    needsUpdate  = true;
                }

                if (needsUpdate && newContent is not null)
                {
                    AnsiConsole.MarkupLine($"  [yellow]~[/] Directory.Build.props — updating to latest version");
                    if (!dryRun) File.WriteAllText(dbPropsPath, newContent);
                    changes++;
                }
            }

            // ── Summary ───────────────────────────────────────────────────
            AnsiConsole.WriteLine();
            if (changes == 0)
                AnsiConsole.MarkupLine("[grey]Nothing to upgrade — project is already up to date.[/]");
            else if (dryRun)
                AnsiConsole.MarkupLine($"[yellow]{changes} change(s) would be made.[/] Run without --dry-run to apply.");
            else
                AnsiConsole.MarkupLine($"[teal]✓[/] {changes} change(s) applied.");

            AnsiConsole.WriteLine();
        }, dirArg, dryRunOpt);

        return cmd;
    }
}
