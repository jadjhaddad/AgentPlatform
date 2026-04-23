namespace DAR.Cli.Templates.Civil3D;

public static class Civil3DTemplates
{
    // ── .csproj ───────────────────────────────────────────────────────────
    public const string CsProj = """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <RootNamespace>{{NAMESPACE}}</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}</AssemblyName>
            <TargetFramework>net48</TargetFramework>
          </PropertyGroup>

          <!-- ── Per-version build configurations ───────────────────────── -->
        {{BUILD_CONFIGS}}

          <PropertyGroup>
            <UseWPF>true</UseWPF>
          </PropertyGroup>

          <!-- ── Resources ──────────────────────────────────────────────── -->
          {{RESOURCE_ITEMS}}

          <!-- ── ASP.NET Core (EmbeddedServer only — net8.0+) ───────────── -->
          {{ASPNET_REF}}

          <!-- ── Logging (Serilog with rolling file sink) ──────────────── -->
          <ItemGroup>
            <PackageReference Include="Serilog" Version="3.1.1" />
            <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
          </ItemGroup>

          <!-- ── Deploy bundle to %AppData%\Autodesk\ApplicationPlugins ──── -->
          <!-- Skipped on Linux/WSL dotnet build — run via 'vs build' to deploy -->
          <Target Name="DeployBundle" AfterTargets="Build"
                  Condition="!$([MSBuild]::IsOSPlatform('Linux')) And '$(AppData)' != ''">
            <PropertyGroup>
              <BundleOut>$(OutputPath)$(AssemblyName).bundle</BundleOut>
              <BundleContents>$(BundleOut)\Contents</BundleContents>
              <BundleDeploy>$(AppData)\Autodesk\ApplicationPlugins\$(AssemblyName).bundle</BundleDeploy>
            </PropertyGroup>
            <ItemGroup>
              <_BundleDlls Include="$(OutputPath)*.dll" />
            </ItemGroup>
            <!-- Build portable bundle in output dir -->
            <MakeDir Directories="$(BundleContents)" />
            <Copy SourceFiles="$(ProjectDir)PackageContents.xml" DestinationFolder="$(BundleOut)" SkipUnchangedFiles="true" />
            <Copy SourceFiles="@(_BundleDlls)" DestinationFolder="$(BundleContents)" SkipUnchangedFiles="true" />
            <Message Text="[Civil 3D] Bundle → $(BundleOut)" Importance="high" />
            <!-- Deploy to AppData so Civil 3D auto-loads on next launch -->
            <MakeDir Directories="$(BundleDeploy)\Contents" />
            <Copy SourceFiles="$(ProjectDir)PackageContents.xml" DestinationFolder="$(BundleDeploy)" SkipUnchangedFiles="true" />
            <Copy SourceFiles="@(_BundleDlls)" DestinationFolder="$(BundleDeploy)\Contents" SkipUnchangedFiles="true" />
            <Message Text="[Civil 3D] Deployed → $(BundleDeploy)" Importance="high" />
          </Target>

        </Project>
        """;

    private static string AcadInstallPath(string version) =>
        $@"C:\Program Files\Autodesk\AutoCAD {version}";

    private static string CivilInstallPath(string version) =>
        $@"C:\Program Files\Autodesk\AutoCAD {version}\C3D";

    public static string BuildConfigBlock(string version)
    {
        var tf = int.Parse(version) >= 2025 ? "net8.0-windows" : "net48";
        var define = $"C3D{version}";
        var acadPath  = AcadInstallPath(version);
        var civilPath = CivilInstallPath(version);

        var wpfBuildService = tf == "net8.0-windows"
            ? "\n    <UseWpfBuildService>false</UseWpfBuildService>"
            : string.Empty;

        // Use $(AcadDir{version}) and $(CivilDir{version}) — defined in Directory.Build.props with WSL remapping
        return $"""
              <PropertyGroup Condition="'$(Configuration)'=='C3D{version}'">
                <TargetFramework>{tf}</TargetFramework>
                <DefineConstants>{define}</DefineConstants>
                <OutputPath>bin\C3D{version}\</OutputPath>{wpfBuildService}
              </PropertyGroup>

              <ItemGroup Condition="'$(Configuration)'=='C3D{version}'">
                <Reference Include="acdbmgd">
                  <HintPath>$(AcadDir{version})\acdbmgd.dll</HintPath>
                  <Private>False</Private>
                </Reference>
                <Reference Include="acmgd">
                  <HintPath>$(AcadDir{version})\acmgd.dll</HintPath>
                  <Private>False</Private>
                </Reference>
                <Reference Include="accoremgd">
                  <HintPath>$(AcadDir{version})\accoremgd.dll</HintPath>
                  <Private>False</Private>
                </Reference>
                <Reference Include="AcWindows">
                  <HintPath>$(AcadDir{version})\AcWindows.dll</HintPath>
                  <Private>False</Private>
                </Reference>
                <Reference Include="AdWindows">
                  <HintPath>$(AcadDir{version})\AdWindows.dll</HintPath>
                  <Private>False</Private>
                </Reference>
                <Reference Include="AeccDbMgd">
                  <HintPath>$(CivilDir{version})\AeccDbMgd.dll</HintPath>
                  <Private>False</Private>
                </Reference>
                <Reference Include="AeccUiMgd">
                  <HintPath>$(CivilDir{version})\AeccUiMgd.dll</HintPath>
                  <Private>False</Private>
                </Reference>
              </ItemGroup>
            """;
    }

    // ── PackageContents.xml ───────────────────────────────────────────────
    // Versions and command names are injected by the scaffolder via tokens.
    public const string PackageContents = """
        <?xml version="1.0" encoding="utf-8"?>
        <ApplicationPackage SchemaVersion="1.0"
                            AutodeskProduct="Civil3D"
                            ProductType="Application"
                            Name="{{PROJECT_NAME}}"
                            Description="{{DESCRIPTION}}"
                            AppVersion="1.0.0"
                            FriendlyVersion="1.0.0"
                            Author="{{AUTHOR}}"
                            ProductCode="{{{PRODUCT_GUID}}}">

          <CompanyDetails Name="{{AUTHOR}}"/>

        {{COMPONENTS}}

        </ApplicationPackage>
        """;

    /// <summary>Civil 3D year → AutoCAD series number (R24.0 = 2024, etc.)</summary>
    public static string YearToSeries(string year) => year switch
    {
        "2023" => "R23.0",
        "2024" => "R24.0",
        "2025" => "R25.0",
        "2026" => "R26.0",
        _      => "R24.0"
    };

    /// <summary>Build a per-version Components block for PackageContents.xml.</summary>
    public static string PackageContentsComponents(string projectName, IEnumerable<string> versions, string commandName, string description)
    {
        var series = versions.Select(YearToSeries).ToList();
        var min    = series.First();
        var max    = series.Last();

        return $"""
              <RuntimeRequirements OS="Win64" Platform="Civil3D"
                                   SeriesMin="{min}" SeriesMax="{max}"/>

              <Components Description="{description}">
                <RuntimeRequirements OS="Win64" Platform="Civil3D"
                                     SeriesMin="{min}" SeriesMax="{max}"/>
                <ComponentEntry AppName="{projectName}"
                                Version="1.0.0"
                                ModuleName="./Contents/{projectName}.dll"
                                AppDescription="{description}"
                                LoadOnCommandInvocation="True"
                                LoadOnAutoCADStartup="False">
                  <Commands GroupName="{projectName}">
                    <Command Global="{commandName}" Local="{commandName}"/>
                  </Commands>
                </ComponentEntry>
              </Components>
            """
            .TrimEnd();
    }

    // ── Application.cs (ribbon) ───────────────────────────────────────────
    public const string ApplicationRibbon = """
        using Autodesk.AutoCAD.Runtime;
        using Autodesk.AutoCAD.ApplicationServices;
        using Autodesk.Windows;
        using {{NAMESPACE}}.Commands;

        [assembly: ExtensionApplication(typeof({{NAMESPACE}}.Application))]

        namespace {{NAMESPACE}};

        public class Application : IExtensionApplication
        {
            public static Application? Instance { get; private set; }

            public void Initialize()
            {
                Instance = this;
                PluginLogger.Initialize("{{VENDOR_ID}}", "{{PROJECT_NAME}}");
                if (ComponentManager.Ribbon != null)
                    BuildRibbon();
                else
                    ComponentManager.ItemInitialized += OnRibbonReady;
            }

            public void Terminate() => PluginLogger.CloseAndFlush();

            private static void OnRibbonReady(object? sender, RibbonItemEventArgs e)
            {
                if (ComponentManager.Ribbon == null) return;
                ComponentManager.ItemInitialized -= OnRibbonReady;
                BuildRibbon();
            }

            private static void BuildRibbon()
            {
                var ribbon = ComponentManager.Ribbon;
                if (ribbon is null) return;

                var tab = new RibbonTab { Title = "{{PROJECT_NAME}}", Id = "{{PROJECT_NAME}}_TAB" };
                ribbon.Tabs.Add(tab);

                var panelSource = new RibbonPanelSource { Title = "{{PROJECT_NAME}}" };
                var panel = new RibbonPanel { Source = panelSource };
                tab.Panels.Add(panel);

                var btn = new RibbonButton
                {
                    Text           = "{{PROJECT_NAME}}",
                    CommandHandler = new {{PROJECT_NAME}}CommandHandler(),
                    Size           = RibbonItemSize.Large,
                    Orientation    = System.Windows.Controls.Orientation.Vertical,
                    ShowText       = true,
                    LargeImage     = LoadIcon("{{LOGO_FOLDER}}/{{LOGO_FILE}}"),
                    Image          = LoadIcon("{{LOGO_FOLDER}}/{{LOGO_FILE}}"),
                };
                panelSource.Items.Add(btn);
            }

            private static System.Windows.Media.ImageSource? LoadIcon(string relativePath)
            {
                try
                {
                    var dir  = System.IO.Path.GetDirectoryName(typeof(Application).Assembly.Location)!;
                    var path = System.IO.Path.Combine(dir, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    if (!System.IO.File.Exists(path)) return null;
                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.UriSource   = new Uri(path, UriKind.Absolute);
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
                catch { return null; }
            }
        }
        """;

    // ── Application.cs (command-only — no ribbon) ────────────────────────
    public const string ApplicationEmpty = """
        using Autodesk.AutoCAD.Runtime;

        [assembly: ExtensionApplication(typeof({{NAMESPACE}}.Application))]

        namespace {{NAMESPACE}};

        public class Application : IExtensionApplication
        {
            public static Application? Instance { get; private set; }
            public void Initialize()
            {
                Instance = this;
                PluginLogger.Initialize("{{VENDOR_ID}}", "{{PROJECT_NAME}}");
            }
            public void Terminate() => PluginLogger.CloseAndFlush();
        }
        """;

    // ── Command.cs (command-only — no window) ────────────────────────────
    public const string CommandOnly = """
        using Autodesk.AutoCAD.Runtime;
        using Autodesk.AutoCAD.ApplicationServices;
        using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

        [assembly: CommandClass(typeof({{NAMESPACE}}.Commands.{{PROJECT_NAME}}Commands))]

        namespace {{NAMESPACE}}.Commands;

        public class {{PROJECT_NAME}}Commands
        {
            [CommandMethod("{{PROJECT_NAME_UPPER}}", CommandFlags.Modal)]
            public void Run()
            {
                var doc = AcadApp.DocumentManager.MdiActiveDocument;
                if (doc is null) return;

                // TODO: implement command logic here
                doc.Editor.WriteMessage($"\n{{PROJECT_NAME}} executed.");
            }
        }
        """;

    // ── Command.cs (modal — opens WPF window) ────────────────────────────
    public const string Command = """
        using Autodesk.AutoCAD.Runtime;
        using Autodesk.AutoCAD.ApplicationServices;

        [assembly: CommandClass(typeof({{NAMESPACE}}.Commands.{{PROJECT_NAME}}Commands))]

        namespace {{NAMESPACE}}.Commands;

        public class {{PROJECT_NAME}}Commands
        {
        [CommandMethod("{{PROJECT_NAME_UPPER}}", CommandFlags.Modal)]
        public void RunModal()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc is null) return;

                var window = new UI.{{PROJECT_NAME}}Window();
                Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModalWindow(window);
            }
        }

        /// <summary>Ribbon button command handler.</summary>
        public class {{PROJECT_NAME}}CommandHandler : System.Windows.Input.ICommand
        {
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter)
            {
                var cmd = new {{PROJECT_NAME}}Commands();
                cmd.RunModal();
            }
        }
        """;

    // ── Modeless Command.cs ───────────────────────────────────────────────
    public const string CommandModeless = """
        using Autodesk.AutoCAD.Runtime;
        using Autodesk.AutoCAD.ApplicationServices;
        using {{NAMESPACE}}.Host;

        [assembly: CommandClass(typeof({{NAMESPACE}}.Commands.{{PROJECT_NAME}}Commands))]

        namespace {{NAMESPACE}}.Commands;

        public class {{PROJECT_NAME}}Commands
        {
            private static Civil3DHostService? _hostService;
            private static UI.{{PROJECT_NAME}}Window? _window;

            [CommandMethod("{{PROJECT_NAME_UPPER}}", CommandFlags.Session)]
            public void Launch()
            {
                if (_window is { IsVisible: true })
                {
                    _window.Activate();
                    return;
                }

                _hostService = new Civil3DHostService();

                StaWindowLauncher.Launch(() =>
                {
                    _window = new UI.{{PROJECT_NAME}}Window(_hostService);
                    return _window;
                });
            }
        }

        /// <summary>Ribbon button command handler.</summary>
        public class {{PROJECT_NAME}}CommandHandler : System.Windows.Input.ICommand
        {
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter)
                => new {{PROJECT_NAME}}Commands().Launch();
        }
        """;

    // ── IHostService.cs ───────────────────────────────────────────────────
    public const string IHostService = """
        using Autodesk.AutoCAD.DatabaseServices;

        namespace {{NAMESPACE}}.Host;

        public interface IHostService
        {
            Task ExecuteAsync(Action<Autodesk.AutoCAD.ApplicationServices.Document> action);
            Database? ActiveDatabase { get; }
        }
        """;

    // ── Civil3DHostService.cs ─────────────────────────────────────────────
    public const string Civil3DHostService = """
        using Autodesk.AutoCAD.ApplicationServices;
        using Autodesk.AutoCAD.DatabaseServices;
        using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

        namespace {{NAMESPACE}}.Host;

        public class Civil3DHostService : IHostService
        {
            public Database? ActiveDatabase
                => AcadApp.DocumentManager.MdiActiveDocument?.Database;

            public Task ExecuteAsync(Action<Document> action)
            {
                var doc = AcadApp.DocumentManager.MdiActiveDocument;
                if (doc is null) return Task.CompletedTask;

                using (doc.LockDocument())
                {
                    action(doc);
                }
                return Task.CompletedTask;
            }
        }
        """;

    // ── Window.xaml ───────────────────────────────────────────────────────
    // (reuses same structure as Revit — same DAR theme)
    public const string WindowXaml = """
        <Window x:Class="{{NAMESPACE}}.UI.{{PROJECT_NAME}}Window"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                Title="{{PROJECT_NAME}}"
                Width="520" Height="400"
                WindowStyle="None"
                AllowsTransparency="True"
                Background="Transparent"
                WindowStartupLocation="CenterScreen">

            <Window.Resources>
                <ResourceDictionary>
                    <ResourceDictionary.MergedDictionaries>
                        <ResourceDictionary Source="../Common/CommonStyles.xaml"/>
                    </ResourceDictionary.MergedDictionaries>
                </ResourceDictionary>
            </Window.Resources>

            <Border CornerRadius="8" Background="#2B2B2B">
                <Border.Effect>
                    <DropShadowEffect BlurRadius="12" ShadowDepth="0" Opacity="0.6" Color="Black"/>
                </Border.Effect>
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="40"/>
                        <RowDefinition Height="*"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>
                    <DockPanel Grid.Row="0" Background="#33373C" LastChildFill="True"
                               MouseLeftButtonDown="TitleBar_MouseLeftButtonDown">
                        <Button DockPanel.Dock="Right" Style="{StaticResource CloseButtonStyle}"
                                Content="✕" Click="CloseButton_Click"/>
                        <Button DockPanel.Dock="Right" Style="{StaticResource MinimizeDashButtonStyle}"
                                Content="─" Click="MinimizeButton_Click"/>
                        {{TITLE_BAR_LOGO}}
                        <TextBlock Text="{{PROJECT_NAME}}" Foreground="#E6E6E6"
                                   VerticalAlignment="Center" FontSize="13"/>
                    </DockPanel>
                    <Grid Grid.Row="1" Margin="20">
                        <TextBlock Text="{{PROJECT_NAME}}" Foreground="#E6E6E6"
                                   HorizontalAlignment="Center" VerticalAlignment="Center"
                                   FontSize="16"/>
                    </Grid>
                    <DockPanel Grid.Row="2" Margin="20,0,20,16" LastChildFill="False">
                        <Button DockPanel.Dock="Right" Style="{StaticResource RunButtonStyle}"
                                Content="Run" Width="90" Command="{Binding RunCommand}"/>
                        <Button DockPanel.Dock="Right" Style="{StaticResource SelectionButtonStyle}"
                                Content="Cancel" Width="80" Margin="0,0,8,0" Click="CloseButton_Click"/>
                    </DockPanel>
                </Grid>
            </Border>
        </Window>
        """;

    // ── Window code-behind (Modal) ────────────────────────────────────────
    public const string WindowCodeBehind = """
        using System.Windows;
        using System.Windows.Input;

        namespace {{NAMESPACE}}.UI;

        public partial class {{PROJECT_NAME}}Window : Window
        {
            public {{PROJECT_NAME}}Window()
            {
                InitializeComponent();
                DataContext = new {{PROJECT_NAME}}ViewModel();
            }

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
            private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
            private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        }
        """;

    // ── Window code-behind (Modeless — accepts host service) ─────────────
    public const string WindowCodeBehindModeless = """
        using System.Windows;
        using System.Windows.Input;
        using {{NAMESPACE}}.Host;

        namespace {{NAMESPACE}}.UI;

        public partial class {{PROJECT_NAME}}Window : Window
        {
            public {{PROJECT_NAME}}Window(IHostService hostService)
            {
                InitializeComponent();
                DataContext = new {{PROJECT_NAME}}ViewModel(hostService);
            }

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
            private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
            private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        }
        """;

    // ── ViewModel (Modal) ─────────────────────────────────────────────────
    public const string ViewModel = """
        using {{NAMESPACE}}.Common;

        namespace {{NAMESPACE}}.UI;

        public class {{PROJECT_NAME}}ViewModel : ViewModelBase
        {
            private string _status = "Ready";
            public string Status
            {
                get => _status;
                set => SetField(ref _status, value);
            }

            public RelayCommand RunCommand { get; }

            public {{PROJECT_NAME}}ViewModel()
            {
                RunCommand = new RelayCommand(OnRun);
            }

            private void OnRun()
            {
                Status = "Running...";
                try
                {
                    // TODO: implement
                }
                catch (Exception ex)
                {
                    Status = "Error";
                    System.Windows.MessageBox.Show(ex.Message, "{{PROJECT_NAME}}", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        """;

    // ── ViewModel (Modeless — has host service) ───────────────────────────
    public const string ViewModelModeless = """
        using {{NAMESPACE}}.Common;
        using {{NAMESPACE}}.Host;

        namespace {{NAMESPACE}}.UI;

        public class {{PROJECT_NAME}}ViewModel : ViewModelBase
        {
            private readonly IHostService _hostService;

            private string _status = "Ready";
            public string Status
            {
                get => _status;
                set => SetField(ref _status, value);
            }

            public RelayCommand RunCommand { get; }

            public {{PROJECT_NAME}}ViewModel(IHostService hostService)
            {
                _hostService = hostService;
                RunCommand = new RelayCommand(OnRun);
            }

            private void OnRun()
            {
                Status = "Running...";
                try
                {
                    // TODO: use _hostService.ExecuteAsync(...) to run on the CAD thread
                }
                catch (Exception ex)
                {
                    Status = "Error";
                    System.Windows.MessageBox.Show(ex.Message, "{{PROJECT_NAME}}", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        """;

    // ── PluginServer.cs ───────────────────────────────────────────────────
    public const string PluginServer = """
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Hosting;
        using Microsoft.Extensions.Hosting;
        using {{NAMESPACE}}.Host;
        using {{NAMESPACE}}.Server.Endpoints;

        namespace {{NAMESPACE}}.Server;

        /// <summary>
        /// Lightweight ASP.NET Core minimal API server hosted inside the Civil 3D plugin.
        /// External tools can POST commands; they are forwarded to the CAD thread
        /// via ExecuteInCommandContextAsync.
        /// </summary>
        public class PluginServer
        {
            private WebApplication? _app;
            public const int Port = 5199;

            public void Start(IHostService hostService)
            {
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls($"http://localhost:{Port}");

                _app = builder.Build();
                ModelEndpoints.Map(_app, hostService);

                Task.Run(() => _app.RunAsync());
            }

            public async Task StopAsync()
            {
                if (_app is not null)
                    await _app.StopAsync();
            }
        }
        """;

    // ── ExternalEventBridge.cs ────────────────────────────────────────────
    public const string ExternalEventBridge = """
        using Autodesk.AutoCAD.ApplicationServices;
        using {{NAMESPACE}}.Host;

        namespace {{NAMESPACE}}.Server;

        /// <summary>
        /// Routes HTTP requests from the embedded server to the CAD API thread
        /// via IHostService.ExecuteAsync.
        /// </summary>
        public static class ExternalEventBridge
        {
            public static async Task<string> RunAsync(IHostService host,
                Func<Autodesk.AutoCAD.DatabaseServices.Database, string> work)
            {
                var result = string.Empty;
                await host.ExecuteAsync(doc =>
                {
                    result = work(doc.Database);
                });
                return result;
            }
        }
        """;

    // ── ModelEndpoints.cs ─────────────────────────────────────────────────
    public const string ModelEndpoints = """
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using {{NAMESPACE}}.Host;

        namespace {{NAMESPACE}}.Server.Endpoints;

        public static class ModelEndpoints
        {
            public static void Map(WebApplication app, IHostService host)
            {
                // GET /status — health check
                app.MapGet("/status", () => Results.Ok(new { status = "ok", plugin = "{{PROJECT_NAME}}" }));

                // GET /layers — list all layers in the active drawing
                app.MapGet("/layers", async () =>
                {
                    var names = new List<string>();
                    await ExternalEventBridge.RunAsync(host, db =>
                    {
                        using var tr = db.TransactionManager.StartTransaction();
                        var lt = (Autodesk.AutoCAD.DatabaseServices.LayerTable)
                            tr.GetObject(db.LayerTableId,
                                Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                        foreach (var id in lt)
                        {
                            var layer = (Autodesk.AutoCAD.DatabaseServices.LayerTableRecord)
                                tr.GetObject(id, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead);
                            names.Add(layer.Name);
                        }
                        tr.Commit();
                        return string.Empty;
                    });
                    return Results.Ok(names);
                });

                // TODO: add more endpoints
            }
        }
        """;
}
