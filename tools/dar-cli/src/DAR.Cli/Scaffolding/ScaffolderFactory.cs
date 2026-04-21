using DAR.Cli.Models;
using DAR.Cli.Scaffolding.Scaffolders;

namespace DAR.Cli.Scaffolding;

public static class ScaffolderFactory
{
    public static ScaffolderBase Create(ProjectConfig config) => config.HostApp switch
    {
        HostApp.Revit                                                  => new RevitScaffolder(config),
        HostApp.Civil3D                                                => new Civil3DScaffolder(config),
        HostApp.CSiBridge or HostApp.SAP2000 or HostApp.ETABS         => new CsiScaffolder(config),
        HostApp.ComCivil3D or HostApp.ComSAP2000
            or HostApp.ComETABS or HostApp.ComCSiBridge                => new ComClientScaffolder(config),
        HostApp.MultiCom                                               => new MultiComScaffolder(config),
        HostApp.DynamoZeroTouch                                        => new DynamoScaffolder(config),
        _ => throw new NotSupportedException($"No scaffolder for {config.HostApp}")
    };
}
