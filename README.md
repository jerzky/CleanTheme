# CleanTheme

A light, neutral and dense WPF theme. One accent (navy) for primary actions; green, amber and red only for status.
Templates stay minimal: controls are restyled with setters wherever the stock template allows it.

Originally the Strato Launcher theme.

| Package | What |
|---|---|
| `CleanTheme.Wpf` | The theme (palette, styles for stock controls, scroll bars) and a few controls: `Card`, `StatusPill`, `MetricTile`, `SegmentedControl` |
| `CleanTheme.Logging` | In-app log: an `ILoggerProvider` feeding a bounded, filterable `LogFeed`, and a `LogView` to show it |

## Install

The packages are published to GitHub Packages (`https://nuget.pkg.github.com/jerzky/index.json`), not nuget.org.

```xml
<PackageReference Include="CleanTheme.Wpf" Version="0.3.0" />
<PackageReference Include="CleanTheme.Logging" Version="0.3.0" />
```

## Use the theme

Merge the theme into your application resources:

```xml
<Application xmlns:clean="https://github.com/jerzky/CleanTheme" ...>
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

The `https://github.com/jerzky/CleanTheme` XML namespace covers both packages.

## Controls

```xml
<clean:Card Header="Nodes">
    <clean:Card.Actions>
        <Button Content="Clear" Style="{StaticResource GhostButton}" />
    </clean:Card.Actions>
    ...
</clean:Card>

<clean:StatusPill State="Positive" Content="Server running" />   <!-- Neutral, Info, Positive, Warning, Negative -->

<clean:MetricTile Label="Tick" Value="{Binding TickMs}" Unit="ms" />

<clean:SegmentedControl ItemsSource="{Binding TimeScales}" SelectedItem="{Binding TimeScale}" />
```

`SegmentedControl` is a single-selection `ListBox`; declare literal items as `clean:SegmentedItem`.

The controls carry their own default styles (with the palette), so they look right even before the theme is merged.

## Log

```csharp
// In [STAThread] Main, while building the host: the feed belongs to the UI thread's dispatcher.
builder.Logging.AddLogFeed();

// Anywhere, any thread:
logger.LogWarning("Tick took {Ms} ms", 18);
feed.Post(new LogEntry(DateTime.UtcNow, LogLevel.Information, Source: "UAV-1", Message: "Process started"));
```

```xml
<clean:LogView Feed="{Binding Log}" />
```

- `LogFeed` keeps the newest `Capacity` lines (default 2000). `SourceFilter`, `MinimumLevel` and `TextFilter` hide
  lines without dropping them; `Sources` lists the sources seen; `ResetFilters()` clears all three. It raises
  `PropertyChanged`, so filters can be bound. `Post` is thread-safe and batches onto the dispatcher; everything else is
  UI-thread only.
- `LogView` colors the level, virtualizes, follows new lines while scrolled to the bottom, and holds still once you
  scroll up. Its top bar shows the line count ("120 of 340 lines" while filtered) and a **Filter** popout with level,
  source and message-text filters; the button turns navy while a filter is on. Hide the bar with `ShowFilterBar="False"`.

## What's in the theme

| Kind | Keys |
|---|---|
| Surfaces | `AppBackground`, `Surface`, `SurfaceSunken`, `Hover`, `Pressed`, `Border`, `BorderStrong`, `Divider` |
| Text | `Text`, `TextMuted`, `TextSubtle`, `TextLog`, `UiFont`, `MonoFont` |
| Accent | `Accent`, `AccentHover`, `AccentPressed`, `AccentSoft` |
| Status | `Positive(Soft)`, `Warning(Soft)`, `Negative(Soft)`, `NegativeBorder`, `NeutralSoft` |
| Text styles | `CardTitle`, `FieldLabel`, `MetricLabel`, `MetricValue` |
| Containers | `CleanWindow`, `Card`, `Pill` (style keys for plain `Border`s; the controls above are the richer option) |
| Buttons | implicit `Button`, `PrimaryButton`, `DangerButton`, `GhostButton`, `RowButton`, `SegmentButton` (for `ToggleButton` / `RadioButton`, highlights on `IsChecked`) |
| Inputs | implicit `TextBox`, `ComboBox`, `ComboBoxItem`; `NumericTextBox` |
| Tables | implicit `DataGrid`, `DataGridColumnHeader`, `DataGridRow`, `DataGridCell`; `NumericHeader`, `NumericCell` |
| Scrolling | implicit `ScrollBar` (thin, rounded thumb with a minimum length, no arrows) and `ScrollViewer` |
| Log | `LogList` (monospace `ListBox`) |

Resource keys are public API: renaming or removing one is a breaking change.

## Develop

```bash
dotnet build CleanTheme.slnx
dotnet test CleanTheme.slnx
dotnet run --project samples/CleanTheme.Gallery
```

The gallery shows every style and control on one page, with a live log.

## Release

Versions come from git tags through MinVer. Pushing a tag such as `v0.3.0` runs `publish.yml`, which builds, tests,
packs and pushes both packages to GitHub Packages.
