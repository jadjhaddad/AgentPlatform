using DAR.Cli.Models;
using DAR.Cli.Templates.Csi;

namespace DAR.Cli.Scaffolding.Scaffolders;

public class CsiScaffolder : ScaffolderBase
{
    public CsiScaffolder(ProjectConfig config) : base(config) { }

    public override void Scaffold()
    {
        Directory.CreateDirectory(Root);
        WriteSolutionRoot(SharedTemplates.DirectoryBuildPropsCsi);

        var prefix = Config.HostApp switch
        {
            HostApp.SAP2000   => "SAP2000",
            HostApp.ETABS     => "ETABS",
            HostApp.CSiBridge => "CSiBridge",
            _ => "CSi"
        };
        WriteGitHubActions(Config.Versions.Select(v => $"{prefix}_{v}"));

        if (Config.PluginType == PluginType.CsiStandalone)
            ScaffoldStandalone();
        else
            ScaffoldStandard();
    }

    private Dictionary<string, string> CsiTokens() => new()
    {
        ["CSI_USING"] = CsiTemplates.DllName(Config.HostApp)
    };

    // ── Standard (in-process) ─────────────────────────────────────────────
    private void ScaffoldStandard()
    {
        var configs = Config.Versions
            .Select(v => CsiTemplates.BuildConfigBlock(Config.HostApp, v))
            .ToList();

        // Build a condition that is FALSE for every named config so the default ref
        // only activates when no named config is selected (IDE / Debug).
        var namedConditions = Config.Versions.Select(v =>
        {
            var prefix = Config.HostApp switch
            {
                HostApp.SAP2000   => "SAP2000",
                HostApp.ETABS     => "ETABS",
                HostApp.CSiBridge => "CSiBridge",
                _ => "CSi"
            };
            return $"'$(Configuration)'!='{prefix}_{v}'";
        });
        var defaultRefCondition = string.Join(" And ", namedConditions);

        // Pick first version as the default hint path (uses MSBuild property for WSL compat)
        var firstVersion = Config.Versions[0];
        var dll          = CsiTemplates.DllName(Config.HostApp);
        var defaultProp  = CsiTemplates.InstallPathProperty(Config.HostApp, firstVersion.TrimStart('v'));
        var defaultHint  = $@"{defaultProp}\{dll}.dll";

        Write($"{Config.ProjectName}/{Config.ProjectName}.csproj",
            CsiTemplates.CsProj,
            new()
            {
                ["BUILD_CONFIGS"]          = string.Join("\n\n", configs),
                ["DEFAULT_REF_CONDITION"]  = defaultRefCondition,
                ["CSI_DLL"]                = dll,
                ["CSI_DEFAULT_HINT"]       = defaultHint,
            });

        WriteLogger();
        Write($"{Config.ProjectName}/cPlugin.cs", CsiTemplates.CPluginStandard, CsiTokens());
        Write($"{Config.ProjectName}/MainForm.cs", CsiTemplates.MainForm, CsiTokens());

        var buildConfigs = SolutionHelper.CsiBuildConfigs(Config.HostApp, Config.Versions);
        SolutionHelper.WriteSln(
            Path.Combine(Root, $"{Config.ProjectName}.sln"),
            Config.ProjectName,
            $"{Config.ProjectName}\\{Config.ProjectName}.csproj",
            buildConfigs);
    }

    // ── Standalone (separate process — Speckle pattern) ───────────────────
    private void ScaffoldStandalone()
    {
        WriteDarLogo($"{Config.ProjectName}.App");
        var defaultProgId = CsiTemplates.ProgId(Config.HostApp);

        // Compute default ref tokens shared by Shim and App csproj
        var namedConditions = Config.Versions.Select(v =>
        {
            var prefix = Config.HostApp switch
            {
                HostApp.SAP2000   => "SAP2000",
                HostApp.ETABS     => "ETABS",
                HostApp.CSiBridge => "CSiBridge",
                _ => "CSi"
            };
            return $"'$(Configuration)'!='{prefix}_{v}'";
        });
        var defaultRefCondition = string.Join(" And ", namedConditions);
        var firstVersion  = Config.Versions[0];
        var dll           = CsiTemplates.DllName(Config.HostApp);
        var defaultPropS  = CsiTemplates.InstallPathProperty(Config.HostApp, firstVersion.TrimStart('v'));
        var defaultHint   = $@"{defaultPropS}\{dll}.dll";

        // Shim project
        var shimConfigs = Config.Versions
            .Select(v => CsiTemplates.BuildConfigBlock(Config.HostApp, v))
            .ToList();

        Write($"{Config.ProjectName}.Shim/{Config.ProjectName}.Shim.csproj",
            CsiTemplates.ShimCsProj,
            new()
            {
                ["BUILD_CONFIGS"]         = string.Join("\n\n", shimConfigs),
                ["DEFAULT_REF_CONDITION"] = defaultRefCondition,
                ["CSI_DLL"]               = dll,
                ["CSI_DEFAULT_HINT"]      = defaultHint,
            });
        Write($"{Config.ProjectName}.Shim/cPlugin.cs", CsiTemplates.CPluginStandalone, CsiTokens());

        // Core project
        Write($"{Config.ProjectName}.Core/{Config.ProjectName}.Core.csproj", CsiTemplates.CoreCsProj);

        // App project
        var csiRefs = Config.Versions
            .Select(v => BuildAppCsiReference(v))
            .ToList();

        Write($"{Config.ProjectName}.App/{Config.ProjectName}.App.csproj",
            CsiTemplates.StandaloneAppCsProj,
            new()
            {
                ["CSI_REFERENCES"]        = string.Join("\n", csiRefs),
                ["DEFAULT_REF_CONDITION"] = defaultRefCondition,
                ["CSI_DLL"]               = dll,
                ["CSI_DEFAULT_HINT"]      = defaultHint,
            });

        Write($"{Config.ProjectName}.App/App.xaml",           CsiTemplates.StandaloneAppXaml);
        Write($"{Config.ProjectName}.App/App.xaml.cs",        CsiTemplates.StandaloneAppCodeBehind,
            new() { ["DEFAULT_PROGID"] = defaultProgId });
        Write($"{Config.ProjectName}.App/MainWindow.xaml",    CsiTemplates.StandaloneMainWindowXaml,
            new() { ["TITLE_BAR_LOGO"] = TitleBarLogoXaml("") });
        Write($"{Config.ProjectName}.App/MainWindow.xaml.cs", CsiTemplates.StandaloneMainWindowCodeBehind);
        Write($"{Config.ProjectName}.App/MainWindowViewModel.cs", CsiTemplates.StandaloneViewModel, CsiTokens());

        // Shared Common/ folder in the App project
        WriteCommonFolder($"{Config.ProjectName}.App");

        // .sln with all three projects
        WriteStandaloneSolution();
    }

    private string BuildAppCsiReference(string version)
    {
        var dll      = CsiTemplates.DllName(Config.HostApp);
        var propName = CsiTemplates.InstallPathProperty(Config.HostApp, version.TrimStart('v'));
        var prefix   = Config.HostApp switch
        {
            HostApp.SAP2000   => "SAP2000",
            HostApp.ETABS     => "ETABS",
            HostApp.CSiBridge => "CSiBridge",
            _ => "CSi"
        };

        return $"""
              <ItemGroup Condition="'$(Configuration)'=='{prefix}_{version}'">
                <Reference Include="{dll}">
                  <HintPath>{propName}\{dll}.dll</HintPath>
                  <Private>False</Private>
                </Reference>
              </ItemGroup>
            """;
    }

    private void WriteStandaloneSolution()
    {
        var shimGuid = Guid.NewGuid().ToString().ToUpper();
        var appGuid  = Guid.NewGuid().ToString().ToUpper();
        var coreGuid = Guid.NewGuid().ToString().ToUpper();
        var buildConfigs = SolutionHelper.CsiBuildConfigs(Config.HostApp, Config.Versions);

        var cfgLines     = buildConfigs.Select(c => $"\t\t{c}|x64 = {c}|x64").ToList();
        var projCfgLines = new List<string>();

        foreach (var guid in new[] { shimGuid, appGuid, coreGuid })
        foreach (var cfg in buildConfigs)
        {
            projCfgLines.Add($"\t\t{{{guid}}}.{cfg}|x64.ActiveCfg = {cfg}|x64");
            projCfgLines.Add($"\t\t{{{guid}}}.{cfg}|x64.Build.0 = {cfg}|x64");
        }

        const string projTypeGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        var sln = string.Join("\n",
            "",
            "Microsoft Visual Studio Solution File, Format Version 12.00",
            "# Visual Studio Version 17",
            $"Project(\"{projTypeGuid}\") = \"{Config.ProjectName}.Shim\", \"{Config.ProjectName}.Shim\\{Config.ProjectName}.Shim.csproj\", \"{{{shimGuid}}}\"",
            "EndProject",
            $"Project(\"{projTypeGuid}\") = \"{Config.ProjectName}.App\", \"{Config.ProjectName}.App\\{Config.ProjectName}.App.csproj\", \"{{{appGuid}}}\"",
            "EndProject",
            $"Project(\"{projTypeGuid}\") = \"{Config.ProjectName}.Core\", \"{Config.ProjectName}.Core\\{Config.ProjectName}.Core.csproj\", \"{{{coreGuid}}}\"",
            "EndProject",
            "Global",
            "\tGlobalSection(SolutionConfigurationPlatforms) = preSolution",
            string.Join("\n", cfgLines),
            "\tEndGlobalSection",
            "\tGlobalSection(ProjectConfigurationPlatforms) = postSolution",
            string.Join("\n", projCfgLines),
            "\tEndGlobalSection",
            "EndGlobal"
        );

        var slnPath = Path.Combine(Root, $"{Config.ProjectName}.sln");
        Directory.CreateDirectory(Path.GetDirectoryName(slnPath)!);
        File.WriteAllText(slnPath, sln);
    }
}
