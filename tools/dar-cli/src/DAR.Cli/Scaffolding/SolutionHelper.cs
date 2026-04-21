using DAR.Cli.Models;

namespace DAR.Cli.Scaffolding;

/// <summary>
/// Generates a valid .sln file with the correct build configuration entries.
/// </summary>
public static class SolutionHelper
{
    public static void WriteSln(string slnPath, string projectName, string csprojRelPath, List<string> buildConfigs)
    {
        var slnGuid  = Guid.NewGuid().ToString().ToUpper();
        var projGuid = Guid.NewGuid().ToString().ToUpper();

        var cfgLines        = new List<string>();
        var projCfgLines    = new List<string>();

        foreach (var cfg in buildConfigs)
        {
            cfgLines.Add($"\t\t{cfg}|x64 = {cfg}|x64");
            projCfgLines.Add($"\t\t{{{projGuid}}}.{cfg}|x64.ActiveCfg = {cfg}|x64");
            projCfgLines.Add($"\t\t{{{projGuid}}}.{cfg}|x64.Build.0 = {cfg}|x64");
        }

        var cfgBlock     = string.Join("\n", cfgLines);
        var projCfgBlock = string.Join("\n", projCfgLines);

        var sln = string.Join("\n",
            "",
            "Microsoft Visual Studio Solution File, Format Version 12.00",
            "# Visual Studio Version 17",
            "VisualStudioVersion = 17.0.31903.59",
            $"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{projectName}\", \"{csprojRelPath}\", \"{{{projGuid}}}\"",
            "EndProject",
            "Global",
            "\tGlobalSection(SolutionConfigurationPlatforms) = preSolution",
            cfgBlock,
            "\tEndGlobalSection",
            "\tGlobalSection(ProjectConfigurationPlatforms) = postSolution",
            projCfgBlock,
            "\tEndGlobalSection",
            "EndGlobal"
        );

        var dir = Path.GetDirectoryName(slnPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(slnPath, sln.Replace("\\t", "\t"));
    }

    public static List<string> RevitBuildConfigs(List<string> versions)
        => versions.Select(v => $"RVT{v}").ToList();

    public static List<string> Civil3DBuildConfigs(List<string> versions)
        => versions.Select(v => $"C3D{v}").ToList();

    public static List<string> CsiBuildConfigs(HostApp host, List<string> versions)
    {
        var prefix = host switch
        {
            HostApp.CSiBridge => "CSiBridge",
            HostApp.SAP2000   => "SAP2000",
            HostApp.ETABS     => "ETABS",
            _ => "CSi"
        };
        return versions.Select(v => $"{prefix}_{v}").ToList();
    }

    public static List<string> DynamoBuildConfigs(List<string> versions)
        => versions.Select(v => $"C3D{v}").ToList();
}
