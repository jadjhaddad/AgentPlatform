namespace DAR.Cli.Models;

public class ProjectConfig
{
    public string ProjectName  { get; set; } = string.Empty;
    public string OutputPath   { get; set; } = string.Empty;
    public string Author       { get; set; } = "";
    public string VendorId     { get; set; } = "";
    public string Description  { get; set; } = string.Empty;
    public HostApp HostApp     { get; set; }
    public PluginType PluginType { get; set; }
    public List<string> Versions { get; set; } = new();

    /// <summary>For MultiCom projects — which COM hosts are included.</summary>
    public List<ComHost> ComHosts { get; set; } = new();
}

/// <summary>Available COM connection targets for MultiCom projects.</summary>
public enum ComHost
{
    Civil3D,
    SAP2000,
    ETABS,
    CSiBridge,
}

public enum HostApp
{
    Revit,
    Civil3D,
    CSiBridge,
    SAP2000,
    ETABS,
    DynamoZeroTouch,
    ComCivil3D,
    ComSAP2000,
    ComETABS,
    ComCSiBridge,
    MultiCom,
}

public enum PluginType
{
    // Autodesk
    RibbonModal,
    RibbonModeless,
    CommandOnly,
    EmbeddedServer,

    // CSi
    CsiStandard,
    CsiStandalone,

    // COM
    ComClient,

    // Dynamo
    ZeroTouchLibrary,
    ZeroTouchWithUI,

    // Multi-COM
    MultiCom,
}
