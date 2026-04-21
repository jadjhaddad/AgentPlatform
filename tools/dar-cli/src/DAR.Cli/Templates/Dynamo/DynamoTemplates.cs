namespace DAR.Cli.Templates.Dynamo;

public static class DynamoTemplates
{
    // ── Version mapping ───────────────────────────────────────────────────
    // Maps Civil 3D version → Dynamo NuGet version + TargetFramework
    public static (string nuget, string tf) DynamoVersion(string c3dVersion) => c3dVersion switch
    {
        "2023" => ("2.16.1", "net48"),
        "2024" => ("2.19.4", "net48"),
        "2025" => ("3.1.0",  "net8.0-windows"),
        "2026" => ("3.4.0",  "net8.0-windows"),
        _      => ("2.19.4", "net48")
    };

    // ── .csproj ───────────────────────────────────────────────────────────
    public const string CsProj = """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <RootNamespace>{{NAMESPACE}}</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}</AssemblyName>
            <TargetFramework>net48</TargetFramework>
          </PropertyGroup>

        {{BUILD_CONFIGS}}

          <!-- ── AfterBuild: deploy to Dynamo packages folder ─────────────── -->
          <!--   %AppData%\Autodesk\C3D {year}\Dynamo\{dynaVer}\packages\Name\ -->
          <!--   DynamoPkgRoot20XX is defined in Directory.Build.props          -->
          <!--   Skipped automatically on Linux ($(AppData) is empty)           -->
          <Target Name="AfterBuild"
                  Condition="'$(AppData)' != '' And '$(DynamoPkgRootActive)' != ''">
            <MakeDir Directories="$(DynamoPkgRootActive)\bin"
                     Condition="!Exists('$(DynamoPkgRootActive)\bin')"/>
            <ItemGroup>
              <_DynFiles Include="$(OutputPath)*.dll"/>
              <_DynFiles Include="$(OutputPath)*.xml"/>
            </ItemGroup>
            <Copy SourceFiles="@(_DynFiles)"
                  DestinationFolder="$(DynamoPkgRootActive)\bin"
                  SkipUnchangedFiles="true"/>
            <Copy SourceFiles="$(MSBuildProjectDirectory)\pkg.json"
                  DestinationFiles="$(DynamoPkgRootActive)\pkg.json"
                  SkipUnchangedFiles="true"
                  Condition="Exists('$(MSBuildProjectDirectory)\pkg.json')"/>
            <Message Text="[Dynamo] Deployed → $(DynamoPkgRootActive)" Importance="high"/>
          </Target>

        </Project>
        """;

    public static string BuildConfigBlock(string version)
    {
        var (nuget, tf) = DynamoVersion(version);
        var major = nuget.Split('.')[0];

        // Build the DynamoPkgRootActive line separately to avoid brace-count issues
        // Use $(AssemblyName) so MSBuild resolves it — avoids template token ordering issues
        var pkgRootLine = $"<DynamoPkgRootActive>$(DynamoPkgRoot{version})\\$(AssemblyName)</DynamoPkgRootActive>";

        return $"""
              <PropertyGroup Condition="'$(Configuration)'=='C3D{version}'">
                <TargetFramework>{tf}</TargetFramework>
                <DefineConstants>C3D{version};DYNAMO{major}</DefineConstants>
                <OutputPath>bin\C3D{version}\</OutputPath>
                <DynamoMajorVersion>{major}.x</DynamoMajorVersion>
                <GenerateDocumentationFile>true</GenerateDocumentationFile>
                <DocumentationFile>bin\C3D{version}\$(AssemblyName).xml</DocumentationFile>
                {pkgRootLine}
              </PropertyGroup>

              <ItemGroup Condition="'$(Configuration)'=='C3D{version}'">
                <PackageReference Include="DynamoVisualProgramming.ZeroTouchLibrary" Version="{nuget}">
                  <ExcludeAssets>runtime</ExcludeAssets>
                </PackageReference>
                <PackageReference Include="DynamoVisualProgramming.DynamoServices" Version="{nuget}">
                  <ExcludeAssets>runtime</ExcludeAssets>
                </PackageReference>
              </ItemGroup>
            """;
    }

    // ── pkg.json ──────────────────────────────────────────────────────────
    public const string PkgJson = """
        {
          "license": "",
          "file_hash": null,
          "name": "{{PROJECT_NAME}}",
          "version": "1.0.0",
          "description": "{{PROJECT_NAME}} Dynamo nodes",
          "group": "",
          "keywords": [],
          "dependencies": [],
          "contents": "",
          "engine_version": "2.0.0",
          "engine": "dynamo",
          "engine_metadata": "",
          "site_url": "",
          "repository_url": "",
          "contains_binaries": true,
          "node_libraries": [
            {{NODE_LIBRARIES}}
          ]
        }
        """;

    public static string NodeLibraries(string projectName, bool hasUi) =>
        hasUi
            ? $"""
               "{projectName}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                "{projectName}.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
               """
            : $"\"{projectName}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"";

    // ── Node library class ────────────────────────────────────────────────
    public const string NodeLibrary = """
        namespace {{NAMESPACE}};

        /// <summary>
        /// {{PROJECT_NAME}} Dynamo zero-touch nodes.
        /// Public static methods in this class are automatically discovered by Dynamo.
        /// </summary>
        public static class {{PROJECT_NAME}}Nodes
        {
            /// <summary>
            /// Example node: adds two numbers.
            /// </summary>
            /// <param name="a">First number</param>
            /// <param name="b">Second number</param>
            /// <returns>Sum of a and b</returns>
            public static double Add(double a, double b) => a + b;

            // TODO: add your nodes here
        }
        """;

    // ── UI .csproj ────────────────────────────────────────────────────────
    public const string UICsProj = """
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <RootNamespace>{{NAMESPACE}}.UI</RootNamespace>
            <AssemblyName>{{PROJECT_NAME}}.UI</AssemblyName>
            <TargetFramework>net48</TargetFramework>
            <UseWPF>true</UseWPF>
          </PropertyGroup>

        {{BUILD_CONFIGS}}

          <ItemGroup>
            <ProjectReference Include="..\{{PROJECT_NAME}}\{{PROJECT_NAME}}.csproj"/>
          </ItemGroup>

          <!-- ── AfterBuild: deploy all DLLs to same Dynamo package bin\ ───── -->
          <!--   Picks up both the node DLL and the UI DLL                      -->
          <Target Name="AfterBuild"
                  Condition="'$(AppData)' != '' And '$(DynamoPkgRootActive)' != ''">
            <MakeDir Directories="$(DynamoPkgRootActive)\bin"
                     Condition="!Exists('$(DynamoPkgRootActive)\bin')"/>
            <ItemGroup>
              <_DynFiles Include="$(OutputPath)*.dll"/>
              <_DynFiles Include="$(OutputPath)*.xml"/>
            </ItemGroup>
            <Copy SourceFiles="@(_DynFiles)"
                  DestinationFolder="$(DynamoPkgRootActive)\bin"
                  SkipUnchangedFiles="true"/>
            <Message Text="[Dynamo] Deployed → $(DynamoPkgRootActive)" Importance="high"/>
          </Target>

        </Project>
        """;

    // ── Dialog node class ─────────────────────────────────────────────────
    public const string DialogNodes = """
        namespace {{NAMESPACE}}.UI;

        /// <summary>
        /// Dynamo nodes that open WPF dialogs.
        /// Public static methods are auto-discovered by Dynamo as zero-touch nodes.
        /// Pattern: one node creates/shows the dialog, another extracts the result.
        /// </summary>
        public static class {{PROJECT_NAME}}DialogNodes
        {
            /// <summary>
            /// Opens the {{PROJECT_NAME}} dialog and returns the result object.
            /// Connect the output of this node to Get{{PROJECT_NAME}}Result.
            /// </summary>
            /// <returns>Dialog result object</returns>
            public static {{PROJECT_NAME}}DialogResult Show{{PROJECT_NAME}}Dialog()
            {
                var result = new {{PROJECT_NAME}}DialogResult();

                var thread = new System.Threading.Thread(() =>
                {
                    var dialog = new {{PROJECT_NAME}}Dialog(result);
                    dialog.ShowDialog();
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                });
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();

                return result;
            }

            /// <summary>
            /// Extracts the value from the dialog result.
            /// </summary>
            /// <param name="result">The result from Show{{PROJECT_NAME}}Dialog</param>
            /// <returns>The selected value</returns>
            public static string GetResult({{PROJECT_NAME}}DialogResult result) => result.Value;
        }

        /// <summary>Carries the result from the dialog back to the graph.</summary>
        public class {{PROJECT_NAME}}DialogResult
        {
            public string Value { get; set; } = string.Empty;
        }
        """;

    // ── Dialog WPF window ─────────────────────────────────────────────────
    public const string DialogXaml = """
        <Window x:Class="{{NAMESPACE}}.UI.{{PROJECT_NAME}}Dialog"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                Title="{{PROJECT_NAME}}"
                Width="420" Height="300"
                WindowStyle="None"
                AllowsTransparency="True"
                Background="Transparent"
                WindowStartupLocation="CenterScreen">

            <Window.Resources>
                <ResourceDictionary Source="pack://application:,,,/{{PROJECT_NAME}}.UI;component/Common/CommonStyles.xaml"/>
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
                    <DockPanel Grid.Row="0" Background="#33373C" MouseLeftButtonDown="TitleBar_MouseLeftButtonDown">
                        <Button DockPanel.Dock="Right" Style="{StaticResource CloseButtonStyle}"
                                Content="✕" Click="CloseButton_Click"/>
                        <TextBlock Text="{{PROJECT_NAME}}" Foreground="#E6E6E6"
                                   VerticalAlignment="Center" Margin="12,0" FontSize="13"/>
                    </DockPanel>
                    <Grid Grid.Row="1" Margin="20">
                        <!-- TODO: dialog content -->
                    </Grid>
                    <DockPanel Grid.Row="2" Margin="20,0,20,16" LastChildFill="False">
                        <Button DockPanel.Dock="Right" Style="{StaticResource RunButtonStyle}"
                                Content="OK" Width="80" Click="OkButton_Click"/>
                        <Button DockPanel.Dock="Right" Style="{StaticResource SelectionButtonStyle}"
                                Content="Cancel" Width="80" Margin="0,0,8,0" Click="CloseButton_Click"/>
                    </DockPanel>
                </Grid>
            </Border>
        </Window>
        """;

    public const string DialogCodeBehind = """
        using System.Windows;
        using System.Windows.Input;

        namespace {{NAMESPACE}}.UI;

        public partial class {{PROJECT_NAME}}Dialog : Window
        {
            private readonly {{PROJECT_NAME}}DialogResult _result;

            public {{PROJECT_NAME}}Dialog({{PROJECT_NAME}}DialogResult result)
            {
                _result = result;
                InitializeComponent();
            }

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
            private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

            private void OkButton_Click(object sender, RoutedEventArgs e)
            {
                // TODO: populate _result.Value from the dialog inputs
                _result.Value = "ok";
                Close();
            }
        }
        """;
}
