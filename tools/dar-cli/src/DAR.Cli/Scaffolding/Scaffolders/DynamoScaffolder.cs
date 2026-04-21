using DAR.Cli.Models;
using DAR.Cli.Templates.Dynamo;

namespace DAR.Cli.Scaffolding.Scaffolders;

public class DynamoScaffolder : ScaffolderBase
{
    public DynamoScaffolder(ProjectConfig config) : base(config) { }

    public override void Scaffold()
    {
        Directory.CreateDirectory(Root);
        WriteSolutionRoot();

        WriteNodeLibrary();

        if (Config.PluginType == PluginType.ZeroTouchWithUI)
            WriteUILibrary();

        WriteGitHubActions(Config.Versions.Select(v => $"C3D{v}"));
        WriteSolution();
    }

    private void WriteNodeLibrary()
    {
        var configs = Config.Versions.Select(v => DynamoTemplates.BuildConfigBlock(v)).ToList();

        Write($"{Config.ProjectName}/{Config.ProjectName}.csproj",
            DynamoTemplates.CsProj,
            new() { ["BUILD_CONFIGS"] = string.Join("\n\n", configs) });

        Write($"{Config.ProjectName}/{Config.ProjectName}Nodes.cs", DynamoTemplates.NodeLibrary);

        var hasUi = Config.PluginType == PluginType.ZeroTouchWithUI;
        Write($"{Config.ProjectName}/pkg.json", DynamoTemplates.PkgJson,
            new() { ["NODE_LIBRARIES"] = DynamoTemplates.NodeLibraries(Config.ProjectName, hasUi) });
        // No Common/ folder needed — Dynamo nodes are plain static classes, no MVVM
    }

    private void WriteUILibrary()
    {
        var configs = Config.Versions.Select(v => DynamoTemplates.BuildConfigBlock(v)).ToList();

        Write($"{Config.ProjectName}.UI/{Config.ProjectName}.UI.csproj",
            DynamoTemplates.UICsProj,
            new() { ["BUILD_CONFIGS"] = string.Join("\n\n", configs) });

        Write($"{Config.ProjectName}.UI/{Config.ProjectName}DialogNodes.cs", DynamoTemplates.DialogNodes);
        Write($"{Config.ProjectName}.UI/{Config.ProjectName}Dialog.xaml",    DynamoTemplates.DialogXaml);
        Write($"{Config.ProjectName}.UI/{Config.ProjectName}Dialog.xaml.cs", DynamoTemplates.DialogCodeBehind);
        // CommonStyles.xaml in the UI project for the dialog theme
        WriteCommonFolder($"{Config.ProjectName}.UI");
    }

    private void WriteSolution()
    {
        var configs  = SolutionHelper.DynamoBuildConfigs(Config.Versions);
        var projects = new List<(string name, string relPath)>
        {
            (Config.ProjectName, $"{Config.ProjectName}\\{Config.ProjectName}.csproj")
        };

        if (Config.PluginType == PluginType.ZeroTouchWithUI)
            projects.Add(($"{Config.ProjectName}.UI", $"{Config.ProjectName}.UI\\{Config.ProjectName}.UI.csproj"));

        // Write multi-project sln
        var projGuid = projects.Select(_ => Guid.NewGuid().ToString().ToUpper()).ToList();
        var cfgLines = configs.Select(c => $"\t\t{c}|x64 = {c}|x64").ToList();
        var projLines = new List<string>();
        var projCfgLines = new List<string>();

        const string projTypeGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        for (int i = 0; i < projects.Count; i++)
        {
            var (name, path) = projects[i];
            projLines.Add($"Project(\"{projTypeGuid}\") = \"{name}\", \"{path}\", \"{{{projGuid[i]}}}\"");
            projLines.Add("EndProject");

            foreach (var cfg in configs)
            {
                projCfgLines.Add($"\t\t{{{projGuid[i]}}}.{cfg}|x64.ActiveCfg = {cfg}|x64");
                projCfgLines.Add($"\t\t{{{projGuid[i]}}}.{cfg}|x64.Build.0 = {cfg}|x64");
            }
        }

        var sln = string.Join("\n",
            "",
            "Microsoft Visual Studio Solution File, Format Version 12.00",
            "# Visual Studio Version 17",
            string.Join("\n", projLines),
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
