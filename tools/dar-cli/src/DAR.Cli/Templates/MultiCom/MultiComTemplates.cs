namespace DAR.Cli.Templates.MultiCom;

public static class MultiComTemplates
{
    // ── IHostConnection.cs ──────────────────────────────────────────────
    public const string IHostConnection = """
        namespace {{NAMESPACE}}.Connections;

        /// <summary>
        /// Common interface for all COM host connections.
        /// Implement per host — each manages its own COM lifecycle.
        /// </summary>
        public interface IHostConnection : IDisposable
        {
            string HostName    { get; }
            string Status      { get; set; }
            bool   IsConnected { get; }
            void   Connect();
            void   Disconnect();

            /// <summary>The COM application object (dynamic to avoid interop DLL dependency).</summary>
            dynamic? HostApp { get; }
        }
        """;

    // ── Civil3DConnection.cs ────────────────────────────────────────────
    public const string Civil3DConnection = """
        using System.Runtime.InteropServices;

        namespace {{NAMESPACE}}.Connections;

        public class Civil3DConnection : IHostConnection
        {
            public string   HostName    => "Civil 3D";
            public string   Status      { get; set; } = "Disconnected";
            public bool     IsConnected => HostApp is not null;
            public dynamic? HostApp     { get; private set; }

            public void Connect()
            {
                try
                {
                    HostApp = Marshal.GetActiveObject("AutoCAD.Application");
                    Status  = $"Connected — {HostApp!.ActiveDocument.Name}";
                }
                catch (Exception ex)
                {
                    Status = $"Error: {ex.Message}";
                }
            }

            public void Disconnect()
            {
                HostApp = null;
                Status  = "Disconnected";
            }

            public void Dispose() => Disconnect();
        }
        """;

    // ── CSiBridgeConnection.cs ──────────────────────────────────────────
    public const string CSiBridgeConnection = """
        using CSiBridge1;

        namespace {{NAMESPACE}}.Connections;

        public class CSiBridgeConnection : IHostConnection
        {
            public string    HostName    => "CSiBridge";
            public string    Status      { get; set; } = "Disconnected";
            public bool      IsConnected => _model is not null;
            public dynamic?  HostApp     { get; private set; }

            private cSapModel? _model;
            public  cSapModel? Model => _model;

            public void Connect()
            {
                try
                {
                    cHelper helper = new Helper();
                    var app    = helper.GetObject("CSI.CSiBridge.API.SapObject");
                    _model     = app.SapModel;
                    HostApp    = app;
                    Status     = "Connected";
                }
                catch (Exception ex)
                {
                    Status = $"Error: {ex.Message}";
                }
            }

            public void Disconnect()
            {
                _model  = null;
                HostApp = null;
                Status  = "Disconnected";
            }

            public void Dispose() => Disconnect();
        }
        """;

    // ── SAP2000Connection.cs ────────────────────────────────────────────
    public const string SAP2000Connection = """
        using SAP2000v1;

        namespace {{NAMESPACE}}.Connections;

        public class SAP2000Connection : IHostConnection
        {
            public string    HostName    => "SAP2000";
            public string    Status      { get; set; } = "Disconnected";
            public bool      IsConnected => _model is not null;
            public dynamic?  HostApp     { get; private set; }

            private cSapModel? _model;
            public  cSapModel? Model => _model;

            public void Connect()
            {
                try
                {
                    cHelper helper = new Helper();
                    var app    = helper.GetObject("CSI.SAP2000.API.SapObject");
                    _model     = app.SapModel;
                    HostApp    = app;
                    Status     = "Connected";
                }
                catch (Exception ex)
                {
                    Status = $"Error: {ex.Message}";
                }
            }

            public void Disconnect()
            {
                _model  = null;
                HostApp = null;
                Status  = "Disconnected";
            }

            public void Dispose() => Disconnect();
        }
        """;

    // ── ETABSConnection.cs ──────────────────────────────────────────────
    public const string ETABSConnection = """
        using ETABSv1;

        namespace {{NAMESPACE}}.Connections;

        public class ETABSConnection : IHostConnection
        {
            public string    HostName    => "ETABS";
            public string    Status      { get; set; } = "Disconnected";
            public bool      IsConnected => _model is not null;
            public dynamic?  HostApp     { get; private set; }

            private cSapModel? _model;
            public  cSapModel? Model => _model;

            public void Connect()
            {
                try
                {
                    cHelper helper = new Helper();
                    var app    = helper.GetObject("CSI.ETABS.API.ETABSObject");
                    _model     = app.SapModel;
                    HostApp    = app;
                    Status     = "Connected";
                }
                catch (Exception ex)
                {
                    Status = $"Error: {ex.Message}";
                }
            }

            public void Disconnect()
            {
                _model  = null;
                HostApp = null;
                Status  = "Disconnected";
            }

            public void Dispose() => Disconnect();
        }
        """;

    // ── MainWindowViewModel.cs ──────────────────────────────────────────
    public const string ViewModel = """
        using System.Collections.ObjectModel;
        using System.Windows.Input;
        using {{NAMESPACE}}.Common;
        using {{NAMESPACE}}.Connections;

        namespace {{NAMESPACE}};

        public class MainWindowViewModel : ViewModelBase
        {
            public ObservableCollection<IHostConnection> Connections { get; } = new();

            public RelayCommand ConnectAllCommand  { get; }
            public RelayCommand RunCommand         { get; }

            public MainWindowViewModel()
            {
                {{CONNECTION_INITS}}

                ConnectAllCommand = new RelayCommand(ConnectAll);
                RunCommand        = new RelayCommand(OnRun, () => Connections.Any(c => c.IsConnected));
            }

            private void ConnectAll()
            {
                foreach (var conn in Connections)
                {
                    if (!conn.IsConnected)
                        conn.Connect();
                }
                OnPropertyChanged(nameof(Connections));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }

            private void OnRun()
            {
                try
                {
                    // TODO: implement cross-application logic here
                    // Example: read from Civil3D, push to CSiBridge
                    //
                    // var c3d = Connections.OfType<Civil3DConnection>().FirstOrDefault();
                    // var csi = Connections.OfType<CSiBridgeConnection>().FirstOrDefault();
                    // if (c3d?.IsConnected == true && csi?.IsConnected == true) { ... }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "{{PROJECT_NAME}}",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        """;

    // ── MainWindow.xaml ─────────────────────────────────────────────────
    public const string WindowXaml = """
        <Window x:Class="{{NAMESPACE}}.MainWindow"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                Title="{{PROJECT_NAME}}" Width="500" Height="400"
                WindowStyle="None" AllowsTransparency="True" Background="Transparent"
                WindowStartupLocation="CenterScreen">
            <Window.Resources>
                <ResourceDictionary Source="Common/CommonStyles.xaml"/>
            </Window.Resources>

            <Border Background="#2B2B2B" CornerRadius="8">
                <Border.Effect>
                    <DropShadowEffect BlurRadius="12" ShadowDepth="0" Opacity="0.6"/>
                </Border.Effect>
                <DockPanel>
                    <!-- Title bar -->
                    <DockPanel DockPanel.Dock="Top" Background="#33373C" Height="40"
                               MouseLeftButtonDown="TitleBar_MouseLeftButtonDown">
                        <Button DockPanel.Dock="Right" Style="{StaticResource CloseButtonStyle}"
                                Content="✕" Click="CloseButton_Click"/>
                        <Button DockPanel.Dock="Right" Style="{StaticResource MinimizeDashButtonStyle}"
                                Content="─" Click="MinimizeButton_Click"/>
                        {{TITLE_BAR_LOGO}}
                        <TextBlock Text="{{PROJECT_NAME}}" Foreground="#E6E6E6"
                                   VerticalAlignment="Center" FontSize="14" FontWeight="SemiBold" Margin="8,0"/>
                    </DockPanel>

                    <!-- Content -->
                    <Grid Margin="16">
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="*"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <TextBlock Grid.Row="0" Text="Connections" Foreground="#E6E6E6"
                                   FontSize="16" FontWeight="SemiBold" Margin="0,0,0,12"/>

                        <!-- Connection list -->
                        <ItemsControl Grid.Row="1" ItemsSource="{Binding Connections}">
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background="#1E1E1E" CornerRadius="4" Padding="12,8" Margin="0,0,0,6">
                                        <Grid>
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition Width="Auto"/>
                                                <ColumnDefinition Width="*"/>
                                                <ColumnDefinition Width="Auto"/>
                                            </Grid.ColumnDefinitions>
                                            <Ellipse Grid.Column="0" Width="10" Height="10" Margin="0,0,10,0"
                                                     VerticalAlignment="Center">
                                                <Ellipse.Style>
                                                    <Style TargetType="Ellipse">
                                                        <Setter Property="Fill" Value="#666"/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding="{Binding IsConnected}" Value="True">
                                                                <Setter Property="Fill" Value="{StaticResource PrimaryActionBrush}"/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </Ellipse.Style>
                                            </Ellipse>
                                            <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                                <TextBlock Text="{Binding HostName}" Foreground="#E6E6E6" FontWeight="SemiBold"/>
                                                <TextBlock Text="{Binding Status}" Foreground="#999" FontSize="11"/>
                                            </StackPanel>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>

                        <!-- Buttons -->
                        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0">
                            <Button Style="{StaticResource SelectionButtonStyle}" Content="Connect All"
                                    Command="{Binding ConnectAllCommand}" Margin="0,0,8,0"/>
                            <Button Style="{StaticResource RunButtonStyle}" Content="Run"
                                    Command="{Binding RunCommand}"/>
                        </StackPanel>
                    </Grid>
                </DockPanel>
            </Border>
        </Window>
        """;

    // ── MainWindow.xaml.cs ──────────────────────────────────────────────
    public const string WindowCodeBehind = """
        using System.Windows;
        using System.Windows.Input;

        namespace {{NAMESPACE}};

        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
                DataContext = new MainWindowViewModel();
            }

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
            private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
            private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        }
        """;

    // ── App.xaml ────────────────────────────────────────────────────────
    public const string AppXaml = """
        <Application x:Class="{{NAMESPACE}}.App"
                     xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                     StartupUri="MainWindow.xaml">
        </Application>
        """;

    // ── App.xaml.cs ─────────────────────────────────────────────────────
    public const string AppCodeBehind = """
        using System.Windows;

        namespace {{NAMESPACE}};

        public partial class App : Application { }
        """;

    // ── .csproj ─────────────────────────────────────────────────────────
    public const string CsProj = """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <OutputType>WinExe</OutputType>
            <RootNamespace>{{NAMESPACE}}</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}</AssemblyName>
            <TargetFramework>net48</TargetFramework>
            <UseWPF>true</UseWPF>
          </PropertyGroup>

          <ItemGroup>
            <Reference Include="Microsoft.CSharp"/>
          </ItemGroup>

        {{CSI_REFERENCES}}

        </Project>
        """;

    // ── Helpers ─────────────────────────────────────────────────────────

    /// <summary>Build an ItemGroup with DLL references for a given COM host.</summary>
    public static string DllReference(Models.ComHost host, string hintPath)
    {
        var dll = host switch
        {
            Models.ComHost.SAP2000   => "SAP2000v1",
            Models.ComHost.ETABS     => "ETABSv1",
            Models.ComHost.CSiBridge => "CSiBridge1",
            _ => null
        };

        if (dll is null) return string.Empty; // Civil3D uses dynamic, no DLL ref

        return $"""
              <ItemGroup>
                <Reference Include="{dll}">
                  <HintPath>{hintPath}</HintPath>
                  <Private>False</Private>
                </Reference>
              </ItemGroup>
            """;
    }
}
