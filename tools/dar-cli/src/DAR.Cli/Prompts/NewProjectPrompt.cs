using DAR.Cli.Brand;
using DAR.Cli.Detection;
using DAR.Cli.Models;
using Spectre.Console;

namespace DAR.Cli.Prompts;

public static class NewProjectPrompt
{
    // ASCII art — block chars from ansi-art(1).html, background removed image, transparent stripped.
    // Moved to BrandConfig.ArtRows for white-labelling. This is the DAR default.
private static readonly (string text, string color)[][] Art =
    [
        [("                                        ", "default")],
        [("                                        ", "default")],
        [("                ", "default"), (" ", "#005fff on #87afff"), (" ", "#87afff on #005fff"), (" ", "#005fff on #0087ff"), ("  ", "#005fd7 on #0087ff"), (" ", "#005fd7 on #5f87ff"), (" ", "#005fd7 on #5fafff"), (" ", "#005fff on #5fafff"), ("                ", "default")],
        [("               ", "default"), (" ", "#5f87ff on #5f5fff"), ("  ", "#87afff on #005fff"), (" ", "#005fff on #005fff"), ("  ", "#005fd7 on #0087ff"), (" ", "#5fafff on #0087ff"), ("  ", "#005fd7 on #0087ff"), ("  ", "#005fd7 on #5fafff"), ("              ", "default")],
        [("              ", "default"), (" ", "#87afff on #87afff"), ("  ", "#5f87ff on #005fff"), (" ", "#87afff on #005fff"), (" ", "#5f87ff on #005fff"), (" ", "#005fd7 on #0087ff"), ("   ", "#0087ff on #5fafff"), (" ", "#5fafff on #5f87ff"), (" ", "#0087ff on #5fafff"), (" ", "#5fafff on #5fafff"), (" ", "#005fd7 on #87afff"), ("░", "#5f87ff on #afafff"), ("            ", "default")],
        [("              ", "default"), (" ", "#005fd7 on #afd7ff"), (" ", "#5f87ff on #005fff"), (" ", "#005fd7 on #0087ff"), ("░", "#87afff on #005fd7"), ("▓▓▓", "#000000 on #000000"), ("▒", "#d75f00 on #303030"), ("▒", "#d7875f on #d7af87"), ("░", "#ff5f00 on #ffafaf"), ("▒", "#d75f00 on #d7af87"), ("▒", "#d7875f on #d7af87"), ("▒", "#d7875f on #121212"), ("             ", "default")],
        [("             ", "default"), ("░", "#5fd7d7 on #6c6c6c"), (" ", "#00005f on #121212"), ("▓", "#5f5f87 on #9e9e9e"), (" ", "#ffd787 on #ffd7af"), (" ", "#875f00 on #ffffd7"), (" ", "#ff8700 on #ffd7af"), ("▓", "#af5f00 on #af875f"), ("▓", "#af5f00 on #875f5f"), ("▓", "#af5f00 on #af875f"), ("▒▒", "#af875f on #d7af87"), ("▓", "#ff8700 on #af875f"), ("▒", "#ff8700 on #d7af87"), ("░", "#d75f00 on #ffd7af"), ("░", "#5fd7d7 on #5f5f5f"), ("            ", "default")],
        [("               ", "default"), ("▒", "#af5f00 on #d7af87"), ("▓", "#d75f00 on #af875f"), ("▓", "#af5f00 on #af875f"), ("▓", "#d75f00 on #87875f"), ("▓", "#af875f on #875f5f"), ("▓", "#af875f on #585858"), ("▓", "#d75f00 on #4e4e4e"), ("▒", "#af5f00 on #d7af87"), ("░", "#ffd7af on #ffd7af"), ("▒", "#d7af87 on #d7af87"), ("▒", "#af875f on #d7af87"), ("▒", "#af5f00 on #d7af87"), ("▓", "#af5f00 on #af875f"), ("            ", "default")],
        [("               ", "default"), ("▓", "#af5f00 on #4e4e4e"), ("▓", "#af875f on #585858"), ("▓", "#d75f00 on #585858"), ("▓", "#d75f00 on #4e4e4e"), ("▓", "#af875f on #444444"), ("▓", "#ff8700 on #444444"), ("▓", "#af875f on #4e4e4e"), ("▓", "#ff8700 on #875f5f"), ("▓", "#af875f on #af875f"), ("▓", "#d75f00 on #af875f"), ("▒", "#ff8700 on #3a3a3a"), ("▓", "#af875f on #875f5f"), ("░", "#00d75f on #878787"), ("            ", "default")],
        [("                ", "default"), ("▓", "#af875f on #4e4e4e"), ("▓", "#d75f00 on #4e4e4e"), ("▓", "#af5f00 on #3a3a3a"), ("▒▒", "#d78700 on #262626"), ("▓", "#ff8700 on #303030"), ("▓", "#af875f on #444444"), ("▓", "#ff8700 on #87875f"), ("▓", "#af5f00 on #afaf87"), ("▓", "#af5f00 on #875f5f"), ("▓", "#ff5f00 on #87875f"), ("             ", "default")],
        [("                 ", "default"), ("▓", "#ff8700 on #875f5f"), ("▓", "#af5f00 on #875f5f"), ("▒", "#af875f on #444444"), ("▓", "#af875f on #3a3a3a"), ("▒", "#af875f on #262626"), (" ", "#5f0000 on #000000"), ("▓", "#000000 on #000000"), ("░", "#d78700 on #121212"), ("▓", "#000000 on #000000"), ("              ", "default")],
        [("                ", "default"), ("▒", "#005fd7 on #3a3a3a"), ("▓", "#d75f00 on #875f5f"), ("▓", "#af875f on #87875f"), ("▓", "#ff8700 on #875f5f"), ("▓", "#af875f on #875f5f"), ("▓", "#af875f on #4e4e4e"), ("▓", "#d7875f on #3a3a3a"), ("▓", "#d75f00 on #875f5f"), ("▒", "#d75f5f on #121212"), ("               ", "default")],
        [("             ", "default"), ("▓", "#5f5f87 on #6c6c6c"), ("▓", "#5f5f87 on #767676"), ("▓", "#875fd7 on #6c6c6c"), ("▓", "#d70087 on #5f5f5f"), ("▓", "#af875f on #444444"), ("▓", "#af5f00 on #87875f"), ("▓", "#d75f00 on #af875f"), ("▓", "#af875f on #af875f"), ("▒", "#af5f00 on #d7af87"), ("▒", "#d7af87 on #d7af87"), ("▓", "#af875f on #87875f"), (" ", "#5f87ff on #0000af"), ("▓", "#875fff on #808080"), ("▓", "#5f5f87 on #767676"), ("▓", "#5f87d7 on #626262"), ("▓", "#005fff on #5f5f5f"), ("▒", "#87d7d7 on #4e4e4e"), ("          ", "default")],
        [("         ", "default"), ("▓", "#878787 on #878787"), ("▓", "#5f5f87 on #767676"), ("▓", "#5f5f87 on #626262"), ("▓", "#5f87ff on #6c6c6c"), ("▓", "#5f87ff on #4e4e4e"), ("▓", "#5f87d7 on #767676"), ("▓", "#5f5f87 on #6c6c6c"), ("▓", "#5f87d7 on #626262"), ("▓", "#005fd7 on #5f5f87"), ("  ", "#0087d7 on #5fd7ff"), (" ", "#0087d7 on #5fafff"), ("▒", "#5f87ff on #5f5f87"), ("▒", "#5f87ff on #8787d7"), (" ", "#87d7ff on #87d7ff"), (" ", "#00afff on #87d7ff"), (" ", "#00005f on #121212"), ("▓", "#8700d7 on #767676"), ("▓", "#5f8787 on #6c6c6c"), ("▓", "#005fff on #6c6c6c"), ("▓", "#0087d7 on #6c6c6c"), ("▓", "#005fff on #626262"), ("▓", "#5fd787 on #444444"), ("        ", "default")],
        [("        ", "default"), ("▓", "#d75faf on #5f5f5f"), ("▓", "#5f5f87 on #4e4e4e"), ("▓", "#5f87ff on #585858"), ("▓", "#5f5f87 on #585858"), ("▓", "#5f5f87 on #5f5f87"), ("▓", "#5f5f87 on #626262"), ("▓", "#5f5f87 on #303030"), ("▓", "#5f87ff on #6c6c6c"), ("▓", "#5f87ff on #626262"), ("▓", "#875fff on #5f5f5f"), (" ", "#5fafff on #87afff"), (" ", "#00afff on #87d7ff"), (" ", "#005f87 on #87d7ff"), ("▒", "#5f87ff on #5f87af"), ("▒", "#5f87d7 on #8787d7"), (" ", "#0087ff on #87d7ff"), (" ", "#87d7ff on #87d7ff"), (" ", "#00afff on #87d7ff"), ("▓", "#5f87d7 on #6c6c6c"), ("▓", "#5f87ff on #6c6c6c"), ("▓", "#5f5f87 on #6c6c6c"), ("▓", "#5f87ff on #585858"), ("▓", "#5f5f87 on #767676"), ("▓", "#5f5f87 on #3a3a3a"), ("        ", "default")],
        [("       ", "default"), ("░", "#5fd7d7 on #767676"), ("▓", "#303030 on #303030"), ("▓", "#3a3a3a on #3a3a3a"), ("▓", "#5f5f87 on #5f5f5f"), ("▓", "#5f5f87 on #444444"), ("▓", "#5f87ff on #444444"), ("▓", "#5f87ff on #4e4e4e"), ("▓", "#5f5f87 on #5f5f5f"), ("▓", "#5f5faf on #121212"), ("▓", "#5f87ff on #626262"), ("▓", "#5f87af on #585858"), ("▓", "#0087d7 on #6c6c6c"), (" ", "#005faf on #5fafff"), (" ", "#0087d7 on #87d7ff"), (" ", "#005fd7 on #afd7ff"), ("░", "#5f87ff on #afafff"), ("▓", "#5f87d7 on #5f5faf"), (" ", "#00afff on #87d7ff"), (" ", "#0087d7 on #5fd7ff"), (" ", "#00005f on #00005f"), ("▓", "#005fff on #808080"), ("▓", "#5f87ff on #626262"), ("▓", "#5f5f87 on #5f5f5f"), ("▓", "#5f5f87 on #6c6c6c"), ("▓", "#303030 on #303030"), ("        ", "default")],
        [("       ", "default"), ("▓", "#3a3a3a on #3a3a3a"), ("▓▓", "#1c1c1c on #1c1c1c"), ("▓", "#3a3a3a on #3a3a3a"), ("▓", "#5f5f87 on #262626"), ("▓", "#005fff on #444444"), ("▓", "#5f87d7 on #3a3a3a"), ("▓", "#5f87ff on #444444"), ("▓", "#5f87ff on #4e4e4e"), ("▓", "#5f87ff on #444444"), ("▓", "#5f87ff on #626262"), ("▓", "#875faf on #4e4e4e"), ("░", "#005fff on #5f87d7"), (" ", "#00afff on #87d7ff"), (" ", "#000080 on #00005f"), ("░", "#5f87ff on #87afff"), ("▒", "#5f87d7 on #5f87af"), ("░", "#5f87d7 on #005faf"), (" ", "#87d7ff on #87d7ff"), (" ", "#87afff on #000087"), ("▓", "#5f87af on #626262"), ("▓", "#5faf87 on #5f5f5f"), ("▓", "#5f5f87 on #4e4e4e"), ("▓", "#626262 on #5f5f5f"), ("▓", "#303030 on #303030"), ("▒", "#87d7d7 on #9e9e9e"), ("       ", "default")],
        [("      ", "default"), ("▓", "#080808 on #080808"), ("▓", "#3a3a3a on #303030"), ("▓", "#3a3a3a on #3a3a3a"), ("▓", "#303030 on #303030"), ("▓", "#005fff on #303030"), ("▓", "#000000 on #000000"), ("▓▓", "#303030 on #303030"), ("▓", "#5f5f87 on #303030"), ("▓", "#5f87d7 on #3a3a3a"), ("▓", "#5f87ff on #3a3a3a"), ("▓", "#5f87ff on #444444"), ("▓", "#5f87ff on #585858"), ("▓", "#875fff on #4e4e4e"), (" ", "#0087d7 on #5fafff"), (" ", "#005fd7 on #5f87ff"), ("▒", "#5f87ff on #5f87d7"), ("▒", "#005fff on #5f87af"), ("▒", "#5f87ff on #5f5f87"), (" ", "#005fd7 on #5fafff"), ("░", "#005fff on #005fff"), ("▒", "#5f87ff on #1c1c1c"), ("▓▓▓", "#ffffff on #ffffff"), ("▓", "#5fff00 on #1c1c1c"), ("▓", "#808080 on #808080"), ("       ", "default")],
        [("     ", "default"), ("▓", "#3a3a3a on #3a3a3a"), ("▓", "#303030 on #303030"), ("▓", "#444444 on #444444"), ("▓", "#000000 on #000000"), ("▓", "#5f5f87 on #262626"), ("▓", "#005fff on #262626"), ("▓▓", "#000000 on #000000"), ("▓", "#262626 on #1c1c1c"), ("▓", "#1c1c1c on #1c1c1c"), ("▓", "#5f5f87 on #303030"), ("▓", "#5f5f87 on #1c1c1c"), ("▓", "#005fff on #303030"), ("▓", "#005fff on #444444"), ("▓", "#00d787 on #444444"), ("▒", "#005fd7 on #5f5f87"), ("░", "#005fd7 on #005fd7"), ("▒", "#5f87ff on #5f5faf"), ("▒", "#005fd7 on #5f5f87"), ("▓▓▓▓▓▓▓▓", "#ffffff on #ffffff"), ("▓", "#303030 on #303030"), ("       ", "default")],
        [("    ", "default"), ("░", "#5fd7d7 on #c6c6c6"), ("▓", "#000000 on #000000"), ("▓", "#1c1c1c on #1c1c1c"), ("▓", "#5f5f87 on #3a3a3a"), ("▓", "#005fff on #121212"), ("▓", "#005fff on #1c1c1c"), ("▓", "#5f5f87 on #080808"), (" ", "default"), ("▓▓", "#000000 on #000000"), ("▓", "#121212 on #121212"), ("▓", "#5f5f87 on #1c1c1c"), ("▓", "#5f87ff on #1c1c1c"), ("▓", "#5f87ff on #121212"), ("▓", "#000000 on #000000"), ("▓", "#005faf on #303030"), ("▓", "#ff5f87 on #af8787"), ("▓▓▓▓▓▓▓", "#ffffff on #ffffff"), ("▓", "#875f87 on #dadada"), ("▓", "#3a3a3a on #3a3a3a"), ("▓", "#585858 on #585858"), ("▓", "#1c1c1c on #1c1c1c"), ("▓", "#3a3a3a on #3a3a3a"), ("       ", "default")],
        [("   ", "default"), ("░", "#5fd7d7 on #808080"), ("▓", "#303030 on #303030"), ("▓", "#3a3a3a on #3a3a3a"), ("▓", "#262626 on #262626"), ("▓▓▓", "#5f5f87 on #1c1c1c"), ("  ", "default"), ("▓▓▓", "#000000 on #000000"), (" ", "#00005f on #000000"), ("▓", "#0087ff on #121212"), ("▓▓", "#ffffff on #ffffff"), (" ", "#ff875f on #ffaf87"), (" ", "#ff5f87 on #eeeeee"), ("▓▓▓▓", "#ffffff on #ffffff"), ("▓", "#5f87ff on #3a3a3a"), (" ", "#005fd7 on #0087ff"), (" ", "#00005f on #080808"), ("▓", "#000000 on #000000"), ("▓", "#875f87 on #262626"), ("▓", "#626262 on #585858"), ("▓", "#5fd7af on #3a3a3a"), ("▓", "#5faf87 on #3a3a3a"), ("▓", "#121212 on #121212"), ("      ", "default")],
        [("   ", "default"), ("▓", "#000000 on #000000"), ("▓", "#080808 on #080808"), ("▓▓", "#3a3a3a on #3a3a3a"), ("▓", "#87875f on #444444"), ("▓", "#5f87ff on #444444"), ("▓", "#5f87ff on #303030"), ("▓", "#eeeeee on #eeeeee"), ("▓", "#8a8a8a on #8a8a8a"), ("▓", "#000000 on #000000"), ("▓", "#5f87ff on #262626"), ("▓▓▓", "#ffffff on #ffffff"), ("▒", "#ff875f on #d78787"), (" ", "#ffaf87 on #ffaf87"), (" ", "#ff5f00 on #ffd7af"), (" ", "#ff875f on #ffd7d7"), (" ", "#ffafaf on #ffd7af"), (" ", "#ff875f on #ffd7af"), (" ", "#ff875f on #ff875f"), ("▓", "#875fff on #121212"), ("▒", "#5f87ff on #121212"), (" ", "#000080 on #000080"), (" ", "#00005f on #080808"), ("▓▓", "#000000 on #000000"), ("▓", "#5f87ff on #121212"), ("▒", "#5f87d7 on #080808"), ("▓", "#5f87ff on #1c1c1c"), ("▓", "#000000 on #000000"), ("      ", "default")],
        [("   ", "default"), ("▓", "#af8787 on #262626"), ("▓", "#af5f87 on #080808"), ("▓", "#af5f87 on #1c1c1c"), ("▓", "#1c1c1c on #121212"), ("▓", "#121212 on #121212"), ("▓", "#1c1c1c on #1c1c1c"), ("▓", "#262626 on #1c1c1c"), ("▓", "#262626 on #262626"), ("▓", "#444444 on #444444"), ("▓", "#303030 on #303030"), ("▓", "#3a3a3a on #3a3a3a"), ("▓", "#303030 on #303030"), ("▓", "#87875f on #303030"), ("▓", "#d7875f on #875f5f"), (" ", "#ffaf87 on #ffaf5f"), (" ", "#d75f00 on #ffaf87"), (" ", "#ffaf87 on #ffaf5f"), ("░", "#ffaf87 on #ff875f"), ("░", "#ff875f on #d75f5f"), (" ", "#00005f on #080808"), ("▒▒", "#5f87ff on #262626"), ("▒", "#5f87ff on #1c1c1c"), (" ", "#005fff on #000080"), (" ", "#5f87ff on #000080"), ("▓▓", "#000000 on #000000"), ("▓", "#5f5f87 on #121212"), ("▒", "#5f87ff on #121212"), ("▓", "#5f5f87 on #121212"), ("▓", "#262626 on #262626"), ("      ", "default")],
        [("       ", "default"), ("▓", "#d75f87 on #808080"), ("▓", "#d7af00 on #303030"), (" ", "#5f0000 on #000000"), ("▓", "#080808 on #080808"), ("▓", "#000000 on #000000"), ("▓", "#87af87 on #080808"), ("▓▓", "#000000 on #000000"), ("▒", "#d75faf on #000000"), ("░", "#ff875f on #5f0000"), ("▒", "#d7875f on #af5f5f"), (" ", "#ff875f on #5f0000"), ("▓▓▓", "#000000 on #000000"), (" ", "#5f87ff on #00005f"), ("▒▒", "#5f87ff on #3a3a3a"), ("▒", "#5f87ff on #444444"), ("░", "#5f87ff on #00005f"), (" ", "#00005f on #080808"), ("▓▓▓", "#000000 on #000000"), ("▒", "#5f87d7 on #080808"), ("▓", "#5f87ff on #303030"), ("▓", "#303030 on #303030"), ("      ", "default")],
        [("      ", "default"), ("▓▓▓▓▓", "#ffffff on #ffffff"), ("▓", "#a8a8a8 on #9e9e9e"), ("▓", "#000000 on #000000"), ("▓", "#5f5f87 on #121212"), ("▓", "#000000 on #000000"), ("▓", "#080808 on #080808"), (" ", "#87ffaf on #000000"), ("▓▓▓▓▓", "#000000 on #000000"), (" ", "#00005f on #00005f"), ("▒", "#5f87ff on #4e4e4e"), ("▒", "#5f87d7 on #4e4e4e"), ("▒", "#5f87ff on #444444"), ("▒", "#5f87ff on #303030"), ("░", "#005fd7 on #005fd7"), ("▓▓▓▓", "#000000 on #000000"), ("▓", "#080808 on #080808"), ("▓", "#000000 on #000000"), ("      ", "default")],
        [("       ", "default"), ("▓", "#ffffff on #ffffff"), ("   ", "default"), (" ", "#ff5f87 on #080808"), ("▓▓▓▓▓▓▓", "#000000 on #000000"), ("▓", "#080808 on #000000"), ("▓▓", "#000000 on #000000"), ("░", "#005fd7 on #005fff"), ("▒", "#5f87ff on #5f5f87"), ("▒▒", "#5f87ff on #3a3a3a"), ("▒", "#5f87ff on #303030"), ("░", "#005fff on #005fd7"), ("▓▓▓▓", "#000000 on #000000"), ("▓", "#121212 on #121212"), ("▓", "#000000 on #000000"), ("      ", "default")],
        [("                                        ", "default")],
        [("                                        ", "default")],
        [("          E  n  g  i  n  i  r           ", "bold white")],
        [("                                        ", "default")],
    ];

    private static void PrintArt()
    {
        // Full ANSI art — DAR default. Forks: replace Art[] above or use BrandConfig.BannerArt fallback.
        foreach (var row in Art)
        {
            foreach (var (text, color) in row)
            {
                if (color == "default")
                    AnsiConsole.Write(text);
                else
                    AnsiConsole.Markup($"[{color}]{Markup.Escape(text)}[/]");
            }
            AnsiConsole.WriteLine();
        }
    }

    public static ProjectConfig Run(string? projectName = null, string? outputPath = null)
    {
        PrintArt();
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]{BrandConfig.BannerSubtitle}[/]\n");

        var config = new ProjectConfig();

        // ── Project name ─────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(projectName))
        {
            config.ProjectName = projectName.Trim();
            AnsiConsole.MarkupLine($"[teal]Project name:[/] {config.ProjectName}");
        }
        else
        {
            config.ProjectName = AnsiConsole.Ask<string>("[teal]Project name:[/]").Trim();
        }

        // ── Output path ───────────────────────────────────────────────────
        config.OutputPath = !string.IsNullOrWhiteSpace(outputPath)
            ? Path.GetFullPath(outputPath)
            : Path.Combine(Directory.GetCurrentDirectory(), config.ProjectName);

        // ── Author / vendor ───────────────────────────────────────────────
        string author;
        if (!string.IsNullOrWhiteSpace(BrandConfig.DefaultAuthor))
        {
            author = AnsiConsole.Ask<string>(
                $"[teal]Author / vendor[/] [grey](default: {BrandConfig.DefaultAuthor}):[/]",
                BrandConfig.DefaultAuthor).Trim();
        }
        else
        {
            author = AnsiConsole.Ask<string>("[teal]Author / vendor:[/]").Trim();
        }
        config.Author   = string.IsNullOrWhiteSpace(author) ? BrandConfig.DefaultAuthor : author;
        config.VendorId = string.IsNullOrWhiteSpace(author) ? BrandConfig.DefaultVendorId : author;

        // ── Description ───────────────────────────────────────────────────
        var desc = AnsiConsole.Ask<string>("[teal]Description[/] [grey](one line, default: project name):[/]", config.ProjectName).Trim();
        config.Description = string.IsNullOrWhiteSpace(desc) ? config.ProjectName : desc;

        // ── Host application ─────────────────────────────────────────────
        var hostChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[teal]Select host application:[/]")
                .AddChoices(
                    "Revit",
                    "Civil 3D",
                    "CSiBridge",
                    "SAP2000",
                    "ETABS",
                    "Dynamo Zero-Touch",
                    "─────────────────────",
                    "COM Client — Civil 3D",
                    "COM Client — SAP2000",
                    "COM Client — ETABS",
                    "COM Client — CSiBridge",
                    "Multi-COM Client"
                ));

        config.HostApp = hostChoice switch
        {
            "Revit"                  => HostApp.Revit,
            "Civil 3D"               => HostApp.Civil3D,
            "CSiBridge"              => HostApp.CSiBridge,
            "SAP2000"                => HostApp.SAP2000,
            "ETABS"                  => HostApp.ETABS,
            "Dynamo Zero-Touch"      => HostApp.DynamoZeroTouch,
            "COM Client — Civil 3D"  => HostApp.ComCivil3D,
            "COM Client — SAP2000"   => HostApp.ComSAP2000,
            "COM Client — ETABS"     => HostApp.ComETABS,
            "COM Client — CSiBridge" => HostApp.ComCSiBridge,
            "Multi-COM Client"       => HostApp.MultiCom,
            _ => throw new InvalidOperationException("Invalid host selection")
        };

        // ── Route to sub-prompts ──────────────────────────────────────────
        switch (config.HostApp)
        {
            case HostApp.Revit:
            case HostApp.Civil3D:
                PromptAutodesk(config);
                break;

            case HostApp.CSiBridge:
            case HostApp.SAP2000:
            case HostApp.ETABS:
                PromptCsi(config);
                break;

            case HostApp.DynamoZeroTouch:
                PromptDynamo(config);
                break;

            case HostApp.ComCivil3D:
            case HostApp.ComSAP2000:
            case HostApp.ComETABS:
            case HostApp.ComCSiBridge:
                PromptComClient(config);
                break;

            case HostApp.MultiCom:
                PromptMultiCom(config);
                break;
        }

        return config;
    }

    // ── Autodesk (Revit / Civil 3D) ───────────────────────────────────────
    private static void PromptAutodesk(ProjectConfig config)
    {
        var typeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[teal]Plugin type:[/]")
                .AddChoices(
                    "Ribbon Tool — Modal       (blocking dialog, host freezes)",
                    "Ribbon Tool — Modeless    (persistent window, host stays live)",
                    "Command Only              (no ribbon, bare command)",
                    "Embedded Server           (plugin hosts local HTTP server)"
                ));

        config.PluginType = typeChoice switch
        {
            var s when s.StartsWith("Ribbon Tool — Modal")     => PluginType.RibbonModal,
            var s when s.StartsWith("Ribbon Tool — Modeless")  => PluginType.RibbonModeless,
            var s when s.StartsWith("Command Only")            => PluginType.CommandOnly,
            var s when s.StartsWith("Embedded Server")         => PluginType.EmbeddedServer,
            _ => throw new InvalidOperationException("Invalid plugin type")
        };

        if (config.PluginType == PluginType.EmbeddedServer)
            ShowWarning("[yellow]⚠[/]  This project type opens a local HTTP port. It may require firewall rules and IT/security whitelisting depending on your environment.");

        // EmbeddedServer requires net8.0 — only Revit 2025+ supports .NET 8
        var isEmbeddedRevit = config.HostApp == HostApp.Revit && config.PluginType == PluginType.EmbeddedServer;
        if (isEmbeddedRevit)
            ShowWarning("[yellow]⚠[/]  EmbeddedServer requires .NET 8 — only Revit 2025+ is supported.");

        // Use detected installed versions; fall back to full list if nothing found
        var detected = config.HostApp == HostApp.Revit
            ? InstalledVersions.Revit
            : InstalledVersions.Civil3D;

        var allVersions = (detected.Count > 0 ? detected : (IReadOnlyList<string>)["2023", "2024", "2025", "2026"])
            .Where(v => !isEmbeddedRevit || int.Parse(v) >= 2025)
            .ToArray();

        if (detected.Count == 0)
            ShowWarning("[yellow]⚠[/]  No installed versions detected — showing all. Install paths may not resolve.");

        var prompt = new MultiSelectionPrompt<string>()
            .Title("[teal]Select versions to support:[/]")
            .InstructionsText("[grey](space to toggle, enter to confirm)[/]")
            .AddChoices(allVersions);

        // Pre-select all detected versions by default
        foreach (var v in allVersions)
            prompt.Select(v);

        config.Versions = AnsiConsole.Prompt(prompt);
    }

    // ── CSi (CSiBridge / SAP2000 / ETABS) ────────────────────────────────
    private static void PromptCsi(ProjectConfig config)
    {
        var typeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[teal]Plugin type:[/]")
                .AddChoices(
                    "Standard    (blocking, in-process — host freezes while plugin runs)",
                    "Standalone  (separate WPF process — host stays live)"
                ));

        config.PluginType = typeChoice.StartsWith("Standard")
            ? PluginType.CsiStandard
            : PluginType.CsiStandalone;

        if (config.PluginType == PluginType.CsiStandalone)
            ShowWarning("[yellow]⚠[/]  This project type launches a separate .exe. Both the plugin shim DLL and the standalone executable require IT/security whitelisting.");

        var fallback = config.HostApp switch
        {
            HostApp.CSiBridge => new[] { "v24", "v25", "v26" },
            HostApp.SAP2000   => new[] { "v23", "v24", "v25", "v26" },
            HostApp.ETABS     => new[] { "v21", "v22" },
            _ => Array.Empty<string>()
        };

        var detected = config.HostApp switch
        {
            HostApp.CSiBridge => InstalledVersions.CSiBridge,
            HostApp.SAP2000   => InstalledVersions.SAP2000,
            HostApp.ETABS     => InstalledVersions.ETABS,
            _ => (IReadOnlyList<string>)Array.Empty<string>()
        };

        var versionChoices = detected.Count > 0 ? detected : fallback;

        if (detected.Count == 0)
            ShowWarning("[yellow]⚠[/]  No installed versions detected — showing all. Install paths may not resolve.");

        var csiPrompt = new MultiSelectionPrompt<string>()
            .Title("[teal]Select versions to support:[/]")
            .InstructionsText("[grey](space to toggle, enter to confirm)[/]")
            .AddChoices(versionChoices);

        foreach (var v in versionChoices)
            csiPrompt.Select(v);

        config.Versions = AnsiConsole.Prompt(csiPrompt);
    }

    // ── Dynamo Zero-Touch ─────────────────────────────────────────────────
    private static void PromptDynamo(ProjectConfig config)
    {
        var typeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[teal]Library type:[/]")
                .AddChoices(
                    "Zero-Touch Library    (static node classes only)",
                    "Zero-Touch + UI       (nodes + WPF dialog nodes)"
                ));

        config.PluginType = typeChoice.StartsWith("Zero-Touch Library")
            ? PluginType.ZeroTouchLibrary
            : PluginType.ZeroTouchWithUI;

        var detectedDynamo = InstalledVersions.DynamoC3D.Count > 0
            ? InstalledVersions.DynamoC3D
            : (IReadOnlyList<string>)["2023", "2024", "2025", "2026"];

        if (InstalledVersions.DynamoC3D.Count == 0)
            ShowWarning("[yellow]⚠[/]  No Dynamo for Civil 3D installations detected — showing all. Install paths may not resolve.");

        var dynaPrompt = new MultiSelectionPrompt<string>()
            .Title("[teal]Select Civil 3D versions to target:[/]")
            .InstructionsText("[grey](space to toggle, enter to confirm)[/]")
            .AddChoices(detectedDynamo);

        foreach (var v in detectedDynamo)
            dynaPrompt.Select(v);

        config.Versions = AnsiConsole.Prompt(dynaPrompt);
    }

    // ── COM Client ────────────────────────────────────────────────────────
    private static void PromptComClient(ProjectConfig config)
    {
        config.PluginType = PluginType.ComClient;
        ShowWarning("[yellow]⚠[/]  This project type is a standalone .exe that connects to a running application via COM automation. It requires COM automation to be enabled on the host and may need IT/security whitelisting.");
    }

    // ── Multi-COM Client ─────────────────────────────────────────────────
    private static void PromptMultiCom(ProjectConfig config)
    {
        config.PluginType = PluginType.MultiCom;

        ShowWarning("[yellow]⚠[/]  Multi-COM Client — standalone .exe that connects to multiple running applications via COM. Requires COM automation enabled on each host.");

        var choices = new List<string> { "Civil 3D", "SAP2000", "ETABS", "CSiBridge" };

        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[teal]Select COM connections to include:[/]")
                .InstructionsText("[grey](space to toggle, enter to confirm — at least 2)[/]")
                .AddChoices(choices)
                .Select("Civil 3D")
                .Select("CSiBridge"));

        if (selected.Count < 2)
        {
            ShowWarning("[yellow]⚠[/]  Multi-COM requires at least 2 connections. Use 'COM Client' for a single host.");
            selected = choices.Take(2).ToList();
        }

        config.ComHosts = selected.Select(s => s switch
        {
            "Civil 3D"  => ComHost.Civil3D,
            "SAP2000"   => ComHost.SAP2000,
            "ETABS"     => ComHost.ETABS,
            "CSiBridge"  => ComHost.CSiBridge,
            _ => throw new InvalidOperationException()
        }).ToList();
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private static void ShowWarning(string message)
    {
        AnsiConsole.Write(new Panel(new Markup(message))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Yellow),
            Padding = new Padding(1, 0),
        });
        AnsiConsole.WriteLine();
    }
}
