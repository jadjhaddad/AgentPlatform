using DAR.Cli.Brand;
using DAR.Cli.Models;

namespace DAR.Cli.Scaffolding;

public abstract class ScaffolderBase
{
    protected ProjectConfig Config { get; }

    protected string Root => Config.OutputPath;
    protected string ProjectRoot => Path.Combine(Root, Config.ProjectName);
    protected string Ns => Config.ProjectName;

    protected ScaffolderBase(ProjectConfig config)
    {
        Config = config;
    }

    public abstract void Scaffold();

    protected Dictionary<string, string> BaseTokens() => new()
    {
        ["PROJECT_NAME"]  = Config.ProjectName,
        ["NAMESPACE"]     = Ns,
        ["YEAR"]          = DateTime.Now.Year.ToString(),
        ["AUTHOR"]        = Config.Author,
        ["VENDOR_ID"]     = Config.VendorId,
        ["DESCRIPTION"]   = string.IsNullOrWhiteSpace(Config.Description)
                             ? Config.ProjectName
                             : Config.Description,
        ["LOGO_FILE"]     = BrandConfig.LogoFileName,
        ["LOGO_FOLDER"]   = BrandConfig.LogoResourceSubfolder,
        ["PRIMARY_COLOR"] = BrandConfig.PrimaryColor,
        ["DANGER_COLOR"]  = BrandConfig.DangerColor,
        // Title bar logo — default uses ../ prefix for windows in UI/ subfolder.
        // Override TITLE_BAR_LOGO in extra tokens for projects where the window is at root.
        ["TITLE_BAR_LOGO"] = TitleBarLogoXaml("../"),
    };

    /// <summary>Returns an XAML &lt;Image&gt; element for the title bar logo, or empty string if no logo.</summary>
    protected static string TitleBarLogoXaml(string pathPrefix = "../")
        => string.IsNullOrEmpty(BrandConfig.LogoFileName)
            ? string.Empty
            : $"""<Image DockPanel.Dock="Left" Source="{pathPrefix}{BrandConfig.LogoResourceSubfolder}/{BrandConfig.LogoFileName}" Height="24" Margin="10,0"/>""";

    protected void Write(string relativePath, string template, Dictionary<string, string>? extra = null)
    {
        var tokens = BaseTokens();
        if (extra != null)
            foreach (var (k, v) in extra)
                tokens[k] = v;

        TemplateEngine.WriteFile(
            Path.Combine(Root, relativePath),
            template,
            tokens);
    }

    protected void Copy(string relativePath, string content)
    {
        TemplateEngine.CopyFile(Path.Combine(Root, relativePath), content);
    }

    /// <summary>
    /// Copy the brand logo (embedded in this assembly) into the generated project's Resources/ folder.
    /// Gracefully skips if BrandConfig.LogoFileName is null or the asset is not found.
    /// </summary>
    protected void WriteDarLogo(string projectSubPath)
    {
        if (string.IsNullOrEmpty(BrandConfig.LogoFileName)) return;

        var destPath = Path.Combine(Root, projectSubPath,
            BrandConfig.LogoResourceSubfolder, BrandConfig.LogoFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

        var assembly = typeof(ScaffolderBase).Assembly;
        var resName  = assembly.GetManifestResourceNames()
                           .FirstOrDefault(n => n.EndsWith(BrandConfig.LogoFileName));
        if (resName is null) return;

        using var stream = assembly.GetManifestResourceStream(resName)!;
        using var file   = File.Create(destPath);
        stream.CopyTo(file);
    }

    /// <summary>
    /// Write the shared DAR Common/ folder (ViewModelBase, RelayCommand, CommonStyles.xaml).
    /// </summary>
    protected void WriteCommonFolder(string projectSubPath = "")
    {
        var prefix = string.IsNullOrEmpty(projectSubPath)
            ? $"{Config.ProjectName}/Common"
            : $"{projectSubPath}/Common";

        Write($"{prefix}/ViewModelBase.cs",    SharedTemplates.ViewModelBase);
        Write($"{prefix}/RelayCommand.cs",     SharedTemplates.RelayCommand);
        Write($"{prefix}/CommonStyles.xaml",   SharedTemplates.CommonStylesXaml);
    }

    /// <summary>
    /// Write Directory.Build.props and .gitignore at solution root.
    /// </summary>
    protected void WriteSolutionRoot(string? directoryBuildPropsOverride = null)
    {
        Write("Directory.Build.props", directoryBuildPropsOverride ?? SharedTemplates.DirectoryBuildProps);
        Copy(".gitignore",    SharedTemplates.GitIgnore);
        Copy(".editorconfig", SharedTemplates.EditorConfig);
    }

    /// <summary>
    /// Write .github/workflows/build.yml with a build matrix for the given configurations.
    /// </summary>
    protected void WriteGitHubActions(IEnumerable<string> buildConfigs)
    {
        var matrix = string.Join(", ", buildConfigs);
        Write(".github/workflows/build.yml",
            SharedTemplates.GitHubActionsWorkflow,
            new() { ["BUILD_MATRIX"] = matrix });
    }
}
