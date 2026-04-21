namespace DAR.Cli.Detection;

/// <summary>
/// Detects which Autodesk / CSi products are installed by probing known filesystem paths.
/// Works on both Windows and WSL (uses /mnt/c/ prefix on Linux).
/// Results are cached for the lifetime of the process.
/// </summary>
public static class InstalledVersions
{
    private static readonly string ProgramFiles = OperatingSystem.IsLinux()
        ? "/mnt/c/Program Files"
        : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    // ── Revit ─────────────────────────────────────────────────────────────
    public static IReadOnlyList<string> Revit { get; } = Detect(
        ("2023", $"{ProgramFiles}/Autodesk/Revit 2023/RevitAPI.dll"),
        ("2024", $"{ProgramFiles}/Autodesk/Revit 2024/RevitAPI.dll"),
        ("2025", $"{ProgramFiles}/Autodesk/Revit 2025/RevitAPI.dll"),
        ("2026", $"{ProgramFiles}/Autodesk/Revit 2026/RevitAPI.dll")
    );

    // ── Civil 3D (via AutoCAD + C3D subfolder) ────────────────────────────
    public static IReadOnlyList<string> Civil3D { get; } = Detect(
        ("2023", $"{ProgramFiles}/Autodesk/AutoCAD 2023/C3D/AeccDbMgd.dll"),
        ("2024", $"{ProgramFiles}/Autodesk/AutoCAD 2024/C3D/AeccDbMgd.dll"),
        ("2025", $"{ProgramFiles}/Autodesk/AutoCAD 2025/C3D/AeccDbMgd.dll"),
        ("2026", $"{ProgramFiles}/Autodesk/AutoCAD 2026/C3D/AeccDbMgd.dll")
    );

    // ── CSiBridge ─────────────────────────────────────────────────────────
    public static IReadOnlyList<string> CSiBridge { get; } = Detect(
        ("v24", $"{ProgramFiles}/Computers and Structures/CSiBridge 24/CSiBridge1.dll"),
        ("v25", $"{ProgramFiles}/Computers and Structures/CSiBridge 25/CSiBridge1.dll"),
        ("v26", $"{ProgramFiles}/Computers and Structures/CSiBridge 26/CSiBridge1.dll")
    );

    // ── SAP2000 ───────────────────────────────────────────────────────────
    public static IReadOnlyList<string> SAP2000 { get; } = Detect(
        ("v23", $"{ProgramFiles}/Computers and Structures/SAP2000 23/SAP2000v1.dll"),
        ("v24", $"{ProgramFiles}/Computers and Structures/SAP2000 24/SAP2000v1.dll"),
        ("v25", $"{ProgramFiles}/Computers and Structures/SAP2000 25/SAP2000v1.dll"),
        ("v26", $"{ProgramFiles}/Computers and Structures/SAP2000 26/SAP2000v1.dll")
    );

    // ── ETABS ─────────────────────────────────────────────────────────────
    public static IReadOnlyList<string> ETABS { get; } = Detect(
        ("v21", $"{ProgramFiles}/Computers and Structures/ETABS 21/ETABSv1.dll"),
        ("v22", $"{ProgramFiles}/Computers and Structures/ETABS 22/ETABSv1.dll")
    );

    // ── Dynamo (via Civil 3D Dynamo AppData folders) ──────────────────────
    // Returns the Civil 3D years that have Dynamo installed.
    public static IReadOnlyList<string> DynamoC3D { get; } = DetectDynamo();

    // ─────────────────────────────────────────────────────────────────────

    private static List<string> Detect(params (string version, string probe)[] candidates)
        => candidates
            .Where(c => File.Exists(c.probe))
            .Select(c => c.version)
            .ToList();

    private static List<string> DetectDynamo()
    {
        // Dynamo for Civil 3D lives at:
        //   %AppData%\Autodesk\C3D {year}\Dynamo\
        // We detect it by checking that folder exists for each C3D year.
        var years = new[] { "2023", "2024", "2025", "2026" };

        if (!OperatingSystem.IsLinux())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return years
                .Where(y => Directory.Exists($"{appData}/Autodesk/C3D {y}/Dynamo"))
                .ToList();
        }

        // On WSL: scan all user profiles under /mnt/c/Users/ looking for C3D Dynamo installs.
        // This handles any Windows username without guessing.
        var usersDir = "/mnt/c/Users";
        if (!Directory.Exists(usersDir)) return [];

        var found = new HashSet<string>();
        foreach (var userDir in Directory.GetDirectories(usersDir))
        {
            foreach (var year in years)
            {
                if (Directory.Exists($"{userDir}/AppData/Roaming/Autodesk/C3D {year}/Dynamo"))
                    found.Add(year);
            }
        }

        return years.Where(found.Contains).ToList();
    }

    /// <summary>
    /// Returns a display summary of detected products — used by `dar version`.
    /// </summary>
    public static string Summary()
    {
        static string fmt(string name, IReadOnlyList<string> versions)
            => versions.Count == 0
                ? $"  {name,-14} [grey](not detected)[/]"
                : $"  {name,-14} [teal]{string.Join(", ", versions)}[/]";

        return string.Join("\n", new[]
        {
            fmt("Revit",     Revit),
            fmt("Civil 3D",  Civil3D),
            fmt("CSiBridge", CSiBridge),
            fmt("SAP2000",   SAP2000),
            fmt("ETABS",     ETABS),
            fmt("Dynamo C3D",DynamoC3D),
        });
    }
}
