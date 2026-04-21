namespace DAR.Cli.Scaffolding;

/// <summary>
/// Simple token-replacement template engine.
/// Tokens in template files are written as {{TOKEN_NAME}}.
/// </summary>
public static class TemplateEngine
{
    public static string Render(string template, Dictionary<string, string> tokens)
    {
        foreach (var (key, value) in tokens)
            template = template.Replace($"{{{{{key}}}}}", value);
        return template;
    }

    public static void WriteFile(string outputPath, string template, Dictionary<string, string> tokens)
    {
        var dir = Path.GetDirectoryName(outputPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(outputPath, Render(template, tokens));
    }

    public static void CopyFile(string outputPath, string content)
    {
        var dir = Path.GetDirectoryName(outputPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(outputPath, content);
    }
}
