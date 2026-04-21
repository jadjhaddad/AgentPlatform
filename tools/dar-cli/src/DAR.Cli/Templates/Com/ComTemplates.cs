using DAR.Cli.Models;

namespace DAR.Cli.Templates.Com;

public static class ComTemplates
{
    public static string GetActiveObjectCall(HostApp host) => host switch
    {
        HostApp.ComCivil3D   => """Marshal.GetActiveObject("AutoCAD.Application")""",
        HostApp.ComSAP2000   => """((cHelper)new Helper()).GetObject("CSI.SAP2000.API.SapObject")""",
        HostApp.ComETABS     => """((cHelper)new Helper()).GetObject("CSI.ETABS.API.ETABSObject")""",
        HostApp.ComCSiBridge => """((cHelper)new Helper()).GetObject("CSI.CSiBridge.API.SapObject")""",
        _ => throw new ArgumentOutOfRangeException()
    };

    public static string HostTypeName(HostApp host) => host switch
    {
        HostApp.ComCivil3D   => "Civil 3D",
        HostApp.ComSAP2000   => "SAP2000",
        HostApp.ComETABS     => "ETABS",
        HostApp.ComCSiBridge => "CSiBridge",
        _ => "Host"
    };

    // ── .csproj ───────────────────────────────────────────────────────────
    public const string CsProj = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>WinExe</OutputType>
            <RootNamespace>{{NAMESPACE}}</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}</AssemblyName>
            <TargetFramework>net48</TargetFramework>
            <UseWPF>true</UseWPF>
          </PropertyGroup>
        {{COM_REFERENCES}}
        </Project>
        """;

    // ── App.xaml ──────────────────────────────────────────────────────────
    public const string AppXaml = """
        <Application x:Class="{{NAMESPACE}}.App"
                     xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                     StartupUri="MainWindow.xaml">
        </Application>
        """;

    // ── App.xaml.cs ───────────────────────────────────────────────────────
    public const string AppCodeBehind = """
        using System.Windows;

        namespace {{NAMESPACE}};

        public partial class App : Application { }
        """;

    // ── MainWindow.xaml ───────────────────────────────────────────────────
    public const string MainWindowXaml = """
        <Window x:Class="{{NAMESPACE}}.MainWindow"
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

    // ── MainWindow.xaml.cs ────────────────────────────────────────────────
    public const string MainWindowCodeBehind = """
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

    // ── HostConnection.cs ─────────────────────────────────────────────────
    public const string HostConnection = """
        using System.Runtime.InteropServices;
        {{CSI_USING_LINE}}
        namespace {{NAMESPACE}};

        /// <summary>
        /// Connects to a running instance of {{HOST_TYPE_NAME}} via COM automation.
        /// The host application must already be open.
        /// </summary>
        public static class HostConnection
        {
            public static dynamic Connect()
            {
                try
                {
                    return {{GET_ACTIVE_OBJECT_CALL}};
                }
                catch (COMException ex)
                {
                    throw new InvalidOperationException(
                        "Could not connect to {{HOST_TYPE_NAME}}. Make sure it is running and COM automation is enabled.",
                        ex);
                }
            }
        }
        """;

    // ── MainWindowViewModel.cs ────────────────────────────────────────────
    public const string MainWindowViewModel = """
        using System.Windows.Input;
        using {{NAMESPACE}}.Common;

        namespace {{NAMESPACE}};

        public class MainWindowViewModel : ViewModelBase
        {
            private string _connectionStatus = "Not connected";
            public string ConnectionStatus
            {
                get => _connectionStatus;
                set => SetField(ref _connectionStatus, value);
            }

            public RelayCommand RunCommand { get; }

            private dynamic? _hostApp;

            public MainWindowViewModel()
            {
                RunCommand = new RelayCommand(OnRun, () => _hostApp is not null);
                Task.Run(Connect);
            }

            private void Connect()
            {
                try
                {
                    _hostApp = HostConnection.Connect();
                    ConnectionStatus = "Connected to {{HOST_TYPE_NAME}}";
                    System.Windows.Threading.Dispatcher.CurrentDispatcher
                        .Invoke(CommandManager.InvalidateRequerySuggested);
                }
                catch (Exception ex)
                {
                    ConnectionStatus = $"Error: {ex.Message}";
                }
            }

            private void OnRun()
            {
                ConnectionStatus = "Running...";
                try
                {
                    // TODO: use HostConnection to interact with the running application
                }
                catch (Exception ex)
                {
                    ConnectionStatus = $"Error: {ex.Message}";
                    System.Windows.MessageBox.Show(ex.Message, "{{PROJECT_NAME}}", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        """;
}
