namespace DAR.Cli.Scaffolding;

/// <summary>
/// Shared template strings used across multiple scaffolders.
/// </summary>
public static class SharedTemplates
{
    // ── Directory.Build.props (generic — all project types) ──────────────
    public const string DirectoryBuildProps = """
        <Project>
          <PropertyGroup>
            <Platform>x64</Platform>
            <Platforms>x64</Platforms>
            <LangVersion>latest</LangVersion>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <EnableWindowsTargeting>true</EnableWindowsTargeting>
            <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
          </PropertyGroup>
        </Project>
        """;

    // ── Directory.Build.props (Revit) ─────────────────────────────────────
    public const string DirectoryBuildPropsRevit = """
        <Project>
          <PropertyGroup>
            <Platform>x64</Platform>
            <Platforms>x64</Platforms>
            <LangVersion>latest</LangVersion>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <EnableWindowsTargeting>true</EnableWindowsTargeting>
            <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
          </PropertyGroup>

          <!-- ── Revit install paths — Windows defaults, overridable ──────── -->
          <!-- Use $(ProgramW6432) — always C:\Program Files\ even in 32-bit MSBuild -->
          <PropertyGroup Condition="!$([MSBuild]::IsOSPlatform('Linux'))">
            <RevitDir2023>$(ProgramW6432)\Autodesk\Revit 2023</RevitDir2023>
            <RevitDir2024>$(ProgramW6432)\Autodesk\Revit 2024</RevitDir2024>
            <RevitDir2025>$(ProgramW6432)\Autodesk\Revit 2025</RevitDir2025>
            <RevitDir2026>$(ProgramW6432)\Autodesk\Revit 2026</RevitDir2026>
          </PropertyGroup>

          <!-- ── WSL / Linux path remapping ───────────────────────────────── -->
          <PropertyGroup Condition="$([MSBuild]::IsOSPlatform('Linux'))">
            <RevitDir2023>/mnt/c/Program Files/Autodesk/Revit 2023</RevitDir2023>
            <RevitDir2024>/mnt/c/Program Files/Autodesk/Revit 2024</RevitDir2024>
            <RevitDir2025>/mnt/c/Program Files/Autodesk/Revit 2025</RevitDir2025>
            <RevitDir2026>/mnt/c/Program Files/Autodesk/Revit 2026</RevitDir2026>
          </PropertyGroup>
        </Project>
        """;

    // ── Directory.Build.props (Civil 3D) ──────────────────────────────────
    public const string DirectoryBuildPropsCivil3D = """
        <Project>
          <PropertyGroup>
            <Platform>x64</Platform>
            <Platforms>x64</Platforms>
            <LangVersion>latest</LangVersion>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <EnableWindowsTargeting>true</EnableWindowsTargeting>
            <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
          </PropertyGroup>

          <!-- ── Civil 3D install paths — Windows defaults, overridable ───── -->
          <!-- Use $(ProgramW6432) — always C:\Program Files\ even in 32-bit MSBuild -->
          <PropertyGroup Condition="!$([MSBuild]::IsOSPlatform('Linux'))">
            <AcadDir2023>$(ProgramW6432)\Autodesk\AutoCAD 2023</AcadDir2023>
            <AcadDir2024>$(ProgramW6432)\Autodesk\AutoCAD 2024</AcadDir2024>
            <AcadDir2025>$(ProgramW6432)\Autodesk\AutoCAD 2025</AcadDir2025>
            <AcadDir2026>$(ProgramW6432)\Autodesk\AutoCAD 2026</AcadDir2026>
            <CivilDir2023>$(AcadDir2023)\C3D</CivilDir2023>
            <CivilDir2024>$(AcadDir2024)\C3D</CivilDir2024>
            <CivilDir2025>$(AcadDir2025)\C3D</CivilDir2025>
            <CivilDir2026>$(AcadDir2026)\C3D</CivilDir2026>
          </PropertyGroup>

          <!-- ── WSL / Linux path remapping ───────────────────────────────── -->
          <PropertyGroup Condition="$([MSBuild]::IsOSPlatform('Linux'))">
            <AcadDir2023>/mnt/c/Program Files/Autodesk/AutoCAD 2023</AcadDir2023>
            <AcadDir2024>/mnt/c/Program Files/Autodesk/AutoCAD 2024</AcadDir2024>
            <AcadDir2025>/mnt/c/Program Files/Autodesk/AutoCAD 2025</AcadDir2025>
            <AcadDir2026>/mnt/c/Program Files/Autodesk/AutoCAD 2026</AcadDir2026>
            <CivilDir2023>$(AcadDir2023)/C3D</CivilDir2023>
            <CivilDir2024>$(AcadDir2024)/C3D</CivilDir2024>
            <CivilDir2025>$(AcadDir2025)/C3D</CivilDir2025>
            <CivilDir2026>$(AcadDir2026)/C3D</CivilDir2026>
          </PropertyGroup>

          <!-- ── Dynamo package deploy paths (Windows MSBuild only) ────────── -->
          <!--   %AppData%\Autodesk\C3D {year}\Dynamo\{dynaVer}\packages\      -->
          <!--   Override in local Directory.Build.props if your versions differ -->
          <PropertyGroup Condition="'$(AppData)' != ''">
            <DynamoPkgRoot2023>$(AppData)\Autodesk\C3D 2023\Dynamo\2.15\packages</DynamoPkgRoot2023>
            <DynamoPkgRoot2024>$(AppData)\Autodesk\C3D 2024\Dynamo\2.19\packages</DynamoPkgRoot2024>
            <DynamoPkgRoot2025>$(AppData)\Autodesk\C3D 2025\Dynamo\3.3\packages</DynamoPkgRoot2025>
            <DynamoPkgRoot2026>$(AppData)\Autodesk\C3D 2026\Dynamo\3.4\packages</DynamoPkgRoot2026>
          </PropertyGroup>
        </Project>
        """;

    // ── Directory.Build.props (CSi) ───────────────────────────────────────
    public const string DirectoryBuildPropsCsi = """
        <Project>
          <PropertyGroup>
            <Platform>x64</Platform>
            <Platforms>x64</Platforms>
            <LangVersion>latest</LangVersion>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <EnableWindowsTargeting>true</EnableWindowsTargeting>
            <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
          </PropertyGroup>

          <!-- ── CSi install paths — Windows defaults, overridable ─────────── -->
          <!-- Use $(ProgramW6432) — always C:\Program Files\ even in 32-bit MSBuild -->
          <PropertyGroup Condition="!$([MSBuild]::IsOSPlatform('Linux'))">
            <CSiBridgeDir25>$(ProgramW6432)\Computers and Structures\CSiBridge 25</CSiBridgeDir25>
            <CSiBridgeDir26>$(ProgramW6432)\Computers and Structures\CSiBridge 26</CSiBridgeDir26>
            <SAP2000Dir25>$(ProgramW6432)\Computers and Structures\SAP2000 25</SAP2000Dir25>
            <SAP2000Dir26>$(ProgramW6432)\Computers and Structures\SAP2000 26</SAP2000Dir26>
            <ETABSDir21>$(ProgramW6432)\Computers and Structures\ETABS 21</ETABSDir21>
            <ETABSDir22>$(ProgramW6432)\Computers and Structures\ETABS 22</ETABSDir22>
          </PropertyGroup>

          <!-- ── WSL / Linux path remapping ───────────────────────────────── -->
          <PropertyGroup Condition="$([MSBuild]::IsOSPlatform('Linux'))">
            <CSiBridgeDir25>/mnt/c/Program Files/Computers and Structures/CSiBridge 25</CSiBridgeDir25>
            <CSiBridgeDir26>/mnt/c/Program Files/Computers and Structures/CSiBridge 26</CSiBridgeDir26>
            <SAP2000Dir25>/mnt/c/Program Files/Computers and Structures/SAP2000 25</SAP2000Dir25>
            <SAP2000Dir26>/mnt/c/Program Files/Computers and Structures/SAP2000 26</SAP2000Dir26>
            <ETABSDir21>/mnt/c/Program Files/Computers and Structures/ETABS 21</ETABSDir21>
            <ETABSDir22>/mnt/c/Program Files/Computers and Structures/ETABS 22</ETABSDir22>
          </PropertyGroup>
        </Project>
        """;

    // ── GitHub Actions CI workflow ────────────────────────────────────────
    // Token {{BUILD_MATRIX}} is replaced with per-project build config lines
    public const string GitHubActionsWorkflow = """
        name: Build

        on:
          push:
            branches: [ main, master ]
          pull_request:
            branches: [ main, master ]

        jobs:
          build:
            runs-on: windows-latest

            strategy:
              matrix:
                configuration: [{{BUILD_MATRIX}}]

            steps:
              - uses: actions/checkout@v4

              - name: Setup .NET
                uses: actions/setup-dotnet@v4
                with:
                  dotnet-version: |
                    8.x
                    9.x

              - name: Setup MSBuild
                uses: microsoft/setup-msbuild@v2

              - name: Restore
                run: dotnet restore

              - name: Build
                run: dotnet build --no-restore -c ${{ matrix.configuration }} -p:Platform=x64
        """;

    // ── .editorconfig ─────────────────────────────────────────────────────
    public const string EditorConfig = """
        root = true

        [*]
        charset                  = utf-8
        end_of_line              = crlf
        indent_style             = space
        indent_size              = 4
        trim_trailing_whitespace = true
        insert_final_newline     = true

        [*.{csproj,props,targets,xml,xaml}]
        indent_size = 2

        [*.{json,yml,yaml}]
        indent_size = 2

        [*.md]
        trim_trailing_whitespace = false
        """;

    // ── .gitignore ────────────────────────────────────────────────────────
    public const string GitIgnore = """
        ## Visual Studio
        .vs/
        bin/
        obj/
        *.user
        *.suo
        *.userosscache
        *.sln.docstates

        ## NuGet
        packages/
        *.nupkg
        project.lock.json
        project.fragment.lock.json
        artifacts/

        ## Rider
        .idea/

        ## OS
        .DS_Store
        Thumbs.db
        """;

    // ── ViewModelBase.cs ──────────────────────────────────────────────────
    public const string ViewModelBase = """
        using System.ComponentModel;
        using System.Runtime.CompilerServices;

        namespace {{NAMESPACE}}.Common;

        public abstract class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
        """;

    // ── RelayCommand.cs ───────────────────────────────────────────────────
    public const string RelayCommand = """
        using System.Windows.Input;

        namespace {{NAMESPACE}}.Common;

        public class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Func<object?, bool>? _canExecute;

            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute    = execute;
                _canExecute = canExecute;
            }

            public RelayCommand(Action execute, Func<bool>? canExecute = null)
                : this(_ => execute(), canExecute is null ? null : _ => canExecute()) { }

            public event EventHandler? CanExecuteChanged
            {
                add    => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
            public void Execute(object? parameter)    => _execute(parameter);

            public void RaiseCanExecuteChanged()
                => CommandManager.InvalidateRequerySuggested();
        }
        """;

    // ── CommonStyles.xaml ─────────────────────────────────────────────────
    public const string CommonStylesXaml = """
        <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

            <!-- ── Color Palette ──────────────────────────────────────────── -->
            <Color x:Key="BackgroundColor">#2B2B2B</Color>
            <Color x:Key="TitleBarColor">#33373C</Color>
            <Color x:Key="PrimaryActionColor">{{PRIMARY_COLOR}}</Color>
            <Color x:Key="DangerColor">{{DANGER_COLOR}}</Color>
            <Color x:Key="PrimaryTextColor">#E6E6E6</Color>
            <Color x:Key="InputBackgroundColor">#1E1E1E</Color>
            <Color x:Key="SecondaryButtonColor">#3A3A3A</Color>

            <SolidColorBrush x:Key="BackgroundBrush"       Color="{StaticResource BackgroundColor}"/>
            <SolidColorBrush x:Key="TitleBarBrush"         Color="{StaticResource TitleBarColor}"/>
            <SolidColorBrush x:Key="PrimaryActionBrush"    Color="{StaticResource PrimaryActionColor}"/>
            <SolidColorBrush x:Key="DangerBrush"           Color="{StaticResource DangerColor}"/>
            <SolidColorBrush x:Key="PrimaryTextBrush"      Color="{StaticResource PrimaryTextColor}"/>
            <SolidColorBrush x:Key="InputBackgroundBrush"  Color="{StaticResource InputBackgroundColor}"/>
            <SolidColorBrush x:Key="SecondaryButtonBrush"  Color="{StaticResource SecondaryButtonColor}"/>

            <!-- ── Base Window Style ─────────────────────────────────────── -->
            <Style x:Key="ToolWindowStyle" TargetType="Window">
                <Setter Property="WindowStyle"         Value="None"/>
                <Setter Property="AllowsTransparency"  Value="True"/>
                <Setter Property="Background"          Value="Transparent"/>
                <Setter Property="ResizeMode"          Value="NoResize"/>
                <Setter Property="UseLayoutRounding"   Value="True"/>
            </Style>

            <!-- ── Close Button ──────────────────────────────────────────── -->
            <Style x:Key="CloseButtonStyle" TargetType="Button">
                <Setter Property="Width"      Value="30"/>
                <Setter Property="Height"     Value="30"/>
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="Foreground" Value="{StaticResource DangerBrush}"/>
                <Setter Property="BorderThickness" Value="0"/>
                <Setter Property="FontSize"   Value="14"/>
                <Setter Property="Cursor"     Value="Hand"/>
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="Button">
                            <Border Background="{TemplateBinding Background}" CornerRadius="0,8,0,0">
                                <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            </Border>
                            <ControlTemplate.Triggers>
                                <Trigger Property="IsMouseOver" Value="True">
                                    <Setter Property="Background" Value="{StaticResource DangerBrush}"/>
                                    <Setter Property="Foreground" Value="White"/>
                                </Trigger>
                            </ControlTemplate.Triggers>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>

            <!-- ── Minimize Button ───────────────────────────────────────── -->
            <Style x:Key="MinimizeDashButtonStyle" TargetType="Button">
                <Setter Property="Width"      Value="30"/>
                <Setter Property="Height"     Value="30"/>
                <Setter Property="Background" Value="Transparent"/>
                <Setter Property="Foreground" Value="{StaticResource PrimaryActionBrush}"/>
                <Setter Property="BorderThickness" Value="0"/>
                <Setter Property="FontSize"   Value="16"/>
                <Setter Property="Cursor"     Value="Hand"/>
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="Button">
                            <Border Background="{TemplateBinding Background}">
                                <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            </Border>
                            <ControlTemplate.Triggers>
                                <Trigger Property="IsMouseOver" Value="True">
                                    <Setter Property="Background" Value="#44FFFFFF"/>
                                </Trigger>
                            </ControlTemplate.Triggers>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>

            <!-- ── Run / Primary Button ──────────────────────────────────── -->
            <Style x:Key="RunButtonStyle" TargetType="Button">
                <Setter Property="Background"       Value="{StaticResource PrimaryActionBrush}"/>
                <Setter Property="Foreground"       Value="#1A1A1A"/>
                <Setter Property="BorderThickness"  Value="0"/>
                <Setter Property="Height"           Value="34"/>
                <Setter Property="FontWeight"       Value="SemiBold"/>
                <Setter Property="Cursor"           Value="Hand"/>
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="Button">
                            <Border Background="{TemplateBinding Background}" CornerRadius="4">
                                <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            </Border>
                            <ControlTemplate.Triggers>
                                <Trigger Property="IsMouseOver" Value="True">
                                    <Setter Property="Background" Value="#FF5BE8D4"/>
                                </Trigger>
                                <Trigger Property="IsEnabled" Value="False">
                                    <Setter Property="Background" Value="#44AAAAAA"/>
                                    <Setter Property="Foreground" Value="#88888888"/>
                                </Trigger>
                            </ControlTemplate.Triggers>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>

            <!-- ── Secondary / Cancel Button ─────────────────────────────── -->
            <Style x:Key="SelectionButtonStyle" TargetType="Button">
                <Setter Property="Background"       Value="{StaticResource SecondaryButtonBrush}"/>
                <Setter Property="Foreground"       Value="{StaticResource PrimaryTextBrush}"/>
                <Setter Property="BorderThickness"  Value="0"/>
                <Setter Property="Height"           Value="34"/>
                <Setter Property="Cursor"           Value="Hand"/>
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="Button">
                            <Border Background="{TemplateBinding Background}" CornerRadius="4">
                                <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                            </Border>
                            <ControlTemplate.Triggers>
                                <Trigger Property="IsMouseOver" Value="True">
                                    <Setter Property="Background" Value="#4A4A4A"/>
                                </Trigger>
                            </ControlTemplate.Triggers>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>

            <!-- ── TextBox ────────────────────────────────────────────────── -->
            <Style TargetType="TextBox">
                <Setter Property="Background"    Value="{StaticResource InputBackgroundBrush}"/>
                <Setter Property="Foreground"    Value="{StaticResource PrimaryTextBrush}"/>
                <Setter Property="BorderBrush"   Value="#444444"/>
                <Setter Property="BorderThickness" Value="1"/>
                <Setter Property="Padding"       Value="6,4"/>
                <Setter Property="Height"        Value="30"/>
            </Style>

            <!-- ── ComboBox ───────────────────────────────────────────────── -->
            <Style TargetType="ComboBox">
                <Setter Property="Background"  Value="{StaticResource InputBackgroundBrush}"/>
                <Setter Property="Foreground"  Value="{StaticResource PrimaryTextBrush}"/>
                <Setter Property="BorderBrush" Value="#444444"/>
                <Setter Property="Height"      Value="30"/>
            </Style>

            <!-- ── Label ─────────────────────────────────────────────────── -->
            <Style TargetType="Label">
                <Setter Property="Foreground" Value="{StaticResource PrimaryTextBrush}"/>
                <Setter Property="Padding"    Value="0,4"/>
            </Style>

            <!-- ── CheckBox ───────────────────────────────────────────────── -->
            <Style TargetType="CheckBox">
                <Setter Property="Foreground" Value="{StaticResource PrimaryTextBrush}"/>
            </Style>

        </ResourceDictionary>
        """;

    // ── PluginLogger.cs ───────────────────────────────────────────────────
    public const string PluginLogger = """
        using Serilog;

        namespace {{NAMESPACE}};

        internal static class PluginLogger
        {
            internal static void Initialize(string vendorId, string pluginName)
            {
                var logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    vendorId, pluginName, "logs");

                Directory.CreateDirectory(logDir);

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(
                        path: Path.Combine(logDir, "log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
            }

            internal static void CloseAndFlush() => Log.CloseAndFlush();
        }
        """;

    // ── StaWindowLauncher.cs ──────────────────────────────────────────────
    public const string StaWindowLauncher = """
        using System.Threading;
        using System.Windows;

        namespace {{NAMESPACE}};

        /// <summary>
        /// Launches a WPF window on a dedicated STA thread so the host application
        /// (Revit or Civil 3D) remains fully interactive.
        /// </summary>
        public static class StaWindowLauncher
        {
            public static void Launch<TWindow>(Func<TWindow> windowFactory)
                where TWindow : Window
            {
                var thread = new Thread(() =>
                {
                    var window = windowFactory();
                    window.Closed += (_, _) =>
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                    window.Show();
                    System.Windows.Threading.Dispatcher.Run();
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Name = $"{typeof(TWindow).Name}_Thread";
                thread.Start();
            }
        }
        """;
}
