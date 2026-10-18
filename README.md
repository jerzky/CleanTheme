# CleanTheme

A light, neutral and dense WPF theme. One accent (navy) for primary actions; green, amber and red only for status.
Templates stay minimal: controls are restyled with setters wherever the stock template allows it.

Originally the Strato Launcher theme.

## Install

The package is published to GitHub Packages (`https://nuget.pkg.github.com/jerzky/index.json`), not nuget.org.

```xml
<PackageReference Include="CleanTheme.Wpf" Version="0.1.0" />
```

## Use

Merge the theme into your application resources:

```xml
<Application xmlns:clean="clr-namespace:CleanTheme.Wpf;assembly=CleanTheme.Wpf" ...>
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <clean:CleanThemeResources />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

or by URI: `pack://application:,,,/CleanTheme.Wpf;component/Themes/CleanTheme.xaml` (also `CleanThemeResources.ThemeUri`).

Give windows the base look (implicit styles don't reach `Window` subclasses):

```xml
<Window ... Style="{StaticResource CleanWindow}">
```

## What's in it

| Kind | Keys |
|---|---|
| Surfaces | `AppBackground`, `Surface`, `SurfaceSunken`, `Hover`, `Pressed`, `Border`, `BorderStrong`, `Divider` |
| Text | `Text`, `TextMuted`, `TextSubtle`, `TextLog`, `UiFont`, `MonoFont` |
| Accent | `Accent`, `AccentHover`, `AccentPressed`, `AccentSoft` |
| Status | `Positive(Soft)`, `Warning(Soft)`, `Negative(Soft)`, `NegativeBorder`, `NeutralSoft` |
| Text styles | `CardTitle`, `FieldLabel`, `MetricLabel`, `MetricValue` |
| Containers | `CleanWindow`, `Card`, `Pill` (set `Background` / `TextElement.Foreground` per state) |
| Buttons | implicit `Button`, `PrimaryButton`, `DangerButton`, `GhostButton`, `RowButton`, `SegmentButton` (for `ToggleButton` / `RadioButton`, highlights on `IsChecked`) |
| Inputs | implicit `TextBox`, `ComboBox`, `ComboBoxItem`; `NumericTextBox` |
| Tables | implicit `DataGrid`, `DataGridColumnHeader`, `DataGridRow`, `DataGridCell`; `NumericHeader`, `NumericCell` |
| Log | `LogList` (monospace `ListBox`) |

Resource keys are public API: renaming or removing one is a breaking change.

## Develop

```bash
dotnet build CleanTheme.slnx
dotnet test CleanTheme.slnx
dotnet run --project samples/CleanTheme.Gallery
```

The gallery shows every style on one page.

## Release

Versions come from git tags through MinVer. Pushing a tag such as `v0.1.0` runs `publish.yml`, which builds, tests,
packs and pushes `CleanTheme.Wpf` to GitHub Packages.
