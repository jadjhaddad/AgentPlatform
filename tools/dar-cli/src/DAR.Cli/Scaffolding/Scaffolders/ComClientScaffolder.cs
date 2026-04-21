using DAR.Cli.Models;
using DAR.Cli.Templates.Com;

namespace DAR.Cli.Scaffolding.Scaffolders;

public class ComClientScaffolder : ScaffolderBase
{
    public ComClientScaffolder(ProjectConfig config) : base(config) { }

    public override void Scaffold()
    {
        Directory.CreateDirectory(Root);
        // Use CSi Directory.Build.props if this is a CSi COM client (provides $(SAP2000DirXX) etc.)
        var dbProps = Config.HostApp is HostApp.ComSAP2000 or HostApp.ComETABS or HostApp.ComCSiBridge
            ? SharedTemplates.DirectoryBuildPropsCsi
            : SharedTemplates.DirectoryBuildProps;
        WriteSolutionRoot(dbProps);
        WriteGitHubActions(new[] { "Debug", "Release" });
        WriteCommonFolder($"{Config.ProjectName}");
        WriteDarLogo($"{Config.ProjectName}");

        var hostType    = ComTemplates.HostTypeName(Config.HostApp);
        var activeCall  = ComTemplates.GetActiveObjectCall(Config.HostApp);

        // CSi COM clients need a using for the CSi namespace (SAP2000v1, ETABSv1, CSiBridge1)
        var csiUsingLine = Config.HostApp switch
        {
            HostApp.ComSAP2000   => "using SAP2000v1;",
            HostApp.ComETABS     => "using ETABSv1;",
            HostApp.ComCSiBridge => "using CSiBridge1;",
            _                    => string.Empty,   // Civil3D uses dynamic, no using needed
        };

        var extra = new Dictionary<string, string>
        {
            ["HOST_TYPE_NAME"]         = hostType,
            ["GET_ACTIVE_OBJECT_CALL"] = activeCall,
            ["CSI_USING_LINE"]         = csiUsingLine,
        };

        var comRefs = BuildComReferences();
        Write($"{Config.ProjectName}/{Config.ProjectName}.csproj",
            ComTemplates.CsProj,
            new() { ["COM_REFERENCES"] = comRefs });

        Write($"{Config.ProjectName}/App.xaml",                 ComTemplates.AppXaml);
        Write($"{Config.ProjectName}/App.xaml.cs",              ComTemplates.AppCodeBehind);
        Write($"{Config.ProjectName}/MainWindow.xaml",          ComTemplates.MainWindowXaml,
            new() { ["TITLE_BAR_LOGO"] = TitleBarLogoXaml("") });
        Write($"{Config.ProjectName}/MainWindow.xaml.cs",       ComTemplates.MainWindowCodeBehind);
        Write($"{Config.ProjectName}/HostConnection.cs",        ComTemplates.HostConnection, extra);
        Write($"{Config.ProjectName}/MainWindowViewModel.cs",   ComTemplates.MainWindowViewModel, extra);

        WriteSolution();
    }

    private string BuildComReferences()
    {
        // Civil3D: connect via Marshal.GetActiveObject — no COM reference needed (uses dynamic)
        if (Config.HostApp == HostApp.ComCivil3D)
            return string.Empty;

        // CSi products — reference the installed DLL directly (no COMReference — not supported by dotnet MSBuild)
        // Use MSBuild properties from Directory.Build.props for WSL compat
        var (dll, installProp) = Config.HostApp switch
        {
            HostApp.ComSAP2000   => ("SAP2000v1",  "$(SAP2000Dir26)"),
            HostApp.ComETABS     => ("ETABSv1",    "$(ETABSDir22)"),
            HostApp.ComCSiBridge => ("CSiBridge1", "$(CSiBridgeDir26)"),
            _ => ("SAP2000v1",   "$(SAP2000Dir26)")
        };

        return $"""
              <ItemGroup>
                <Reference Include="{dll}">
                  <HintPath>{installProp}\{dll}.dll</HintPath>
                  <Private>True</Private>
                </Reference>
              </ItemGroup>
            """;
    }

    private void WriteSolution()
    {
        SolutionHelper.WriteSln(
            Path.Combine(Root, $"{Config.ProjectName}.sln"),
            Config.ProjectName,
            $"{Config.ProjectName}\\{Config.ProjectName}.csproj",
            new List<string> { "Debug", "Release" });
    }
}
