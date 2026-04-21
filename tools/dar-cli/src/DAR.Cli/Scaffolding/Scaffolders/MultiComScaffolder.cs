using DAR.Cli.Brand;
using DAR.Cli.Models;
using DAR.Cli.Templates.MultiCom;

namespace DAR.Cli.Scaffolding.Scaffolders;

public class MultiComScaffolder : ScaffolderBase
{
    public MultiComScaffolder(ProjectConfig config) : base(config) { }

    public override void Scaffold()
    {
        Directory.CreateDirectory(Root);

        // Use CSi Directory.Build.props if any CSi host is selected
        var hasCsi = Config.ComHosts.Any(h => h is not ComHost.Civil3D);
        WriteSolutionRoot(hasCsi ? SharedTemplates.DirectoryBuildPropsCsi : SharedTemplates.DirectoryBuildProps);
        WriteGitHubActions(new[] { "Debug", "Release" });

        WriteCommonFolder($"{Config.ProjectName}");
        if (!string.IsNullOrEmpty(BrandConfig.LogoFileName))
            WriteDarLogo($"{Config.ProjectName}");

        WriteConnections();
        WriteWindow();
        WriteApp();
        WriteProjectFile();
        WriteSolution();
    }

    private void WriteConnections()
    {
        Write($"{Config.ProjectName}/Connections/IHostConnection.cs",
            MultiComTemplates.IHostConnection);

        foreach (var host in Config.ComHosts)
        {
            var (template, filename) = host switch
            {
                ComHost.Civil3D   => (MultiComTemplates.Civil3DConnection,   "Civil3DConnection.cs"),
                ComHost.CSiBridge => (MultiComTemplates.CSiBridgeConnection, "CSiBridgeConnection.cs"),
                ComHost.SAP2000   => (MultiComTemplates.SAP2000Connection,   "SAP2000Connection.cs"),
                ComHost.ETABS     => (MultiComTemplates.ETABSConnection,     "ETABSConnection.cs"),
                _ => throw new NotSupportedException()
            };
            Write($"{Config.ProjectName}/Connections/{filename}", template);
        }
    }

    private void WriteWindow()
    {
        // Build CONNECTION_INITS lines — one `Connections.Add(new XxxConnection());` per host
        var inits = string.Join("\n            ",
            Config.ComHosts.Select(h =>
            {
                var cls = h switch
                {
                    ComHost.Civil3D   => "Civil3DConnection",
                    ComHost.CSiBridge => "CSiBridgeConnection",
                    ComHost.SAP2000   => "SAP2000Connection",
                    ComHost.ETABS     => "ETABSConnection",
                    _ => throw new NotSupportedException()
                };
                return $"Connections.Add(new {cls}());";
            }));

        Write($"{Config.ProjectName}/MainWindowViewModel.cs",
            MultiComTemplates.ViewModel,
            new() { ["CONNECTION_INITS"] = inits });

        Write($"{Config.ProjectName}/MainWindow.xaml",
            MultiComTemplates.WindowXaml,
            new() { ["TITLE_BAR_LOGO"] = TitleBarLogoXaml("") });
        Write($"{Config.ProjectName}/MainWindow.xaml.cs",
            MultiComTemplates.WindowCodeBehind);
    }

    private void WriteApp()
    {
        Write($"{Config.ProjectName}/App.xaml",    MultiComTemplates.AppXaml);
        Write($"{Config.ProjectName}/App.xaml.cs", MultiComTemplates.AppCodeBehind);
    }

    private void WriteProjectFile()
    {
        // Build DLL references for each CSi host (Civil3D uses dynamic — no DLL)
        var refs = Config.ComHosts
            .Where(h => h is not ComHost.Civil3D)
            .Select(h =>
            {
                var (dll, prop) = h switch
                {
                    ComHost.SAP2000   => ("SAP2000v1",  "$(SAP2000Dir26)"),
                    ComHost.ETABS     => ("ETABSv1",    "$(ETABSDir22)"),
                    ComHost.CSiBridge => ("CSiBridge1",  "$(CSiBridgeDir26)"),
                    _ => throw new NotSupportedException()
                };
                return $"""
                      <ItemGroup>
                        <Reference Include="{dll}">
                          <HintPath>{prop}\{dll}.dll</HintPath>
                          <Private>False</Private>
                        </Reference>
                      </ItemGroup>
                    """;
            })
            .ToList();

        Write($"{Config.ProjectName}/{Config.ProjectName}.csproj",
            MultiComTemplates.CsProj,
            new() { ["CSI_REFERENCES"] = string.Join("\n", refs) });
    }

    private void WriteSolution()
    {
        var slnPath     = Path.Combine(Root, $"{Config.ProjectName}.sln");
        var csprojRel   = $"{Config.ProjectName}\\{Config.ProjectName}.csproj";
        var buildConfigs = new List<string> { "Debug", "Release" };

        SolutionHelper.WriteSln(slnPath, Config.ProjectName, csprojRel, buildConfigs);
    }
}
