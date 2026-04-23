using DAR.Cli.Models;

namespace DAR.Cli.Templates.Csi;

public static class CsiTemplates
{
    // ── DLL name per product ──────────────────────────────────────────────
    public static string DllName(HostApp host) => host switch
    {
        HostApp.SAP2000   => "SAP2000v1",
        HostApp.ETABS     => "ETABSv1",
        HostApp.CSiBridge => "CSiBridge1",
        _ => "SAP2000v1"
    };

    public static string ProgId(HostApp host) => host switch
    {
        HostApp.SAP2000   => "CSI.SAP2000.API.SapObject",
        HostApp.ETABS     => "CSI.ETABS.API.ETABSObject",
        HostApp.CSiBridge => "CSI.CSiBridge.API.SapObject",
        _ => "CSI.SAP2000.API.SapObject"
    };

    /// <summary>
    /// Returns an MSBuild property name like $(CSiBridgeDir25), $(SAP2000Dir26), etc.
    /// The actual path is defined in Directory.Build.props with OS-conditional logic.
    /// </summary>
    public static string InstallPathProperty(HostApp host, string version) => host switch
    {
        HostApp.SAP2000   => $"$(SAP2000Dir{version})",
        HostApp.ETABS     => $"$(ETABSDir{version})",
        HostApp.CSiBridge => $"$(CSiBridgeDir{version})",
        _ => $"$(SAP2000Dir{version})"
    };

    public static string InstallPath(HostApp host, string version) => host switch
    {
        HostApp.SAP2000   => $@"C:\Program Files\Computers and Structures\SAP2000 {version}",
        HostApp.ETABS     => $@"C:\Program Files\Computers and Structures\ETABS {version}",
        HostApp.CSiBridge => $@"C:\Program Files\Computers and Structures\CSiBridge {version}",
        _ => $@"C:\Program Files\Computers and Structures\SAP2000 {version}"
    };

    // ── .csproj (standard plugin) ─────────────────────────────────────────
    public const string CsProj = """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <RootNamespace>{{NAMESPACE}}</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}</AssemblyName>
            <TargetFramework>net48</TargetFramework>
            <UseWindowsForms>true</UseWindowsForms>
          </PropertyGroup>

        {{BUILD_CONFIGS}}

          <!-- Default reference used when no named build configuration is active (IDE / Debug) -->
          <ItemGroup Condition="{{DEFAULT_REF_CONDITION}}">
            <Reference Include="{{CSI_DLL}}">
              <HintPath>{{CSI_DEFAULT_HINT}}</HintPath>
              <Private>False</Private>
            </Reference>
          </ItemGroup>

          <!-- ── Logging (Serilog with rolling file sink) ──────────────── -->
          <ItemGroup>
            <PackageReference Include="Serilog" Version="3.1.1" />
            <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
          </ItemGroup>

        </Project>
        """;

    public static string BuildConfigBlock(HostApp host, string version)
    {
        var dll    = DllName(host);
        var propName = InstallPathProperty(host, version.TrimStart('v'));
        var prefix = host switch
        {
            HostApp.SAP2000   => "SAP2000",
            HostApp.ETABS     => "ETABS",
            HostApp.CSiBridge => "CSiBridge",
            _ => "CSi"
        };

        return $"""
              <PropertyGroup Condition="'$(Configuration)'=='{prefix}_{version}'">
                <TargetFramework>net48</TargetFramework>
                <DefineConstants>{prefix.ToUpper()}_{version.ToUpper()}</DefineConstants>
                <OutputPath>bin\{prefix}_{version}\</OutputPath>
              </PropertyGroup>

              <ItemGroup Condition="'$(Configuration)'=='{prefix}_{version}'">
                <Reference Include="{dll}">
                  <HintPath>{propName}\{dll}.dll</HintPath>
                  <Private>False</Private>
                </Reference>
              </ItemGroup>
            """;
    }

    // ── cPlugin.cs (standard) ─────────────────────────────────────────────
    public const string CPluginStandard = """
        using System.Windows.Forms;
        using {{CSI_USING}};

        namespace {{NAMESPACE}};

        public class cPlugin : cPluginContract
        {
            private cSapModel _sapModel = null!;
            private cPluginCallback _callback = null!;

            public void Main(ref cSapModel SapModel, ref cPluginCallback ISapPlugin)
            {
                _sapModel = SapModel;
                _callback = ISapPlugin;

                PluginLogger.Initialize("{{VENDOR_ID}}", "{{PROJECT_NAME}}");

                AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
                {
                    var dir  = Path.GetDirectoryName(typeof(cPlugin).Assembly.Location)!;
                    var path = Path.Combine(dir, new System.Reflection.AssemblyName(args.Name).Name + ".dll");
                    return File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
                };

                try
                {
                    var form = new MainForm(_sapModel);
                    form.FormClosed += (_, _) => { PluginLogger.CloseAndFlush(); _callback.Finish(0); };
                    form.Show();
                    // Main() returns immediately; form stays open.
                    // Finish() is called when the form closes.
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "{{PROJECT_NAME}}",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _callback.Finish(1);
                }
            }

            public int Info(ref string Text)
            {
                Text = "{{PROJECT_NAME}} v1.0";
                return 0;
            }
        }
        """;

    // ── cPlugin.cs (standalone — launches external process) ───────────────
    public const string CPluginStandalone = """
        using System.IO;
        using System.Threading;
        using {{CSI_USING}};

        namespace {{NAMESPACE}}.Shim;

        /// <summary>
        /// Thin shim loaded by the CSi application.
        /// Immediately launches the standalone WPF app and returns control to the host.
        /// </summary>
        public class cPlugin : cPluginContract
        {
            public void Main(ref cSapModel SapModel, ref cPluginCallback ISapPlugin)
            {
                var pluginDir = Path.GetDirectoryName(typeof(cPlugin).Assembly.Location)!;
                var exe       = Path.Combine(pluginDir, "{{PROJECT_NAME}}.App.exe");

                if (!File.Exists(exe))
                {
                    System.Windows.Forms.MessageBox.Show(
                        $"Could not find {exe}",
                        "{{PROJECT_NAME}}",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                    ISapPlugin.Finish(1);
                    return;
                }

                // Detect which product is running so the App can reconnect
                var progId = DetectProgId();
                System.Diagnostics.Process.Start(exe, progId);

                // Small delay to let the app start before we release the lock
                Thread.Sleep(2000);

                // Return control to the host immediately — host is no longer blocked
                ISapPlugin.Finish(0);
            }

            private static string DetectProgId()
            {
        #if CSIBRIDGE_V24 || CSIBRIDGE_V25 || CSIBRIDGE_V26
                return "CSI.CSiBridge.API.SapObject";
        #elif ETABS_V21 || ETABS_V22
                return "CSI.ETABS.API.ETABSObject";
        #else
                return "CSI.SAP2000.API.SapObject";
        #endif
            }

            public int Info(ref string Text)
            {
                Text = "{{PROJECT_NAME}} v1.0";
                return 0;
            }
        }
        """;

    // ── MainForm.cs (standard) ────────────────────────────────────────────
    public const string MainForm = """
        using System.Windows.Forms;
        using {{CSI_USING}};

        namespace {{NAMESPACE}};

        public partial class MainForm : Form
        {
            private readonly cSapModel _model;

            public MainForm(cSapModel model)
            {
                _model = model;
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                Text      = "{{PROJECT_NAME}}";
                Width     = 520;
                Height    = 400;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition   = FormStartPosition.CenterScreen;

                var runBtn = new Button
                {
                    Text   = "Run",
                    Dock   = DockStyle.Bottom,
                    Height = 36,
                };
                runBtn.Click += (_, _) => OnRun();
                Controls.Add(runBtn);
            }

            private void OnRun()
            {
                // TODO: use _model to interact with the CSi application
            }
        }
        """;

    // ── Standalone App — App.xaml ─────────────────────────────────────────
    public const string StandaloneAppXaml = """
        <Application x:Class="{{NAMESPACE}}.App.App"
                     xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                     StartupUri="MainWindow.xaml">
        </Application>
        """;

    // ── Standalone App — App.xaml.cs ──────────────────────────────────────
    public const string StandaloneAppCodeBehind = """
        using System.Windows;

        namespace {{NAMESPACE}}.App;

        public partial class App : Application
        {
            protected override void OnStartup(StartupEventArgs e)
            {
                base.OnStartup(e);

                // Receive the COM ProgID passed as argument from the shim
                var progId = e.Args.Length > 0 ? e.Args[0] : "{{DEFAULT_PROGID}}";

                var window = new MainWindow(progId);
                window.Show();
            }
        }
        """;

    // ── Standalone App — MainWindow.xaml ──────────────────────────────────
    public const string StandaloneMainWindowXaml = """
        <Window x:Class="{{NAMESPACE}}.App.MainWindow"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                Title="{{PROJECT_NAME}}"
                Width="560" Height="440"
                WindowStyle="None"
                AllowsTransparency="True"
                Background="Transparent"
                WindowStartupLocation="CenterScreen">

            <Window.Resources>
                <ResourceDictionary>
                    <ResourceDictionary.MergedDictionaries>
                        <ResourceDictionary Source="Common/CommonStyles.xaml"/>
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
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock Text="{Binding ConnectionStatus}" Foreground="#E6E6E6"
                                       HorizontalAlignment="Center" FontSize="13" Margin="0,0,0,12"/>
                            <!-- TODO: add your content here -->
                        </StackPanel>
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

    // ── Standalone App — MainWindow.xaml.cs ───────────────────────────────
    public const string StandaloneMainWindowCodeBehind = """
        using System.Windows;
        using System.Windows.Input;

        namespace {{NAMESPACE}}.App;

        public partial class MainWindow : Window
        {
            public MainWindow(string progId)
            {
                InitializeComponent();
                DataContext = new MainWindowViewModel(progId);
            }

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
            private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
            private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        }
        """;

    // ── Standalone App — MainWindowViewModel.cs ───────────────────────────
    public const string StandaloneViewModel = """
        using {{CSI_USING}};
        using {{NAMESPACE}}.Common;

        namespace {{NAMESPACE}}.App;

        public class MainWindowViewModel : ViewModelBase
        {
            private string _connectionStatus = "Connecting...";
            public string ConnectionStatus
            {
                get => _connectionStatus;
                set => SetField(ref _connectionStatus, value);
            }

            public RelayCommand RunCommand { get; }

            private cSapModel? _model;

            public MainWindowViewModel(string progId)
            {
                RunCommand = new RelayCommand(OnRun, () => _model is not null);
                Task.Run(() => Connect(progId));
            }

            private void OnRun()
            {
                if (_model is null) { ConnectionStatus = "Not connected"; return; }
                ConnectionStatus = "Running...";
                try
                {
                    // TODO: use _model to interact with the application
                }
                catch (Exception ex)
                {
                    ConnectionStatus = $"Error: {ex.Message}";
                    System.Windows.MessageBox.Show(ex.Message, "{{PROJECT_NAME}}", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }

            private void Connect(string progId)
            {
                try
                {
                    cHelper helper = new Helper();
                    var app    = helper.GetObject(progId);
                    _model     = app.SapModel;
                    ConnectionStatus = "Connected";
                    System.Windows.Threading.Dispatcher.CurrentDispatcher
                        .Invoke(System.Windows.Input.CommandManager.InvalidateRequerySuggested);
                }
                catch (Exception ex)
                {
                    ConnectionStatus = $"Connection failed: {ex.Message}";
                }
            }
        }
        """;

    // ── Standalone .csproj ────────────────────────────────────────────────
    public const string StandaloneAppCsProj = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>WinExe</OutputType>
            <RootNamespace>{{NAMESPACE}}.App</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}.App</AssemblyName>
            <TargetFramework>net48</TargetFramework>
            <UseWPF>true</UseWPF>
          </PropertyGroup>
          <ItemGroup>
            <ProjectReference Include="..\{{PROJECT_NAME}}.Core\{{PROJECT_NAME}}.Core.csproj"/>
          </ItemGroup>
        {{CSI_REFERENCES}}

          <!-- Default reference used when no named build configuration is active -->
          <ItemGroup Condition="{{DEFAULT_REF_CONDITION}}">
            <Reference Include="{{CSI_DLL}}">
              <HintPath>{{CSI_DEFAULT_HINT}}</HintPath>
              <Private>False</Private>
            </Reference>
          </ItemGroup>

        </Project>
        """;

    // ── Core .csproj ──────────────────────────────────────────────────────
    public const string CoreCsProj = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <RootNamespace>{{NAMESPACE}}.Core</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}.Core</AssemblyName>
            <TargetFramework>netstandard2.0</TargetFramework>
          </PropertyGroup>
        </Project>
        """;

    // ── Shim .csproj ──────────────────────────────────────────────────────
    public const string ShimCsProj = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Library</OutputType>
            <RootNamespace>{{NAMESPACE}}.Shim</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}</AssemblyName>
            <TargetFramework>net48</TargetFramework>
            <UseWindowsForms>true</UseWindowsForms>
          </PropertyGroup>
        {{BUILD_CONFIGS}}

          <!-- Default reference used when no named build configuration is active -->
          <ItemGroup Condition="{{DEFAULT_REF_CONDITION}}">
            <Reference Include="{{CSI_DLL}}">
              <HintPath>{{CSI_DEFAULT_HINT}}</HintPath>
              <Private>False</Private>
            </Reference>
          </ItemGroup>

        </Project>
        """;
}
