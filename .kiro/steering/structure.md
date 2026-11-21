---
inclusion: always
---
# Project structure

## Solution / project
- Single-project: `PAGELY.csproj`
- Target framework: `net8.0-android` (Android API 26–34)
- MAUI version: 8.0.100
- Key packages: CommunityToolkit.Mvvm 8.3.2, sqlite-net-pcl, VersOne.Epub

## Pages / views

### Reader
- `Views/ReaderPage.xaml` — full-screen PDF/EPUB reader with floating top bar, bottom chapter nav, right-side bookmarks/notes drawer, and PDF mini-menu button
- `ViewModels/SavedPage.xaml` *(file is in ViewModels/ folder)* — standalone bookmarks + notes list (routed as "saved")

### Library
- `Views/LibraryPage.xaml` — 2-column grid of local books, FAB import button
- `Views/BookDetailPage.xaml` — local book detail: cover, progress, Read/Delete actions

### Social / Discover
- `Views/DiscoverPage.xaml` — community catalog: genre chip filters, sort, book list
- `Views/DiscoverBookDetailPage.xaml` — community book detail: description, reading-list toggle, review composer, review list with likes
- `Views/SearchPage.xaml` — dual-scope search (My Library / Community) with filter panel
- `Views/ReadingListPage.xaml` — saved-to-read list (community books)
- `Views/MyReviewsPage.xaml` — user's own reviews

### Profile / Stats
- `Views/ProfilePage.xaml` — 4-tab page (Profile, Reading List, Reading History, Settings) using CarouselView; also hosts History and Settings content via DataTemplates
- `Views/HistoryPage.xaml` — standalone history page (also embedded inside ProfilePage template)
- `Views/StatsPage.xaml` — reading statistics: streak, book counts, activity heatmap

### Auth (embedded in ProfilePage)
- Login and register forms live inside `ProfilePage` as part of the Profile DataTemplate; no separate auth page

### Settings
- `SettingsPage.xaml` *(root folder)* — reader settings: font size, line spacing, theme, font, API URL
- Settings content also duplicated as a DataTemplate inside `ProfilePage`

## Shell / navigation
- `AppShell.xaml` — Shell with TabBar (4 tabs: Library, Discover, Search, Profile)
- ReaderPage and BookDetailPage registered as full-screen routes (TabBar hidden)
- SavedPage registered as routed sub-page ("saved")

## Shared resources
- `Resources/Styles/Colors.xaml` — all color tokens and SolidColorBrush aliases (light-only, no AppThemeBinding)
- `Resources/Styles/Styles.xaml` — implicit + named styles for all controls
- `App.xaml` — merges Colors + Styles; registers all Converters

## Fonts
- OpenSans-Regular.ttf → `OpenSansRegular`
- OpenSans-Semibold.ttf → `OpenSansSemibold`
- No serif font registered

## Icons / images
- Tab icons: tab_library, tab_discover, tab_search, tab_profile, tab_history, tab_settings, tab_stats (SVG)
- UI icons: ic_back.svg, heart_outline.svg, heart_filled.svg

## Controls / converters
- `Controls/PdfViewer.cs` — custom PDF viewer control
- `Converters/ValueConverters.cs` — all converters in one file: HasValue, InverseHasValue, InverseBool, StatusColor, PercentToProgress, HasIntValue, RatingStarColor, HasText, HasObject, InverseHasText, HasProgress

## Platform (Android)
- `Platforms/Android/MainActivity.cs` — sets status bar to Paper (#FBF6EA) with light icons
- `Platforms/Android/Resources/values/colors.xml` — colorPrimary, colorPrimaryDark, colorAccent (no styles.xml)
