---
inclusion: fileMatch
fileMatchPattern: "**/*.xaml"
---
# .NET MAUI (Android) XAML gotchas

- Use `x:DataType` for compiled bindings on every page and DataTemplate. Match the
  existing ViewModel type exactly; do not rename anything.
- Put colors, brushes, and styles in `Resources/Styles`; reference with
  `{StaticResource}` or `{AppThemeBinding Light=..., Dark=...}`.
- Implicit styles go in `Styles.xaml` (e.g., Label, Button, Entry, Border); use
  explicit `Style="{StaticResource X}"` for variants.
- Remove the Android Entry/Editor underline via a handler mapping in `MauiProgram` or
  `Platforms/Android` ONLY if needed, and only as pure styling.
- Status bar and navigation bar colors: set in `Platforms/Android` theme or with
  `CommunityToolkit.Maui` `StatusBarBehavior` (visual only; add the package only if
  I approve).
- `CollectionView` perf: keep ItemTemplate shallow, set `ItemSizingStrategy="MeasureFirstItem"`
  when items are uniform, avoid `Frame`, avoid images without fixed sizes.
- Images: set `Aspect="AspectFill"` plus a fixed container; use `FallbackValue` /
  placeholder for missing covers.
- Press feedback: use `VisualStateManager` `Pressed` state with Opacity/Scale
  or a ripple-like background change.
- WebView (EPUB): style the host page container; do not change injected scripts or
  content loading logic.
- Don't use `Frame`, `ListView`, `OnPlatform` for iOS values, or obsolete layouts
  (`AbsoluteLayout` unless truly needed).
