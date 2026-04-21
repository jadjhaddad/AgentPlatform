using DAR.Cli.Brand;
using DAR.Cli.Models;
using DAR.Cli.Templates.Revit;

namespace DAR.Cli.Scaffolding.Scaffolders;

public class RevitScaffolder : ScaffolderBase
{
    public RevitScaffolder(ProjectConfig config) : base(config) { }

    public override void Scaffold()
    {
        Directory.CreateDirectory(Root);

        WriteSolutionRoot(SharedTemplates.DirectoryBuildPropsRevit);

        var hasUi = Config.PluginType != PluginType.CommandOnly;
        if (hasUi)
        {
            WriteCommonFolder($"{Config.ProjectName}");
            WriteDarLogo($"{Config.ProjectName}");
        }

        WriteProjectFile();
        WriteRuntimeConfigs();
        WriteAddin();
        WriteApplication();
        WriteCommand();

        switch (Config.PluginType)
        {
            case PluginType.RibbonModal:
            case PluginType.RibbonModeless:
            case PluginType.EmbeddedServer:
                WriteWindow();
                WriteViewModel();
                if (Config.PluginType is PluginType.RibbonModeless or PluginType.EmbeddedServer)
                    WriteModelessInfra();
                if (Config.PluginType == PluginType.EmbeddedServer)
                    WriteEmbeddedServer();
                break;
            // CommandOnly: no UI scaffolding at all
        }

        WriteGitHubActions(Config.Versions.Select(v => $"RVT{v}"));
        WriteSolution();
    }

    // ── .csproj ───────────────────────────────────────────────────────────
    private void WriteProjectFile()
    {
        var configs = Config.Versions.Select(v => BuildConfigBlock(v)).ToList();
        var logoFile   = BrandConfig.LogoFileName;
        var logoFolder = BrandConfig.LogoResourceSubfolder;
        var resourceItems = Config.PluginType != PluginType.CommandOnly && !string.IsNullOrEmpty(logoFile)
            ? $"""
                <ItemGroup>
                  <!-- WPF resource (XAML pack URI) -->
                  <Resource Include="{logoFolder}\{logoFile}"/>
                  <!-- Also copy to output so ribbon LoadIcon() can find it by path -->
                  <Content Include="{logoFolder}\{logoFile}">
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                    <Link>{logoFolder}\{logoFile}</Link>
                  </Content>
                </ItemGroup>
              """
            : string.Empty;

        // runtimeconfig.json — copy to output for net8.0 build configs (Revit 2025+)
        var runtimeConfigItem = Config.Versions.Any(v => int.Parse(v) >= 2025)
            ? $"""
                <ItemGroup>
                  <Content Include="{Config.ProjectName}.runtimeconfig.json">
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                  </Content>
                </ItemGroup>
              """
            : string.Empty;

        // ASP.NET Core FrameworkReference only for EmbeddedServer (needs net8.0+)
        var aspNetRef = Config.PluginType == PluginType.EmbeddedServer
            ? """
                <ItemGroup Condition="$(TargetFramework.StartsWith('net8')) Or $(TargetFramework.StartsWith('net9')) Or $(TargetFramework.StartsWith('net1'))">
                  <FrameworkReference Include="Microsoft.AspNetCore.App"/>
                </ItemGroup>
              """
            : string.Empty;

        // PackageContents.xml component entries — one per selected version
        var components = string.Join("\n", Config.Versions.Select(v =>
            $"    &lt;Components Description=&quot;Revit {v}&quot;&gt;&#xA;" +
            $"        &lt;RuntimeRequirements OS=&quot;Win64&quot; Platform=&quot;Revit&quot; SeriesMin=&quot;R{v}&quot; SeriesMax=&quot;R{v}&quot; /&gt;&#xA;" +
            $"        &lt;ComponentEntry ModuleName=&quot;./Contents/RVT{v}/$(AssemblyName).addin&quot; /&gt;&#xA;" +
            $"    &lt;/Components&gt;&#xA;"));

        Write($"{Config.ProjectName}/{Config.ProjectName}.csproj",
            RevitTemplates.CsProj,
            new()
            {
                ["BUILD_CONFIGS"]               = string.Join("\n\n", configs),
                ["RESOURCE_ITEMS"]              = resourceItems,
                ["RUNTIMECONFIG_ITEM"]          = runtimeConfigItem,
                ["ASPNET_REF"]                  = aspNetRef,
                ["PACKAGE_CONTENTS_COMPONENTS"] = components,
            });
    }

    private static string BuildConfigBlock(string version)
    {
        var tf = int.Parse(version) >= 2025 ? "net8.0-windows" : "net48";
        var define = $"RVT{version}";

        var wpfBuildService = tf == "net8.0-windows"
            ? "\n    <UseWpfBuildService>false</UseWpfBuildService>"
            : string.Empty;

        // Use $(RevitDir{version}) — defined in Directory.Build.props with WSL remapping
        return $"""
              <PropertyGroup Condition="'$(Configuration)'=='RVT{version}'">
                <TargetFramework>{tf}</TargetFramework>
                <DefineConstants>{define}</DefineConstants>
                <OutputPath>bin\RVT{version}\</OutputPath>{wpfBuildService}
              </PropertyGroup>

              <ItemGroup Condition="'$(Configuration)'=='RVT{version}'">
                <Reference Include="RevitAPI">
                  <HintPath>$(RevitDir{version})\RevitAPI.dll</HintPath>
                  <Private>False</Private>
                </Reference>
                <Reference Include="RevitAPIUI">
                  <HintPath>$(RevitDir{version})\RevitAPIUI.dll</HintPath>
                  <Private>False</Private>
                </Reference>
              </ItemGroup>
            """;
    }

    // ── runtimeconfig.json ────────────────────────────────────────────────
    // Only needed for net8.0-windows versions (Revit 2025+).
    private void WriteRuntimeConfigs()
    {
        var net8Versions = Config.Versions.Where(v => int.Parse(v) >= 2025).ToList();
        if (net8Versions.Count == 0) return;

        // One runtimeconfig.json in the project root — copied to output for each net8.0 config
        Write($"{Config.ProjectName}/{Config.ProjectName}.runtimeconfig.json",
            RevitTemplates.RuntimeConfig);
    }

    // ── .addin manifest ───────────────────────────────────────────────────
    // Written once at scaffold time — never re-generated by MSBuild so GUID stays stable.
    // Written to both the project folder (for IDE) and Resources/ (picked up by CreateBundleFolder).
    private void WriteAddin()
    {
        var guid    = Guid.NewGuid().ToString();
        var tokens  = new Dictionary<string, string> { ["ADDIN_GUID"] = guid };
        Write($"{Config.ProjectName}/{Config.ProjectName}.addin", RevitTemplates.Addin, tokens);
        Write($"Resources/{Config.ProjectName}.addin",            RevitTemplates.Addin, tokens);
    }

    // ── Application.cs ────────────────────────────────────────────────────
    private void WriteApplication()
    {
        var template = Config.PluginType switch
        {
            PluginType.RibbonModal or PluginType.RibbonModeless or PluginType.EmbeddedServer
                => RevitTemplates.ApplicationRibbon,
            PluginType.CommandOnly
                => RevitTemplates.ApplicationEmpty,
            _ => RevitTemplates.ApplicationRibbon
        };

        Write($"{Config.ProjectName}/Application.cs", template);
    }

    // ── Command.cs ────────────────────────────────────────────────────────
    private void WriteCommand()
    {
        var template = Config.PluginType switch
        {
            PluginType.CommandOnly    => RevitTemplates.CommandOnly,
            PluginType.RibbonModeless => RevitTemplates.CommandModeless,
            _                         => RevitTemplates.Command,
        };
        Write($"{Config.ProjectName}/Commands/{Config.ProjectName}Command.cs", template);
    }

    // ── WPF Window + ViewModel ────────────────────────────────────────────
    private void WriteWindow()
    {
        var codeBehind = Config.PluginType == PluginType.RibbonModeless
            ? RevitTemplates.WindowCodeBehindModeless
            : RevitTemplates.WindowCodeBehind;

        Write($"{Config.ProjectName}/UI/{Config.ProjectName}Window.xaml",     RevitTemplates.WindowXaml);
        Write($"{Config.ProjectName}/UI/{Config.ProjectName}Window.xaml.cs",  codeBehind);
    }

    private void WriteViewModel()
    {
        var template = Config.PluginType == PluginType.RibbonModeless
            ? RevitTemplates.ViewModelModeless
            : RevitTemplates.ViewModel;
        Write($"{Config.ProjectName}/UI/{Config.ProjectName}ViewModel.cs", template);
    }

    // ── Modeless infrastructure ───────────────────────────────────────────
    // Note: Revit modeless uses ExternalEvent + IHostService (not StaWindowLauncher).
    // StaWindowLauncher is Civil 3D only.
    private void WriteModelessInfra()
    {
        Write($"{Config.ProjectName}/Host/IHostService.cs",
            RevitTemplates.IHostService);
        Write($"{Config.ProjectName}/Host/RevitHostService.cs",
            RevitTemplates.RevitHostService);
    }

    // ── Embedded server ───────────────────────────────────────────────────
    private void WriteEmbeddedServer()
    {
        Write($"{Config.ProjectName}/Server/PluginServer.cs",
            RevitTemplates.PluginServer);
        Write($"{Config.ProjectName}/Server/ExternalEventBridge.cs",
            RevitTemplates.ExternalEventBridge);
        Write($"{Config.ProjectName}/Server/Endpoints/ModelEndpoints.cs",
            RevitTemplates.ModelEndpoints);
    }

    // ── .sln ──────────────────────────────────────────────────────────────
    private void WriteSolution()
    {
        var configs = SolutionHelper.RevitBuildConfigs(Config.Versions);
        SolutionHelper.WriteSln(
            Path.Combine(Root, $"{Config.ProjectName}.sln"),
            Config.ProjectName,
            $"{Config.ProjectName}\\{Config.ProjectName}.csproj",
            configs);
    }
}
