using System.CommandLine;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using DAR.Cli.Brand;
using Spectre.Console;

namespace DAR.Cli.Commands;

/// <summary>
/// dar brand — create a fully branded fork of the tool source.
///
/// Extracts the embedded source snapshot, patches BrandConfig.cs and
/// the csproj, renames files, and optionally builds + packs a new
/// branded dotnet tool ready to install.
/// </summary>
public static class BrandCommand
{
    public static Command Build()
    {
        var cmd = new Command("brand", "Create a branded fork of this tool for your organisation");

        var outputOpt = new Option<string?>("--output", () => null,
            "Directory to create the branded fork in (defaults to ./<tool-name>)");
        outputOpt.AddAlias("-o");

        var buildOpt = new Option<bool>("--build",
            "Build and pack the branded tool after creation");

        cmd.AddOption(outputOpt);
        cmd.AddOption(buildOpt);

        cmd.SetHandler((string? output, bool build) =>
        {
            // ── Check embedded source snapshot exists ─────────────────────
            var asm     = Assembly.GetExecutingAssembly();
            var zipRes  = asm.GetManifestResourceNames()
                             .FirstOrDefault(n => n.EndsWith("source.zip"));

            if (zipRes is null)
            {
                AnsiConsole.MarkupLine("[red]Error:[/] No source snapshot found in this build.");
                AnsiConsole.MarkupLine("[grey]Run [white]dotnet pack -p:PackAsTool=true[/] to embed the source, then reinstall.[/]");
                Environment.Exit(1);
                return;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[teal]{BrandConfig.ToolName} brand[/] — Create a branded fork\n");

            // ── Prompt ────────────────────────────────────────────────────
            var toolName   = AnsiConsole.Ask<string>("[teal]CLI command name[/] [grey](e.g. acme):[/]").Trim().ToLower();
            var packageId  = AnsiConsole.Ask<string>("[teal]NuGet package ID[/] [grey](e.g. Acme.Cli):[/]",
                                $"{char.ToUpper(toolName[0])}{toolName[1..]}.Cli").Trim();
            var company    = AnsiConsole.Ask<string>("[teal]Company / author name[/] [grey](e.g. Acme Corp):[/]").Trim();
            var subtitle   = AnsiConsole.Ask<string>("[teal]Banner subtitle[/]",
                                $"{company} Project Scaffold").Trim();
            var logoPath   = AnsiConsole.Ask<string>($"[teal]Logo PNG path[/] [grey](leave blank to keep {BrandConfig.LogoFileName}):[/]", "").Trim();

            var hasLogo    = !string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath);
            var logoFile   = hasLogo ? Path.GetFileName(logoPath) : BrandConfig.LogoFileName;

            if (!string.IsNullOrWhiteSpace(logoPath) && !hasLogo)
                AnsiConsole.MarkupLine($"[yellow]⚠[/]  Logo file not found — keeping {BrandConfig.LogoFileName}");

            // ── Output directory ──────────────────────────────────────────
            var outDir = !string.IsNullOrWhiteSpace(output)
                ? Path.GetFullPath(output)
                : Path.Combine(Directory.GetCurrentDirectory(), toolName);

            if (Directory.Exists(outDir))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Output directory already exists: {outDir}");
                Environment.Exit(1);
                return;
            }

            AnsiConsole.WriteLine();

            // ── Extract source snapshot ───────────────────────────────────
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("teal"))
                .Start("Extracting source...", ctx =>
                {
                    using var stream = asm.GetManifestResourceStream(zipRes)!;
                    using var zip    = new ZipArchive(stream, ZipArchiveMode.Read);

                    foreach (var entry in zip.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name)) continue; // directory entries

                        // Strip leading "src/" prefix so output is rooted at the project
                        var rel  = entry.FullName.StartsWith("src/") ? entry.FullName[4..] : entry.FullName;
                        var dest = Path.Combine(outDir, rel.Replace('/', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

                        using var src  = entry.Open();
                        using var file = File.Create(dest);
                        src.CopyTo(file);
                    }

                    ctx.Status("Applying brand...");

                    // ── Derive namespace from packageId (e.g. Acme.Cli) ──
                    var srcFolder   = BrandConfig.SourceProjectFolder;   // "DAR.Cli"
                    var srcNs       = BrandConfig.RootNamespace;         // "DAR.Cli"
                    var newNs       = packageId;                          // "Acme.Cli"
                    var srcProjDir  = Path.Combine(outDir, srcFolder);
                    var newProjDir  = Path.Combine(outDir, newNs);

                    // ── Clean up files that shouldn't be in the fork ──────
                    foreach (var f in new[] { "Assets/source.zip", "Assets/_zip.py" })
                    {
                        var fp = Path.Combine(srcProjDir, f.Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(fp)) File.Delete(fp);
                    }

                    // ── Replace logo asset ────────────────────────────────
                    if (hasLogo)
                    {
                        var assetsDir = Path.Combine(srcProjDir, "Assets");
                        var oldLogo   = Path.Combine(assetsDir, BrandConfig.LogoFileName);
                        if (File.Exists(oldLogo) && logoFile != BrandConfig.LogoFileName)
                            File.Delete(oldLogo);
                        File.Copy(logoPath!, Path.Combine(assetsDir, logoFile), overwrite: true);
                    }

                    // ── Patch all .cs files — namespace + using renames ───
                    ctx.Status("Patching namespaces...");
                    foreach (var cs in Directory.GetFiles(srcProjDir, "*.cs", SearchOption.AllDirectories))
                    {
                        var content = File.ReadAllText(cs);
                        // Rename namespace and using declarations
                        content = content
                            .Replace($"namespace {srcNs}",  $"namespace {newNs}")
                            .Replace($"using {srcNs}",      $"using {newNs}");
                        File.WriteAllText(cs, content);
                    }

                    // ── Patch BrandConfig.cs ──────────────────────────────
                    var brandFile = Path.Combine(srcProjDir, "Brand", "BrandConfig.cs");
                    if (File.Exists(brandFile))
                    {
                        var content = File.ReadAllText(brandFile);
                        content = SetConst(content, "ToolName",             toolName);
                        content = SetConst(content, "PackageId",            packageId);
                        content = SetConst(content, "SourceProjectFolder",  newNs);
                        content = SetConst(content, "RootNamespace",        newNs);
                        content = SetConst(content, "DefaultAuthor",        company);
                        content = SetConst(content, "DefaultVendorId",      company);
                        content = SetConst(content, "LogoFileName",         logoFile);
                        content = SetConst(content, "BannerSubtitle",       subtitle);
                        File.WriteAllText(brandFile, content);
                    }

                    // ── Patch csproj ──────────────────────────────────────
                    var csprojSrc  = Path.Combine(srcProjDir, $"{srcFolder}.csproj");
                    var csprojDest = Path.Combine(srcProjDir, $"{packageId}.csproj");
                    if (File.Exists(csprojSrc))
                    {
                        var content = File.ReadAllText(csprojSrc);
                        content = SetXmlElement(content, "AssemblyName",   toolName);
                        content = SetXmlElement(content, "ToolCommandName", toolName);
                        content = SetXmlElement(content, "PackageId",       packageId);
                        content = SetXmlElement(content, "RootNamespace",   newNs);
                        content = SetXmlElement(content, "Version",         "1.0.0");
                        content = SetXmlElement(content, "Description",
                            $"{company} AEC plugin scaffolding CLI");

                        // Update EmbeddedResource for logo if changed
                        if (logoFile != BrandConfig.LogoFileName)
                            content = content.Replace(
                                $"Assets\\{BrandConfig.LogoFileName}",
                                $"Assets\\{logoFile}");

                        // Remove source.zip EmbeddedResource — fork doesn't embed itself yet
                        content = Regex.Replace(content,
                            @"\s*<!--[^>]*source snapshot[^>]*-->[^\n]*\n",
                            "\n", RegexOptions.IgnoreCase);
                        content = Regex.Replace(content,
                            @"\s*<EmbeddedResource Include=""Assets\\source\.zip""[^/]*/>\s*\n",
                            "\n");

                        // Remove PackSourceSnapshot target block
                        content = Regex.Replace(content,
                            @"\s*<!--\s*BeforePack[\s\S]*?</Target>\s*\n",
                            "\n");

                        File.WriteAllText(csprojDest, content);
                        File.Delete(csprojSrc);
                    }

                    // ── Rename project folder DAR.Cli → {newNs} ───────────
                    ctx.Status($"Renaming {srcFolder} → {newNs}...");
                    if (srcProjDir != newProjDir && Directory.Exists(srcProjDir))
                        Directory.Move(srcProjDir, newProjDir);

                    // ── Write solution file ───────────────────────────────
                    var projGuid   = Guid.NewGuid().ToString().ToUpper();
                    var slnContent = "\r\nMicrosoft Visual Studio Solution File, Format Version 12.00\r\n"
                        + "# Visual Studio Version 17\r\n"
                        + $"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{packageId}\", \"{newNs}\\{packageId}.csproj\", \"{{{projGuid}}}\"\r\n"
                        + "EndProject\r\n"
                        + "Global\r\n"
                        + "\tGlobalSection(SolutionConfigurationPlatforms) = preSolution\r\n"
                        + "\t\tDebug|Any CPU = Debug|Any CPU\r\n"
                        + "\t\tRelease|Any CPU = Release|Any CPU\r\n"
                        + "\tEndGlobalSection\r\n"
                        + "EndGlobal\r\n";
                    File.WriteAllText(Path.Combine(outDir, $"{packageId}.sln"), slnContent);

                    ctx.Status("Done.");
                });

            // ── Build + pack (optional) ───────────────────────────────────
            if (build)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[teal]Building[/] {packageId}...");

                var csproj  = Path.Combine(outDir, packageId, $"{packageId}.csproj");
                var packOut = Path.Combine(outDir, "nupkg");
                Directory.CreateDirectory(packOut);

                var exitCode = RunProcess("dotnet",
                    $"pack \"{csproj}\" -c Release -p:PackAsTool=true -o \"{packOut}\"");

                if (exitCode != 0)
                {
                    AnsiConsole.MarkupLine("[red]✗[/] Build failed.");
                    Environment.Exit(exitCode);
                    return;
                }

                AnsiConsole.MarkupLine($"[teal]✓[/] Packed to [grey]{packOut}[/]");
            }

            // ── Summary ───────────────────────────────────────────────────
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[teal]✓[/] Brand created at [grey]{outDir}[/]");
            AnsiConsole.WriteLine();
            var packOut2 = Path.Combine(outDir, "nupkg");
            AnsiConsole.MarkupLine("[grey]Next steps:[/]");
            AnsiConsole.MarkupLine($"  [white]1.[/] Optionally replace the banner art in [grey]{packageId}/Prompts/NewProjectPrompt.cs[/]");
            AnsiConsole.MarkupLine($"  [white]2.[/] Build:   [white]dotnet pack \"{outDir}/{packageId}/{packageId}.csproj\" -c Release -p:PackAsTool=true -o \"{packOut2}\"[/]");
            AnsiConsole.MarkupLine($"  [white]3.[/] Install: [white]dotnet tool install -g {packageId} --add-source \"{packOut2}\"[/]");
            AnsiConsole.MarkupLine($"  [white]4.[/] Use:     [white]{toolName} new[/]");
            AnsiConsole.WriteLine();
        }, outputOpt, buildOpt);

        return cmd;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>Replace a C# string constant value in BrandConfig.cs.</summary>
    private static string SetConst(string content, string name, string value)
        => Regex.Replace(content,
            $@"(public const string\?? {Regex.Escape(name)}\s*=\s*)"".+?""",
            $@"$1""{value}""");

    /// <summary>Replace a single-line XML element value.</summary>
    private static string SetXmlElement(string content, string element, string value)
        => Regex.Replace(content,
            $@"(<{Regex.Escape(element)}(?:\s[^>]*)?>)[^<]*(</)",
            $"$1{value}$2");

    private static int RunProcess(string exe, string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo(exe, args)
        {
            UseShellExecute = false,
        };
        using var proc = System.Diagnostics.Process.Start(psi)!;
        proc.WaitForExit();
        return proc.ExitCode;
    }
}
