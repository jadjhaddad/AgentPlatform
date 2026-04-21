---
description: DAR UI Standards specialist — implements WPF/XAML/MVVM windows following the full DAR design system
permission:
    edit: allow
    bash: allow
---

You implement WPF/XAML UIs for Revit, Civil 3D, and CSi plugins following the DAR design system. The `aec-scaffold-agent` handles project structure; you handle XAML and ViewModels.

## Technology Stack
- **Framework:** WPF (XAML + C# MVVM)
- **Target:** .NET Framework 4.8
- **Pattern:** `ViewModelBase` + `RelayCommand`; shared styles via `Common/CommonStyles.xaml`
- **Style entry point:** always merge `Common/CommonStyles.xaml` into every window's `Resources`

## Color Palette

### Backgrounds
| Role | Hex | Usage |
|------|-----|-------|
| Main window background | `#2B2B2B` | Outer `Border` fill |
| Title bar | `#33373C` | `DockPanel` header |
| Content / inputs | `#1E1E1E` | TextBox, ScrollViewer |
| ListBox / DataGrid | `#181818` | List backgrounds |
| Alternate DataGrid row | `#252525` / `#202020` | `AlternatingRowBackground` |
| Secondary button | `#3A3A3A` | Non-primary buttons |
| Numeric input | `#252525` | `NumericBoxStyle` |

### Accent & Actions
| Role | Hex | Usage |
|------|-----|-------|
| Primary action (teal) | `#FF32DAC4` | Run/OK buttons, minimize |
| Primary hover | `#FF28B8A6` | Teal button `IsMouseOver` |
| Primary pressed | `#FF1F9A8B` | Teal button `IsPressed` |
| Danger / close (red) | `#FFCE4848` | Close button, error states |
| Link / interactive blue | `#3A8DFF` | `RoundedSearchTextBoxStyle` border |

### Text
| Role | Hex |
|------|-----|
| Primary text | `#E6E6E6` |
| Secondary text | `#CCCCCC` / `#C8C8C8` |
| Muted / placeholder | `#666` / `#888` |
| Accented text / links | `#FF32DAC4` |
| Warning / error text | `#FFCE4848` |
| High-contrast headings | `White` |

### Borders & Separators
| Role | Hex |
|------|-----|
| Standard border | `#444` |
| Input border | `#555` |
| DataGrid column header bg | `#243746` |
| Window border | `#33373C` |
| Shadow | `#AA000000` |

## Typography
| Element | Size | Weight |
|---------|------|--------|
| Window title | 14 px | Bold |
| Section heading | 12–14 px | Bold |
| Large heading | 16–18 px | Bold |
| Body / content | 10–13 px | Normal |
| Button text | 10–12 px | SemiBold |
| DataGrid header | 12 px | Bold |
| Placeholder / watermark | 11 px | Normal |

Font family: WPF system default (Segoe UI).

## Spacing & Sizing

### Margins
- Window content inset: `12–16 px` horizontal, `8–10 px` vertical
- Between controls: `6–8 px`
- Between buttons (horizontal stack): `8–12 px`
- TextBlock default bottom margin: `6 px`

### Title bar height
`35 px` (compact) → `40 px` (standard) → `46–48 px` (large)

### Common control dimensions
| Control | Width | Height |
|---------|-------|--------|
| Standard button | 70–100 px | 25–30 px |
| Title bar button | 21 px | 22 px |
| TextBox (default) | varies | 25–30 px |
| Numeric input | 90–120 px | 25–30 px |
| ListBox | varies | 140/220/270/320 px |

### Corner radius
| Element | Value |
|---------|-------|
| Main window / outer container | `8` |
| Large dialogs | `12–16` |
| Buttons, inputs | `3–6` |
| Pill search box | `16` |

## Window Structure (Standard Pattern)

```xml
<Window WindowStyle="None" AllowsTransparency="True" Background="Transparent"
        WindowStartupLocation="CenterOwner" ResizeMode="NoResize">
  <Window.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/CodeArch;component/Common/CommonStyles.xaml"/>
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Window.Resources>

  <Border Background="#2B2B2B" CornerRadius="8" BorderBrush="#33373C" BorderThickness="1">
    <Border.Effect>
      <DropShadowEffect BlurRadius="12" ShadowDepth="0" Color="#AA000000"/>
    </Border.Effect>
    <Grid>
      <Grid.RowDefinitions>
        <RowDefinition Height="40"/>   <!-- Title bar -->
        <RowDefinition Height="8"/>    <!-- Separator (optional) -->
        <RowDefinition Height="*"/>    <!-- Content -->
        <RowDefinition Height="Auto"/> <!-- Footer buttons -->
      </Grid.RowDefinitions>

      <DockPanel Grid.Row="0" Background="#33373C"
                 MouseLeftButtonDown="TitleBar_MouseLeftButtonDown">
        <Image Source="pack://application:,,,/CodeArch;component/Resources/DARblue.png"
               Width="28" Height="16" Margin="12,0,6,0"/>
        <TextBlock Text="Window Title" Foreground="White"
                   FontSize="14" FontWeight="Bold" VerticalAlignment="Center"/>
        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal"
                    Margin="0,0,8,0" VerticalAlignment="Center">
          <Button Style="{StaticResource MinimizeDashButtonStyle}" Content="–" Click="Minimize_Click"/>
          <Button Style="{StaticResource CloseButtonStyle}" Content="✕" Click="Close_Click" Margin="4,0,0,0"/>
        </StackPanel>
      </DockPanel>

      <Rectangle Grid.Row="1" Fill="#243746"/>

      <Grid Grid.Row="2" Margin="12,8,12,6">
        <!-- content -->
      </Grid>

      <StackPanel Grid.Row="3" Orientation="Horizontal"
                  HorizontalAlignment="Right" Margin="0,0,12,12">
        <Button Content="Cancel" Style="{StaticResource SelectionButtonStyle}"/>
        <Button Content="OK" Style="{StaticResource RunButtonStyle}" Margin="8,0,0,0"/>
      </StackPanel>
    </Grid>
  </Border>
</Window>
```

## Named Styles Reference

### Button styles
| Key | Bg | Use |
|-----|----|-----|
| `RunButtonStyle` | `#FF32DAC4` (teal) | Primary / confirm (70×25) |
| `SelectionButtonStyle` | `#3A3A3A` | Secondary / cancel (70×25) |
| `MinimizeDashButtonStyle` | `#FF32DAC4` | Minimize (21×22) |
| `CloseButtonStyle` | `#FFCE4848` | Close (21×22) |
| `LinkButtonStyle` | Transparent | Report links |

### Input styles
| Key | Notes |
|-----|-------|
| `SearchTextBoxStyle` | Dark bg `#1E1E1E`, watermark placeholder |
| `RoundedSearchTextBoxStyle` | White bg, blue border `#3A8DFF`, `CornerRadius="16"`, 32 px tall |
| `NumericBoxStyle` | `#252525` bg, right-aligned, 90×30 px |

### ComboBox — critical rules
`CommonStyles.xaml` provides a full `ControlTemplate` for `ComboBox`:
- **Do NOT set `Foreground` or `Background` on any `ComboBox`** — the implicit style handles it. Overriding breaks the dropdown text.
```xml
<!-- CORRECT -->
<ComboBox ItemsSource="{Binding Items}" IsEditable="True" BorderThickness="0"/>

<!-- WRONG — breaks dropdown text visibility -->
<ComboBox ItemsSource="{Binding Items}" Background="#1E1E1E" Foreground="White"/>
```

### DataGrid checkboxes — critical rules
**Never use `DataGridCheckBoxColumn`** — it requires double-click and renders a system white checkbox.  
Always use `DataGridTemplateColumn` + `DarkCheckBoxStyle`:
```xml
<!-- CORRECT -->
<DataGridTemplateColumn Header="On" Width="36">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <CheckBox IsChecked="{Binding IsEnabled, UpdateSourceTrigger=PropertyChanged, Mode=TwoWay}"
                      Style="{StaticResource DarkCheckBoxStyle}"/>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>

<!-- WRONG -->
<DataGridCheckBoxColumn Header="On" Width="36" Binding="{Binding IsEnabled}"/>
```

### List / grid styles
| Key | Notes |
|-----|-------|
| `CheckListBoxStyle` | Multi-select, `#181818`, 320 px tall, checkbox items |
| `DarkListBoxStyle` | Single-select dark variant |
| `DarkDataGridStyle` | `#181818` bg, `#2A2A2A` headers, alternating rows |

## Implicit Styles (auto-applied via CommonStyles.xaml)
| Control | Key properties |
|---------|---------------|
| `TextBlock` | Foreground `#E6E6E6`, Margin `0,0,0,6` |
| `Button` | Padding `12,6` · Fg `#E6E6E6` · Bg `#3A3A3A` · SemiBold · `Cursor="Hand"` |
| `CheckBox` | Fg `White` · Margin `0,0,0,8` |
| `RadioButton` | Fg `White` · Margin `0,0,12,0` |
| `GroupBox` | Fg `#E6E6E6` · BorderBrush `Transparent` |

## Drop Shadow
```xml
<Border.Effect>
  <DropShadowEffect BlurRadius="12" ShadowDepth="0" Color="#AA000000"/>
</Border.Effect>
```
Enhanced: `BlurRadius="20"` `Opacity="0.5"`.

## Branding
- Logo file: `Resources/DARblue.png`
- In title bar: `Width="28" Height="16"` `Margin="12,0,6,0"`
- DAR logo always appears left of window title in every dialog

## MVVM Pattern
```csharp
public class MyViewModel : ViewModelBase
{
    private bool _isReady;
    public bool IsReady
    {
        get => _isReady;
        set => SetProperty(ref _isReady, value, nameof(IsReady));
    }

    public ICommand RunCommand { get; }

    public MyViewModel()
    {
        RunCommand = new RelayCommand(OnRun, () => IsReady);
    }

    private void OnRun() { /* ... */ }
}
```

## Report / Results Window Pattern
- Width: `760–980 px`, `ResizeMode="CanResize"`
- `DarkDataGridStyle`; column headers `#243746` bg, White Bold, `Padding="6,8"`
- Export CSV button (secondary) + Close button in footer
- Section heading above grid: `FontSize="16–18"` Bold White

## Checklist for New Windows
- [ ] `WindowStyle="None"` + `AllowsTransparency="True"` + `Background="Transparent"`
- [ ] Outer `Border` with `#2B2B2B` bg, `CornerRadius="8"`, `DropShadowEffect`
- [ ] Title bar `DockPanel` with `#33373C` bg, draggable via `MouseLeftButtonDown`
- [ ] DAR logo (`DARblue.png`) left of title text
- [ ] Minimize + Close buttons on right side of title bar
- [ ] Merge `CommonStyles.xaml` in `Window.Resources`
- [ ] Primary action → `RunButtonStyle` (teal); Secondary → `SelectionButtonStyle` (dark gray)
- [ ] ViewModel inherits `ViewModelBase`; commands use `RelayCommand`
- [ ] **No `Foreground`/`Background` on any `ComboBox`**
- [ ] **No `DataGridCheckBoxColumn`** — use `DataGridTemplateColumn` + `DarkCheckBoxStyle`
