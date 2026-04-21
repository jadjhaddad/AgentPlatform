# dar-new CLI Tool — Implementation Plan

## What It Is

A `dotnet tool` installed globally via:
```bash
dotnet tool install -g DAR.Cli
```

Run with:
```bash
dar new
```

Launches an interactive Spectre.Console prompt that scaffolds a fully configured C# project for Autodesk (Revit, Civil 3D), CSi (CSiBridge, SAP2000, ETABS), or Dynamo development — enforcing DAR conventions by default.

---

## Project Types

### Autodesk — Revit & Civil 3D
Both ask:
- **Plugin type:**
  - Ribbon Tool — Modal (blocking WPF dialog, host freezes)
  - Ribbon Tool — Modeless (persistent WPF window, host stays live)
  - Command Only (no ribbon, bare IExternalCommand / [CommandMethod])
  - Embedded Server (plugin hosts local ASP.NET Core HTTP server on localhost, external apps talk to it)
- **Versions:** 2023 / 2024 / 2025 / 2026 (multi-select, generates named build configs)

### CSi — CSiBridge, SAP2000, ETABS
Each asks:
- **Plugin type:**
  - Standard (blocking, in-process WinForms)
  - Standalone (separate WPF process — Speckle pattern — shim launches exe, calls Finish(0) immediately)
- **Versions per product:**
  - CSiBridge: v24, v25, v26
  - SAP2000: v23, v24, v25, v26
  - ETABS: v21, v22

### COM Automation (standalone WPF exe, no plugin)
- Civil 3D COM Client → `Marshal.GetActiveObject("AutoCAD.Application.XX")`
- SAP2000 COM Client → `cHelper.GetObject("CSI.SAP2000.API.SapObject")`
- ETABS COM Client → `cHelper.GetObject("CSI.ETABS.API.ETABSObject")`
- CSiBridge COM Client → `cHelper.GetObject("CSI.CSiBridge.API.SapObject")`
- ⚠ Revit has no COM interface — use Plugin + Embedded Server instead

### Dynamo Zero-Touch
- Zero-Touch Library (static node classes only)
- Zero-Touch + UI (nodes + WPF dialog nodes, Create/Show pattern)
- Versions: 2023 / 2024 / 2025 / 2026 (maps to Dynamo NuGet versions)

---

## Template Details

### Revit / Civil 3D — Modal
```
MyPlugin/
├── MyPlugin.sln
├── MyPlugin/
│   ├── MyPlugin.csproj           ← multi-version build configs
│   ├── Application.cs            ← IExternalApplication / CivilApplication
│   ├── MyToolCommand.cs          ← IExternalCommand / [CommandMethod]
│   ├── UI/
│   │   ├── MyToolWindow.xaml     ← DAR dark theme
│   │   ├── MyToolWindow.xaml.cs
│   │   └── MyToolViewModel.cs    ← ViewModelBase
│   ├── Common/
│   │   ├── ViewModelBase.cs
│   │   ├── RelayCommand.cs
│   │   └── CommonStyles.xaml     ← DAR color palette + button styles
│   └── Resources/
│       └── DARblue.png
├── Directory.Build.props
├── .gitignore
└── MyPlugin.addin                ← Revit only
    PackageContents.xml           ← Civil 3D only
```

### Revit / Civil 3D — Modeless
Same as Modal plus:
```
│   ├── StaWindowLauncher.cs      ← new Thread(STA) + Dispatcher.Run()
│   ├── IHostService.cs           ← host interface (ExecuteAsync, GetDocument, etc.)
│   └── HostService.cs            ← Revit:    AsyncEventHandler.RaiseAsync()
│                                    Civil 3D: ExecuteInCommandContextAsync()
```

### Revit / Civil 3D — Embedded Server
Same as Modeless plus:
```
│   ├── PluginServer.cs           ← ASP.NET Core minimal API, localhost:PORT
│   ├── ExternalEventBridge.cs    ← routes HTTP requests → ExternalEvent/ExecuteInCommandContext
│   └── Endpoints/
│       └── ModelEndpoints.cs     ← example endpoints (GET /views, POST /elements etc.)
```
⚠ Warning: requires firewall/IT whitelisting for the port.

### CSi — Standard
```
MyPlugin/
├── MyPlugin.sln
├── MyPlugin/
│   ├── MyPlugin.csproj           ← CSiBridge/SAP/ETABS build configs
│   ├── cPlugin.cs                ← cPluginContract entry point
│   ├── MainForm.cs               ← WinForms (CSi native)
│   └── AssemblyResolve.cs        ← DLL conflict resolution
├── Directory.Build.props
└── .gitignore
```

### CSi — Standalone (Speckle pattern)
```
MyPlugin/
├── MyPlugin.sln
├── MyPlugin.Shim/                ← thin DLL loaded by CSi
│   └── cPlugin.cs                ← launches App.exe, calls Finish(0) immediately
├── MyPlugin.App/                 ← standalone WPF exe (full DAR theme)
│   ├── App.xaml
│   ├── MainWindow.xaml
│   ├── MainWindowViewModel.cs
│   └── CsiService.cs             ← cHelper.GetObject(progId) reconnect
├── MyPlugin.Core/                ← .NET Standard 2.0 shared logic
├── Directory.Build.props
└── .gitignore
```
⚠ Warning: shim DLL + standalone exe both require IT/security whitelisting.

### COM Client (Civil 3D / CSi)
```
MyClient/
├── MyClient.sln
├── MyClient/
│   ├── MyClient.csproj
│   ├── App.xaml                  ← DAR dark theme WPF app
│   ├── MainWindow.xaml
│   ├── MainWindowViewModel.cs
│   └── HostConnection.cs         ← GetActiveObject / cHelper.GetObject
├── Directory.Build.props
└── .gitignore
```
⚠ Warning: requires COM automation enabled on host + IT whitelisting.

### Dynamo — Zero-Touch Library
```
MyNodes/
├── MyNodes.sln
├── MyNodes/
│   ├── MyNodes.csproj            ← per-version NuGet + build configs
│   ├── MyNodes.cs                ← static node class(es)
│   └── pkg.json                  ← Dynamo package manifest
├── Directory.Build.props
└── .gitignore
```

### Dynamo — Zero-Touch + UI
```
MyNodes/
├── MyNodes.sln
├── MyNodes/                      ← node library (same as above)
└── MyNodes.UI/
    ├── MyNodes.UI.csproj
    ├── MyDialog.xaml             ← DAR dark theme
    ├── MyDialogViewModel.cs
    └── MyDialogNodes.cs          ← Create/Show node pattern
```

---

## Multi-Version Strategy

### Revit / Civil 3D
Named build configurations per version:
```xml
<!-- RVT2023, RVT2024 → net48 -->
<!-- RVT2025, RVT2026 → net8.0-windows -->
<PropertyGroup Condition="'$(Configuration)'=='RVT2023'">
  <TargetFramework>net48</TargetFramework>
  <DefineConstants>RVT2023</DefineConstants>
</PropertyGroup>
```
All Autodesk DLLs: `<Private>False</Private>`

### CSi
Named build configurations per product+version:
```
CSiBridge_v24, CSiBridge_v25, CSiBridge_v26
SAP2000_v23, SAP2000_v24, SAP2000_v25, SAP2000_v26
ETABS_v21, ETABS_v22
```
All CSi DLLs: `<Private>False</Private>`

### Dynamo
NuGet packages with `<ExcludeAssets>runtime</ExcludeAssets>`, version per build config:
```
C3D2023 → DynamoVisualProgramming.* 2.16.x, net48
C3D2024 → DynamoVisualProgramming.* 2.19.x, net48
C3D2025 → DynamoVisualProgramming.* 3.x,    net8.0-windows
C3D2026 → DynamoVisualProgramming.* 4.x,    net8.0-windows
```

---

## Baked Into Every Template

| Feature | All | Revit/C3D | CSi Standalone | Dynamo |
|---|---|---|---|---|
| DAR dark theme (CommonStyles.xaml, DARblue.png) | — | ✓ | ✓ | — |
| ViewModelBase + RelayCommand | — | ✓ | ✓ | — |
| Directory.Build.props (x64, nullable, LangVersion) | ✓ | ✓ | ✓ | ✓ |
| Private=False / ExcludeAssets on host DLLs | ✓ | ✓ | ✓ | ✓ |
| .gitignore | ✓ | ✓ | ✓ | ✓ |
| AssemblyResolve hook (net48) | — | ✓ | ✓ | — |
| AssemblyLoadContext isolation (net8, Revit 2026) | — | ✓ | — | — |
| AfterBuild bundle/deploy target | — | ✓ | — | ✓ |
| .addin manifest | — | Revit only | — | — |
| PackageContents.xml | — | C3D only | — | — |
| pkg.json | — | — | — | ✓ |

---

## Warnings Shown During Scaffolding

| Template | Warning |
|---|---|
| CSi Standalone | ⚠ Launches a separate .exe. Both the shim DLL and standalone exe require IT/security whitelisting. |
| COM Clients | ⚠ Standalone .exe connecting via COM. Requires COM automation enabled on host + may need IT whitelisting. |
| Embedded Server | ⚠ Opens a local HTTP port. May require firewall rules and IT whitelisting depending on your environment. |

---

## CLI Tech Stack

| Concern | Library |
|---|---|
| Interactive prompts / checkboxes / spinners | Spectre.Console |
| Subcommand parsing (`dar new`, `dar list`) | System.CommandLine |
| File templating | Simple string replacement (no external engine) |
| Delivery | `dotnet tool install -g DAR.Cli` |
| CLI target framework | net8.0 |

---

## Implementation Build Order

1. [ ] CLI skeleton — solution + project + `dar new` prompt flow (Spectre.Console)
2. [ ] Revit Ribbon Tool — Modal template
3. [ ] Revit Ribbon Tool — Modeless template
4. [ ] Civil 3D Plugin — Modal + Modeless (unified pattern, different host marshal)
5. [ ] CSi Plugin — Standard template
6. [ ] CSi Plugin — Standalone (Speckle pattern)
7. [ ] COM Clients — Civil 3D + CSi
8. [ ] Dynamo Zero-Touch Library
9. [ ] Dynamo Zero-Touch + UI
10. [ ] Embedded Server — Revit + Civil 3D
11. [ ] `dar list` command (list available templates)
12. [ ] Polish, error handling, help text
