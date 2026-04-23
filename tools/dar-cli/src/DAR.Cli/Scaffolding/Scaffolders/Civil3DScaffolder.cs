using DAR.Cli.Brand;
using DAR.Cli.Models;
using DAR.Cli.Templates.Civil3D;

namespace DAR.Cli.Scaffolding.Scaffolders;

public class Civil3DScaffolder : ScaffolderBase
{
    public Civil3DScaffolder(ProjectConfig config) : base(config) { }

    public override void Scaffold()
    {
        Directory.CreateDirectory(Root);
        WriteSolutionRoot(SharedTemplates.DirectoryBuildPropsCivil3D);

        var hasUi = Config.PluginType != PluginType.CommandOnly;
        if (hasUi)
        {
        WriteCommonFolder($"{Config.ProjectName}");
        WriteDarLogo($"{Config.ProjectName}");
        }

        WriteProjectFile();
        WritePackageContents();
        WriteLogger();
        WriteApplication();
        WriteCommand();

        if (hasUi)
        {
            WriteWindow();
            WriteViewModel();
        }

        if (Config.PluginType is PluginType.RibbonModeless or PluginType.EmbeddedServer)
        {
            Write($"{Config.ProjectName}/StaWindowLauncher.cs",       SharedTemplates.StaWindowLauncher);
            Write($"{Config.ProjectName}/Host/IHostService.cs",       Civil3DTemplates.IHostService);
            Write($"{Config.ProjectName}/Host/Civil3DHostService.cs", Civil3DTemplates.Civil3DHostService);
        }

        if (Config.PluginType == PluginType.EmbeddedServer)
            WriteEmbeddedServer();

        WriteGitHubActions(Config.Versions.Select(v => $"C3D{v}"));
        WriteSolution();
    }

    private void WriteEmbeddedServer()
    {
        Write($"{Config.ProjectName}/Server/PluginServer.cs",
            Civil3DTemplates.PluginServer);
        Write($"{Config.ProjectName}/Server/ExternalEventBridge.cs",
            Civil3DTemplates.ExternalEventBridge);
        Write($"{Config.ProjectName}/Server/Endpoints/ModelEndpoints.cs",
            Civil3DTemplates.ModelEndpoints);
    }

    private void WriteProjectFile()
    {
        var configs = Config.Versions.Select(v => Civil3DTemplates.BuildConfigBlock(v)).ToList();

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

        var aspNetRef = Config.PluginType == PluginType.EmbeddedServer
            ? """
                <ItemGroup Condition="$(TargetFramework.StartsWith('net8')) Or $(TargetFramework.StartsWith('net9')) Or $(TargetFramework.StartsWith('net1'))">
                  <FrameworkReference Include="Microsoft.AspNetCore.App"/>
                </ItemGroup>
              """
            : string.Empty;

        Write($"{Config.ProjectName}/{Config.ProjectName}.csproj",
            Civil3DTemplates.CsProj,
            new()
            {
                ["BUILD_CONFIGS"]  = string.Join("\n\n", configs),
                ["RESOURCE_ITEMS"] = resourceItems,
                ["ASPNET_REF"]     = aspNetRef,
            });
    }

    private void WritePackageContents()
    {
        var commandName  = Config.ProjectName.ToUpper();
        var description  = string.IsNullOrWhiteSpace(Config.Description)
            ? Config.ProjectName
            : Config.Description;
        var components   = Civil3DTemplates.PackageContentsComponents(
            Config.ProjectName, Config.Versions, commandName, description);

        Write($"{Config.ProjectName}/PackageContents.xml",
            Civil3DTemplates.PackageContents,
            new()
            {
                ["COMPONENTS"]    = components,
                ["PRODUCT_GUID"]  = Guid.NewGuid().ToString().ToUpper(),
            });
    }

    private void WriteApplication()
    {
        var template = Config.PluginType == PluginType.CommandOnly
            ? Civil3DTemplates.ApplicationEmpty
            : Civil3DTemplates.ApplicationRibbon;
        Write($"{Config.ProjectName}/Application.cs", template);
    }

    private void WriteCommand()
    {
        var template = Config.PluginType switch
        {
            PluginType.CommandOnly    => Civil3DTemplates.CommandOnly,
            PluginType.RibbonModeless => Civil3DTemplates.CommandModeless,
            _                         => Civil3DTemplates.Command,
        };
        Write($"{Config.ProjectName}/Commands/{Config.ProjectName}Commands.cs",
            template,
            new() { ["PROJECT_NAME_UPPER"] = Config.ProjectName.ToUpper() });
    }

    private void WriteWindow()
    {
        var codeBehind = Config.PluginType == PluginType.RibbonModeless
            ? Civil3DTemplates.WindowCodeBehindModeless
            : Civil3DTemplates.WindowCodeBehind;
        Write($"{Config.ProjectName}/UI/{Config.ProjectName}Window.xaml",    Civil3DTemplates.WindowXaml);
        Write($"{Config.ProjectName}/UI/{Config.ProjectName}Window.xaml.cs", codeBehind);
    }

    private void WriteViewModel()
    {
        var vmTemplate = Config.PluginType == PluginType.RibbonModeless
            ? Civil3DTemplates.ViewModelModeless
            : Civil3DTemplates.ViewModel;
        Write($"{Config.ProjectName}/UI/{Config.ProjectName}ViewModel.cs", vmTemplate);
    }

    private void WriteSolution()
    {
        var configs = SolutionHelper.Civil3DBuildConfigs(Config.Versions);
        SolutionHelper.WriteSln(
            Path.Combine(Root, $"{Config.ProjectName}.sln"),
            Config.ProjectName,
            $"{Config.ProjectName}\\{Config.ProjectName}.csproj",
            configs);
    }
}
