// Quick integration smoke test — not a proper test project, just a dev helper.
// Run via: dotnet run --project src/DAR.Cli -- test-scaffold
using DAR.Cli.Models;
using DAR.Cli.Scaffolding;

namespace DAR.Cli.Tests;

public static class ScaffoldTest
{
    public static void RunAll(string baseOutput)
    {
        var tests = new List<(string name, ProjectConfig cfg, string buildConfig)>
        {
            // ── Revit ──────────────────────────────────────────────────────────
            ("Revit_Modal",         new ProjectConfig {
                ProjectName = "TestRevitModal",
                OutputPath  = Path.Combine(baseOutput, "TestRevitModal"),
                HostApp     = HostApp.Revit,
                PluginType  = PluginType.RibbonModal,
                Versions    = new() { "2024", "2025", "2026" }
            }, "RVT2024"),

            ("Revit_Modeless",      new ProjectConfig {
                ProjectName = "TestRevitModeless",
                OutputPath  = Path.Combine(baseOutput, "TestRevitModeless"),
                HostApp     = HostApp.Revit,
                PluginType  = PluginType.RibbonModeless,
                Versions    = new() { "2024", "2025" }
            }, "RVT2024"),

            ("Revit_CommandOnly",   new ProjectConfig {
                ProjectName = "TestRevitCmd",
                OutputPath  = Path.Combine(baseOutput, "TestRevitCmd"),
                HostApp     = HostApp.Revit,
                PluginType  = PluginType.CommandOnly,
                Versions    = new() { "2025" }
            }, "RVT2025"),

            ("Revit_EmbeddedServer", new ProjectConfig {
                ProjectName = "TestRevitServer",
                OutputPath  = Path.Combine(baseOutput, "TestRevitServer"),
                HostApp     = HostApp.Revit,
                PluginType  = PluginType.EmbeddedServer,
                Versions    = new() { "2025", "2026" }
            }, "RVT2025"),

            // ── Civil 3D ───────────────────────────────────────────────────────
            ("Civil3D_Modal",       new ProjectConfig {
                ProjectName = "TestCivilModal",
                OutputPath  = Path.Combine(baseOutput, "TestCivilModal"),
                HostApp     = HostApp.Civil3D,
                PluginType  = PluginType.RibbonModal,
                Versions    = new() { "2024", "2025", "2026" }
            }, "C3D2024"),

            ("Civil3D_Modeless",    new ProjectConfig {
                ProjectName = "TestCivilModeless",
                OutputPath  = Path.Combine(baseOutput, "TestCivilModeless"),
                HostApp     = HostApp.Civil3D,
                PluginType  = PluginType.RibbonModeless,
                Versions    = new() { "2025" }
            }, "C3D2025"),

            ("Civil3D_CommandOnly", new ProjectConfig {
                ProjectName = "TestCivilCmd",
                OutputPath  = Path.Combine(baseOutput, "TestCivilCmd"),
                HostApp     = HostApp.Civil3D,
                PluginType  = PluginType.CommandOnly,
                Versions    = new() { "2024", "2025" }
            }, "C3D2024"),

            ("Civil3D_EmbeddedServer", new ProjectConfig {
                ProjectName = "TestCivilServer",
                OutputPath  = Path.Combine(baseOutput, "TestCivilServer"),
                HostApp     = HostApp.Civil3D,
                PluginType  = PluginType.EmbeddedServer,
                Versions    = new() { "2025" }
            }, "C3D2025"),

            // ── CSi ────────────────────────────────────────────────────────────
            ("CSiBridge_Standard",  new ProjectConfig {
                ProjectName = "TestCsiBridgeStd",
                OutputPath  = Path.Combine(baseOutput, "TestCsiBridgeStd"),
                HostApp     = HostApp.CSiBridge,
                PluginType  = PluginType.CsiStandard,
                Versions    = new() { "v25", "v26" }
            }, "CSiBridge_v25"),

            ("CSiBridge_Standalone", new ProjectConfig {
                ProjectName = "TestCsiBridgeSa",
                OutputPath  = Path.Combine(baseOutput, "TestCsiBridgeSa"),
                HostApp     = HostApp.CSiBridge,
                PluginType  = PluginType.CsiStandalone,
                Versions    = new() { "v25", "v26" }
            }, "CSiBridge_v25"),

            ("SAP2000_Standard",    new ProjectConfig {
                ProjectName = "TestSapStd",
                OutputPath  = Path.Combine(baseOutput, "TestSapStd"),
                HostApp     = HostApp.SAP2000,
                PluginType  = PluginType.CsiStandard,
                Versions    = new() { "v26" }
            }, "SAP2000_v26"),

            ("SAP2000_Standalone",  new ProjectConfig {
                ProjectName = "TestSapSa",
                OutputPath  = Path.Combine(baseOutput, "TestSapSa"),
                HostApp     = HostApp.SAP2000,
                PluginType  = PluginType.CsiStandalone,
                Versions    = new() { "v26" }
            }, "SAP2000_v26"),

            ("ETABS_Standard",      new ProjectConfig {
                ProjectName = "TestEtabsStd",
                OutputPath  = Path.Combine(baseOutput, "TestEtabsStd"),
                HostApp     = HostApp.ETABS,
                PluginType  = PluginType.CsiStandard,
                Versions    = new() { "v22" }
            }, "ETABS_v22"),

            ("ETABS_Standalone",    new ProjectConfig {
                ProjectName = "TestEtabsSa",
                OutputPath  = Path.Combine(baseOutput, "TestEtabsSa"),
                HostApp     = HostApp.ETABS,
                PluginType  = PluginType.CsiStandalone,
                Versions    = new() { "v22" }
            }, "ETABS_v22"),

            // ── COM ────────────────────────────────────────────────────────────
            ("COM_Civil3D",         new ProjectConfig {
                ProjectName = "TestComCivil",
                OutputPath  = Path.Combine(baseOutput, "TestComCivil"),
                HostApp     = HostApp.ComCivil3D,
                PluginType  = PluginType.ComClient,
            }, "Debug"),

            ("COM_SAP2000",         new ProjectConfig {
                ProjectName = "TestComSap",
                OutputPath  = Path.Combine(baseOutput, "TestComSap"),
                HostApp     = HostApp.ComSAP2000,
                PluginType  = PluginType.ComClient,
            }, "Debug"),

            ("COM_ETABS",           new ProjectConfig {
                ProjectName = "TestComEtabs",
                OutputPath  = Path.Combine(baseOutput, "TestComEtabs"),
                HostApp     = HostApp.ComETABS,
                PluginType  = PluginType.ComClient,
            }, "Debug"),

            ("COM_CSiBridge",       new ProjectConfig {
                ProjectName = "TestComCsiBridge",
                OutputPath  = Path.Combine(baseOutput, "TestComCsiBridge"),
                HostApp     = HostApp.ComCSiBridge,
                PluginType  = PluginType.ComClient,
            }, "Debug"),

            // ── net48-only Revit (no runtimeconfig expected) ───────────────────
            ("Revit_Net48Only", new ProjectConfig {
                ProjectName = "TestRevitNet48",
                OutputPath  = Path.Combine(baseOutput, "TestRevitNet48"),
                HostApp     = HostApp.Revit,
                PluginType  = PluginType.RibbonModal,
                Versions    = new() { "2023", "2024" }
            }, "RVT2024"),

            // ── Single-version projects ────────────────────────────────────────
            ("Revit_SingleVersion", new ProjectConfig {
                ProjectName = "TestRevitSingle",
                OutputPath  = Path.Combine(baseOutput, "TestRevitSingle"),
                HostApp     = HostApp.Revit,
                PluginType  = PluginType.RibbonModal,
                Versions    = new() { "2025" }
            }, "RVT2025"),

            ("Civil3D_SingleVersion", new ProjectConfig {
                ProjectName = "TestCivilSingle",
                OutputPath  = Path.Combine(baseOutput, "TestCivilSingle"),
                HostApp     = HostApp.Civil3D,
                PluginType  = PluginType.RibbonModal,
                Versions    = new() { "2024" }
            }, "C3D2024"),

            ("SAP2000_SingleVersion", new ProjectConfig {
                ProjectName = "TestSapSingle",
                OutputPath  = Path.Combine(baseOutput, "TestSapSingle"),
                HostApp     = HostApp.SAP2000,
                PluginType  = PluginType.CsiStandard,
                Versions    = new() { "v26" }
            }, "SAP2000_v26"),

            // ── Multi-COM ──────────────────────────────────────────────────────
            ("MultiCom_C3D_CSi",    new ProjectConfig {
                ProjectName = "TestMultiCom1",
                OutputPath  = Path.Combine(baseOutput, "TestMultiCom1"),
                HostApp     = HostApp.MultiCom,
                PluginType  = PluginType.MultiCom,
                ComHosts    = new() { ComHost.Civil3D, ComHost.CSiBridge }
            }, "Debug"),

            ("MultiCom_AllFour",    new ProjectConfig {
                ProjectName = "TestMultiCom2",
                OutputPath  = Path.Combine(baseOutput, "TestMultiCom2"),
                HostApp     = HostApp.MultiCom,
                PluginType  = PluginType.MultiCom,
                ComHosts    = new() { ComHost.Civil3D, ComHost.SAP2000, ComHost.ETABS, ComHost.CSiBridge }
            }, "Debug"),

            // ── Dynamo ─────────────────────────────────────────────────────────
            ("Dynamo_Library",      new ProjectConfig {
                ProjectName = "TestDynaLib",
                OutputPath  = Path.Combine(baseOutput, "TestDynaLib"),
                HostApp     = HostApp.DynamoZeroTouch,
                PluginType  = PluginType.ZeroTouchLibrary,
                Versions    = new() { "2024", "2025" }
            }, "C3D2024"),

            ("Dynamo_WithUI",       new ProjectConfig {
                ProjectName = "TestDynaUI",
                OutputPath  = Path.Combine(baseOutput, "TestDynaUI"),
                HostApp     = HostApp.DynamoZeroTouch,
                PluginType  = PluginType.ZeroTouchWithUI,
                Versions    = new() { "2025" }
            }, "C3D2025"),
        };

        var pass = 0; var fail = 0;
        foreach (var (name, cfg, buildConfig) in tests)
        {
            try
            {
                if (Directory.Exists(cfg.OutputPath))
                    Directory.Delete(cfg.OutputPath, recursive: true);

                var scaffolder = ScaffolderFactory.Create(cfg);
                scaffolder.Scaffold();

                // Basic check: solution file exists
                var slnFiles = Directory.GetFiles(cfg.OutputPath, "*.sln", SearchOption.AllDirectories);
                if (slnFiles.Length == 0)
                    throw new Exception("No .sln file generated");

                // Build check
                var sln = slnFiles[0];
                var args = buildConfig == "Debug"
                    ? $"build \"{sln}\""
                    : $"build \"{sln}\" -c {buildConfig}";

                var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName               = "dotnet",
                    Arguments              = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                })!;
                var stdout = proc.StandardOutput.ReadToEnd();
                var stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    var firstError = (stdout + stderr)
                        .Split('\n')
                        .FirstOrDefault(l => l.Contains(": error "))
                        ?.Trim() ?? "build failed";
                    throw new Exception($"build [{buildConfig}] FAILED — {firstError}");
                }

                // ── Runtime reflection check ──────────────────────────────
                // Find the output DLL and verify the expected entry-point class loads
                var dllName     = cfg.ProjectName + ".dll";
                var outDir      = Path.Combine(cfg.OutputPath, cfg.ProjectName, "bin", buildConfig);
                var dllPath     = Path.Combine(outDir, dllName);
                string? reflectWarn = null;

                if (File.Exists(dllPath))
                {
                    try
                    {
                        // Use MetadataLoadContext-style approach: catch TypeLoadException from
                        // missing host DLLs (RevitAPI, acdbmgd, etc.) and fall back to exported types
                        System.Reflection.Assembly asm;
                        System.Type[] types;
                        try
                        {
                            asm   = System.Reflection.Assembly.LoadFrom(dllPath);
                            types = asm.GetTypes();
                        }
                        catch (System.Reflection.ReflectionTypeLoadException rtle)
                        {
                            // Partial load — host DLLs not available on Linux; use what we got
                            asm   = rtle.Types.FirstOrDefault(t => t is not null)?.Assembly
                                    ?? System.Reflection.Assembly.LoadFrom(dllPath);
                            types = rtle.Types.Where(t => t is not null).ToArray()!;
                        }

                        var typeNames  = types.Select(t => t.FullName ?? "").ToHashSet();
                        var ns         = cfg.ProjectName;
                        var entryPoint = cfg.HostApp switch
                        {
                            HostApp.Revit           => $"{ns}.Application",
                            HostApp.Civil3D         => $"{ns}.Application",
                            HostApp.CSiBridge or
                            HostApp.SAP2000  or
                            HostApp.ETABS           => $"{ns}.cPlugin",
                            HostApp.DynamoZeroTouch => $"{ns}.{cfg.ProjectName}Nodes",
                            _                       => null
                        };

                        // Only warn on Windows where host DLLs are available for full load
                        if (entryPoint is not null && !typeNames.Contains(entryPoint) && !OperatingSystem.IsLinux())
                            reflectWarn = $"[warn] entry point '{entryPoint}' not found in DLL";
                    }
                    catch (Exception rex)
                    {
                        // On Linux, host DLLs (RevitAPI, acdbmgd, etc.) aren't available —
                        // reflection failure here is expected and not a real problem.
                        if (OperatingSystem.IsLinux())
                            reflectWarn = null; // suppress on Linux
                        else
                            reflectWarn = $"[warn] reflection: {rex.Message.Split('\n')[0]}";
                    }
                }

                var suffix = reflectWarn is not null ? $" {reflectWarn}" : "";
                Console.WriteLine($"  [PASS] {name,-30} build [{buildConfig}]{suffix}");
                pass++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [FAIL] {name,-30} {ex.Message}");
                fail++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Results: {pass} passed, {fail} failed");
        if (fail > 0) Environment.Exit(1);
    }
}
