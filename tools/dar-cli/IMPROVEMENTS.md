# dar-cli Improvements

## Legend
- 🔴 Bug / runtime crash
- 🟡 Missing feature / rough edge
- 🟢 Nice to have / polish
- ✅ Done

---

## All items complete ✅

| # | Description | Version |
|---|-------------|---------|
| 1  | **`dar upgrade`** — patches `pkg.json`, Dynamo NuGet versions, old AfterBuild paths | 1.0.9 |
| 2  | **Dynamo `pkg.json` `node_libraries`** — `WithUI` now lists both `Name.dll` and `Name.UI.dll` | 1.0.9 |
| 3  | **Civil 3D `PackageContents.xml`** — proper `Platform="Civil3D"`, `SeriesMin/Max`, `<ComponentEntry>`, `<Commands>` | 1.0.7 |
| 4  | **Revit `.addin` re-generated every build** — removed `DeployAddin` target; `.addin` written once at scaffold time to both project folder and `Resources/` | 1.0.9 |
| 5  | **Uninstalled versions still offered** — version prompts show only detected installs; fallback with warning if nothing found | 1.0.8 |
| 6  | *(merged into #1)* | — |
| 7  | **`dar deploy`** — builds and deploys via Windows MSBuild (triggers AfterBuild targets) | 1.0.9 |
| 8  | **`dar version`** — shows all detected products with installed version numbers | 1.0.8 |
| 9  | **Version detection** — `InstalledVersions` class probes known paths; WSL-aware | 1.0.8 |
| 10 | **`--output` / `-o` flag on `dar new`** — override output directory | 1.0.9 |
| 11 | **Civil 3D `CommandHandler` missing** — confirmed already in templates, not a bug | — |
| 12 | **EmbeddedServer + pre-2025 Revit** — version prompt filtered to 2025+ with warning | 1.0.7 |
| 13 | **Error handling in `OnRun()`** — try/catch with `TaskDialog.Show` (Revit), `MessageBox.Show` (Civil 3D / COM / CSi) | 1.0.9 |
| 14 | **`StaWindowLauncher` dead code in Revit** — removed from Revit modeless; Revit now uses proper `ExternalEvent` + `IHostService` modeless pattern with `StaWindowLauncher` inlined in the command | 1.0.9 |
| 15 | **Single-version projects not tested** — added `Revit_SingleVersion`, `Civil3D_SingleVersion`, `SAP2000_SingleVersion` test cases | 1.0.9 |
| 16 | **Runtime reflection test** — test runner loads built DLL and verifies entry point class; suppressed on Linux where host DLLs unavailable | 1.0.9 |

---

## Test coverage

| Metric | Value |
|--------|-------|
| Template variations | 23 |
| All passing | ✅ |
| Single-version tested | ✅ |
| Runtime reflection check | ✅ (Windows only) |

---

## Commands

```
dar new [name] [-o <dir>]   scaffold a new project
dar list                     list available templates
dar version                  show detected installed products
dar deploy <sln> <config>    build + deploy (runs AfterBuild targets via Windows MSBuild)
dar upgrade [dir] [--dry-run] patch existing project to latest conventions
```
