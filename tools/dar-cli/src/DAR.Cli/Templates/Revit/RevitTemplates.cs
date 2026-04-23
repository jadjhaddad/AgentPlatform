namespace DAR.Cli.Templates.Revit;

public static class RevitTemplates
{
    // ── .csproj ───────────────────────────────────────────────────────────
    public const string CsProj = """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <RootNamespace>{{NAMESPACE}}</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}</AssemblyName>
            <!-- Default config — override via named build configurations below -->
            <TargetFramework>net48</TargetFramework>
          </PropertyGroup>

          <!-- ── Per-version build configurations ───────────────────────── -->
        {{BUILD_CONFIGS}}

          <!-- ── WPF (all versions) ─────────────────────────────────────── -->
          <PropertyGroup>
            <UseWPF>true</UseWPF>
          </PropertyGroup>

          <!-- ── runtimeconfig.json (Revit 2025+ / net8.0-windows) ───────── -->
          {{RUNTIMECONFIG_ITEM}}

          <!-- ── ASP.NET Core (EmbeddedServer only — net8.0+) ───────────── -->
          {{ASPNET_REF}}

          {{RESOURCE_ITEMS}}

          <!-- ── .addin manifest (copy to output) ───────────────────────── -->
          <ItemGroup>
            <Content Include="{{PROJECT_NAME}}.addin">
              <CopyToOutputDirectory>Always</CopyToOutputDirectory>
            </Content>
          </ItemGroup>

          <!-- ── Logging (Serilog with rolling file sink) ──────────────── -->
          <ItemGroup>
            <PackageReference Include="Serilog" Version="3.1.1" />
            <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
          </ItemGroup>

          <!-- ── Resources dir property ─────────────────────────────────── -->
          <PropertyGroup>
            <ResourcesDir>$(SolutionDir)Resources</ResourcesDir>
          </PropertyGroup>

          <!-- ── Generate PackageContents.xml (safe to overwrite — no GUID) -->
          <Target Name="GenerateApplicationPackage" AfterTargets="Build">
            <WriteLinesToFile File="$(ResourcesDir)\PackageContents.xml" Overwrite="true"
              Lines="&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;&#xA;&lt;ApplicationPackage&gt;&#xA;{{PACKAGE_CONTENTS_COMPONENTS}}&lt;/ApplicationPackage&gt;" />
          </Target>

          <!-- ── Build bundle and deploy to C:\ProgramData\Autodesk\ApplicationPlugins -->
          <!-- Skipped on Linux/WSL dotnet build — run via 'vs build' to deploy       -->
          <Target Name="CreateBundleFolder" AfterTargets="Build"
                  Condition="!$([MSBuild]::IsOSPlatform('Linux'))">
            <PropertyGroup>
              <PluginName Condition="'$(PluginName)' == ''">$(SolutionName)</PluginName>
              <PluginYear Condition="'$(Configuration)'=='RVT2023'">RVT2023</PluginYear>
              <PluginYear Condition="'$(Configuration)'=='RVT2024'">RVT2024</PluginYear>
              <PluginYear Condition="'$(Configuration)'=='RVT2025'">RVT2025</PluginYear>
              <PluginYear Condition="'$(Configuration)'=='RVT2026'">RVT2026</PluginYear>
              <BundleRoot>$(SolutionDir)$(PluginName).bundle\</BundleRoot>
              <BundleContentDir>$(BundleRoot)Contents\$(PluginYear)\</BundleContentDir>
              <AutodeskPluginDir>C:\ProgramData\Autodesk\ApplicationPlugins\$(PluginName).bundle\</AutodeskPluginDir>
            </PropertyGroup>
            <MakeDir Directories="$(BundleContentDir)" />
            <ItemGroup>
              <AllDllFiles Include="$(OutputPath)*.dll" />
            </ItemGroup>
            <Copy SourceFiles="@(AllDllFiles)" DestinationFolder="$(BundleContentDir)" SkipUnchangedFiles="true" />
            <Copy SourceFiles="$(SolutionDir)Resources\PackageContents.xml" DestinationFolder="$(BundleRoot)"
                  Condition="Exists('$(SolutionDir)Resources\PackageContents.xml')" SkipUnchangedFiles="true" />
            <ItemGroup>
              <AddinFile Include="$(SolutionDir)Resources\*.addin" />
            </ItemGroup>
            <Copy SourceFiles="@(AddinFile)" DestinationFolder="$(BundleContentDir)" SkipUnchangedFiles="true"
                  Condition="@(AddinFile-&gt;Count()) != 0" />
            <ItemGroup>
              <BundleFiles Include="$(BundleRoot)**\*.*" />
            </ItemGroup>
            <Copy SourceFiles="@(BundleFiles)"
                  DestinationFiles="@(BundleFiles->'$(AutodeskPluginDir)%(RecursiveDir)%(Filename)%(Extension)')"
                  SkipUnchangedFiles="true" />
            <Message Text="[Revit] Deployed → $(AutodeskPluginDir)" Importance="high" />
          </Target>

        </Project>
        """;

    // ── runtimeconfig.json (net8.0-windows only) ─────────────────────────
    // Required for Revit 2025+ to locate and load the .NET 8 runtime correctly.
    public const string RuntimeConfig = """
        {
          "runtimeOptions": {
            "tfm": "net8.0-windows",
            "rollForward": "LatestMinor",
            "frameworks": [
              {
                "name": "Microsoft.NETCore.App",
                "version": "8.0.0"
              },
              {
                "name": "Microsoft.WindowsDesktop.App",
                "version": "8.0.0"
              }
            ],
            "configProperties": {
              "System.Reflection.Metadata.MetadataUpdater.IsSupported": false,
              "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false
            }
          }
        }
        """;

    // ── .addin ────────────────────────────────────────────────────────────
    public const string Addin = """
        <?xml version="1.0" encoding="utf-8"?>
        <RevitAddIns>
          <AddIn Type="Application">
            <Name>{{PROJECT_NAME}}</Name>
            <Assembly>{{PROJECT_NAME}}.dll</Assembly>
            <FullClassName>{{NAMESPACE}}.Application</FullClassName>
            <ClientId>{{ADDIN_GUID}}</ClientId>
            <VendorId>{{VENDOR_ID}}</VendorId>
            <VendorDescription>{{AUTHOR}}</VendorDescription>
          </AddIn>
        </RevitAddIns>
        """;

    // ── Application.cs (ribbon) ───────────────────────────────────────────
    public const string ApplicationRibbon = """
        using System.IO;
        using System.Reflection;
        using Autodesk.Revit.UI;
        using {{NAMESPACE}}.Commands;

        namespace {{NAMESPACE}};

        public class Application : IExternalApplication
        {
            public static Application? Instance { get; private set; }

            public Result OnStartup(UIControlledApplication application)
            {
                Instance = this;
                PluginLogger.Initialize("{{VENDOR_ID}}", "{{PROJECT_NAME}}");

        #if NET48
                AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
                {
                    var dir = Path.GetDirectoryName(typeof(Application).Assembly.Location)!;
                    var path = Path.Combine(dir, new System.Reflection.AssemblyName(args.Name).Name + ".dll");
                    return File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
                };
        #endif

                CreateRibbon(application);
                return Result.Succeeded;
            }

            public Result OnShutdown(UIControlledApplication application)
            {
                PluginLogger.CloseAndFlush();
                return Result.Succeeded;
            }

            private static void CreateRibbon(UIControlledApplication application)
            {
                var panel = application.CreateRibbonPanel("{{PROJECT_NAME}}");

                var btnData = new PushButtonData(
                    "{{PROJECT_NAME}}Cmd",
                    "{{PROJECT_NAME}}",
                    typeof(Application).Assembly.Location,
                    typeof({{PROJECT_NAME}}Command).FullName)
                {
                    LargeImage = LoadIcon("{{LOGO_FOLDER}}/{{LOGO_FILE}}"),
                    Image      = LoadIcon("{{LOGO_FOLDER}}/{{LOGO_FILE}}"),
                    ToolTip    = "{{DESCRIPTION}}",
                };

                panel.AddItem(btnData);
            }

            private static System.Windows.Media.ImageSource? LoadIcon(string relativePath)
            {
                try
                {
                    var dir  = Path.GetDirectoryName(typeof(Application).Assembly.Location)!;
                    var path = Path.Combine(dir, relativePath.Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(path)) return null;
                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.UriSource    = new Uri(path, UriKind.Absolute);
                    img.CacheOption  = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
                catch { return null; }
            }
        }
        """;

    // ── Application.cs (empty / command-only) ────────────────────────────
    public const string ApplicationEmpty = """
        using System.IO;
        using System.Reflection;
        using Autodesk.Revit.UI;

        namespace {{NAMESPACE}};

        public class Application : IExternalApplication
        {
            public Result OnStartup(UIControlledApplication application)
            {
                PluginLogger.Initialize("{{VENDOR_ID}}", "{{PROJECT_NAME}}");

        #if NET48
                AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
                {
                    var dir = Path.GetDirectoryName(typeof(Application).Assembly.Location)!;
                    var path = Path.Combine(dir, new System.Reflection.AssemblyName(args.Name).Name + ".dll");
                    return File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
                };
        #endif
                return Result.Succeeded;
            }

            public Result OnShutdown(UIControlledApplication application)
            {
                PluginLogger.CloseAndFlush();
                return Result.Succeeded;
            }
        }
        """;

    // ── Command.cs (modal — blocking dialog) ──────────────────────────────
    public const string Command = """
        using Autodesk.Revit.Attributes;
        using Autodesk.Revit.DB;
        using Autodesk.Revit.UI;

        namespace {{NAMESPACE}}.Commands;

        [Transaction(TransactionMode.Manual)]
        public class {{PROJECT_NAME}}Command : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc   = uiDoc.Document;

                var window = new UI.{{PROJECT_NAME}}Window();
                window.ShowDialog();

                return Result.Succeeded;
            }
        }
        """;

    // ── Command.cs (modeless — persistent window via ExternalEvent) ────────
    public const string CommandModeless = """
        using System.Threading;
        using System.Windows;
        using System.Windows.Threading;
        using Autodesk.Revit.Attributes;
        using Autodesk.Revit.DB;
        using Autodesk.Revit.UI;
        using {{NAMESPACE}}.Host;
        using {{NAMESPACE}}.UI;

        namespace {{NAMESPACE}}.Commands;

        [Transaction(TransactionMode.ReadOnly)]
        public class {{PROJECT_NAME}}Command : IExternalCommand
        {
            private static {{PROJECT_NAME}}Window? _window;
            private static RevitHostService?       _hostService;

            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                if (_window is { IsVisible: true })
                {
                    _window.Activate();
                    return Result.Succeeded;
                }

                _hostService = new RevitHostService();

                var thread = new Thread(() =>
                {
                    _window = new {{PROJECT_NAME}}Window(_hostService);
                    _window.Closed += (_, _) => Dispatcher.CurrentDispatcher.InvokeShutdown();
                    _window.Show();
                    Dispatcher.Run();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Name = "{{PROJECT_NAME}}Window_Thread";
                thread.Start();

                return Result.Succeeded;
            }
        }
        """;

    // ── Command.cs (command-only — no WPF window) ─────────────────────────
    public const string CommandOnly = """
        using Autodesk.Revit.Attributes;
        using Autodesk.Revit.DB;
        using Autodesk.Revit.UI;

        namespace {{NAMESPACE}}.Commands;

        [Transaction(TransactionMode.Manual)]
        public class {{PROJECT_NAME}}Command : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc   = uiDoc.Document;

                // TODO: implement command logic here
                TaskDialog.Show("{{PROJECT_NAME}}", "Command executed successfully.");

                return Result.Succeeded;
            }
        }
        """;

    // ── WPF Window .xaml ──────────────────────────────────────────────────
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

            <!-- Drop shadow wrapper -->
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

                    <!-- Title bar -->
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

                    <!-- Content area -->
                    <Grid Grid.Row="1" Margin="20">
                        <!-- TODO: add your content here -->
                        <TextBlock Text="{{PROJECT_NAME}}" Foreground="#E6E6E6"
                                   HorizontalAlignment="Center" VerticalAlignment="Center"
                                   FontSize="16"/>
                    </Grid>

                    <!-- Footer buttons -->
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

    // ── WPF Window code-behind ────────────────────────────────────────────
    // ── Window code-behind (modal) ────────────────────────────────────────
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

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                => DragMove();

            private void CloseButton_Click(object sender, RoutedEventArgs e)
                => Close();

            private void MinimizeButton_Click(object sender, RoutedEventArgs e)
                => WindowState = WindowState.Minimized;
        }
        """;

    // ── Window code-behind (modeless — accepts IHostService) ─────────────
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

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                => DragMove();

            private void CloseButton_Click(object sender, RoutedEventArgs e)
                => Close();

            private void MinimizeButton_Click(object sender, RoutedEventArgs e)
                => WindowState = WindowState.Minimized;
        }
        """;

    // ── ViewModel (modal) ─────────────────────────────────────────────────
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
                    Autodesk.Revit.UI.TaskDialog.Show("{{PROJECT_NAME}}", ex.Message);
                }
            }
        }
        """;

    // ── ViewModel (modeless — holds IHostService) ─────────────────────────
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
                RunCommand   = new RelayCommand(OnRun);
            }

            private void OnRun()
            {
                Status = "Running...";
                try
                {
                    // TODO: use _hostService.ExecuteAsync(uiApp => { ... }) to run on the Revit API thread
                }
                catch (Exception ex)
                {
                    Status = "Error";
                    Autodesk.Revit.UI.TaskDialog.Show("{{PROJECT_NAME}}", ex.Message);
                }
            }
        }
        """;

    // ── IHostService.cs ───────────────────────────────────────────────────
    public const string IHostService = """
        using Autodesk.Revit.DB;
        using Autodesk.Revit.UI;

        namespace {{NAMESPACE}}.Host;

        /// <summary>
        /// Abstraction over the Revit host — allows the ViewModel to trigger
        /// Revit API calls without directly depending on UIApplication.
        /// </summary>
        public interface IHostService
        {
            /// <summary>Execute an action on the Revit API thread and await its completion.</summary>
            Task ExecuteAsync(Action<UIApplication> action);

            Document? ActiveDocument { get; }
        }
        """;

    // ── RevitHostService.cs ───────────────────────────────────────────────
    public const string RevitHostService = """
        using Autodesk.Revit.DB;
        using Autodesk.Revit.UI;

        namespace {{NAMESPACE}}.Host;

        /// <summary>
        /// Implements IHostService using ExternalEvent + TaskCompletionSource
        /// so that ViewModel code can await Revit API calls from a modeless window.
        /// </summary>
        public class RevitHostService : IHostService, IExternalEventHandler
        {
            private readonly ExternalEvent _externalEvent;
            private Action<UIApplication>? _pendingAction;
            private TaskCompletionSource<bool>? _tcs;
            private UIApplication? _uiApp;

            public Document? ActiveDocument => _uiApp?.ActiveUIDocument?.Document;

            public RevitHostService()
            {
                _externalEvent = ExternalEvent.Create(this);
            }

            public async Task ExecuteAsync(Action<UIApplication> action)
            {
                _pendingAction = action;
                _tcs = new TaskCompletionSource<bool>();
                _externalEvent.Raise();
                await _tcs.Task;
            }

            // Called by Revit on the API thread
            void IExternalEventHandler.Execute(UIApplication app)
            {
                _uiApp = app;
                try
                {
                    _pendingAction?.Invoke(app);
                    _tcs?.SetResult(true);
                }
                catch (Exception ex)
                {
                    _tcs?.SetException(ex);
                }
                finally
                {
                    _pendingAction = null;
                }
            }

            string IExternalEventHandler.GetName() => "{{PROJECT_NAME}}HostService";
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
        /// Lightweight ASP.NET Core minimal API server hosted inside the Revit plugin.
        /// External tools can POST commands to this server; they are forwarded to Revit
        /// via ExternalEvent on the API thread.
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
        using Autodesk.Revit.UI;
        using {{NAMESPACE}}.Host;

        namespace {{NAMESPACE}}.Server;

        /// <summary>
        /// Routes HTTP requests from the embedded server to the Revit API thread
        /// via IHostService.ExecuteAsync.
        /// </summary>
        public static class ExternalEventBridge
        {
            public static async Task<string> RunAsync(IHostService host, Func<Autodesk.Revit.DB.Document, string> work)
            {
                var result = string.Empty;
                await host.ExecuteAsync(uiApp =>
                {
                    var doc = uiApp.ActiveUIDocument?.Document;
                    if (doc is not null)
                        result = work(doc);
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

                // GET /views — list open views
                app.MapGet("/views", async () =>
                {
                    var names = new List<string>();
                    await ExternalEventBridge.RunAsync(host, doc =>
                    {
                        var collector = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                            .OfClass(typeof(Autodesk.Revit.DB.View));
                        names.AddRange(collector
                            .Cast<Autodesk.Revit.DB.View>()
                            .Where(v => !v.IsTemplate)
                            .Select(v => v.Name));
                        return string.Empty;
                    });
                    return Results.Ok(names);
                });

                // TODO: add more endpoints
            }
        }
        """;

    // ── .sln ──────────────────────────────────────────────────────────────
    public const string Solution = """

        Microsoft Visual Studio Solution File, Format Version 12.00
        # Visual Studio Version 17
        VisualStudioVersion = 17.0.31903.59
        Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "{{PROJECT_NAME}}", "{{PROJECT_NAME}}\{{PROJECT_NAME}}.csproj", "{{{SLN_GUID}}}"
        EndProject
        Global
            GlobalSection(SolutionConfigurationPlatforms) = preSolution
        {{SLN_CONFIGS}}
            EndGlobalSection
            GlobalSection(ProjectConfigurationPlatforms) = postSolution
        {{SLN_PROJECT_CONFIGS}}
            EndGlobalSection
        EndGlobal
        """;
}
