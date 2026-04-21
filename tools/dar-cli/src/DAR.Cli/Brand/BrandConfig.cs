namespace DAR.Cli.Brand;

/// <summary>
/// All brand-specific values for the scaffolding tool.
///
/// To white-label this tool for your organisation:
///   1. Edit the constants below.
///   2. Replace Assets/{LogoFileName} with your own logo (or update LogoFileName).
///   3. In {PackageId}.csproj change:
///        &lt;ToolCommandName&gt;  → your CLI command name  (e.g. "acme")
///        &lt;PackageId&gt;        → your NuGet package ID  (e.g. "Acme.Cli")
///        &lt;AssemblyName&gt;     → your assembly name     (e.g. "acme")
///   4. dotnet pack → dotnet tool install -g
/// </summary>
public static class BrandConfig
{
    // ── CLI identity ──────────────────────────────────────────────────────
    /// <summary>The CLI command name shown in help text (must match csproj ToolCommandName).</summary>
    public const string ToolName = "aec";

    /// <summary>NuGet package ID (must match csproj PackageId).</summary>
    public const string PackageId = "AEC.Cli";

    /// <summary>
    /// The C# project folder name inside the source tree (matches RootNamespace).
    /// Used by `dar brand` to locate files in the extracted source snapshot.
    /// Must match the folder name in src/ and the csproj RootNamespace.
    /// </summary>
    public const string SourceProjectFolder = "AEC.Cli";

    /// <summary>The C# root namespace for all internal tool code.</summary>
    public const string RootNamespace = "AEC.Cli";

    // ── Scaffold defaults ─────────────────────────────────────────────────
    /// <summary>Default author / company name pre-filled in the prompt.</summary>
    public const string DefaultAuthor = "";

    /// <summary>Default vendor ID written into Revit .addin files.</summary>
    public const string DefaultVendorId = "";

    // ── Brand colors (used in generated project CommonStyles.xaml) ───────
    /// <summary>Primary action color — teal buttons, highlights. ARGB hex e.g. #FF32DAC4</summary>
    public const string PrimaryColor = "#FF32DAC4";

    /// <summary>Danger / close button color. ARGB hex e.g. #FFCE4848</summary>
    public const string DangerColor = "#FFCE4848";

    // ── Logo asset ────────────────────────────────────────────────────────
    /// <summary>
    /// Filename of the logo PNG embedded in this assembly as a ManifestResource.
    /// Must match the filename in Assets/ and the csproj EmbeddedResource Include.
    /// Set to empty string to scaffold projects with no logo (ribbon button will have text only).
    /// </summary>
    public const string LogoFileName = "";

    /// <summary>Subfolder under the generated project where the logo is written.</summary>
    public const string LogoResourceSubfolder = "Resources";

    // ── Terminal banner ───────────────────────────────────────────────────
    /// <summary>One-line subtitle shown under the ASCII art banner.</summary>
    public const string BannerSubtitle = "AEC Project Scaffold";

    // NOTE for forks: the full ANSI art banner lives in
    // Prompts/NewProjectPrompt.cs — Art[]. Replace the Art array there
    // to use your own terminal artwork.
}
