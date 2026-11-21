---
inclusion: always
---
# UI/UX guidelines (Android, Material 3 feel)

## Design tokens (define once in Colors.xaml / Styles.xaml, reuse everywhere)
- **Colors:** Primary, OnPrimary, Surface, SurfaceVariant, Background, OnSurface,
  OnSurfaceMuted, Outline, Error, Success. Every one needs a Light and Dark value via
  `AppThemeBinding`. Never hard-code hex values inside pages.
- **Reader themes:** Light, Dark, and Sepia are separate from the app theme; keep their
  colors in their own named resources.
- **Spacing scale:** 4, 8, 12, 16, 24, 32. No other values.
- **Corner radius:** 8 (chips, inputs), 12 (cards), 16 (sheets/dialogs), full-round for
  avatars and FABs.
- **Type scale (sp):** Display 28, Title 22, Heading 18, Body 16, Label 14, Caption 12.
  One font family for UI, optionally a serif for book titles and review text.
- **Elevation:** use subtle borders or tonal surfaces over heavy shadows.

## Layout and ergonomics
- Minimum touch target **48x48dp**, 8dp between adjacent targets.
- Screen horizontal padding 16. Respect safe areas / edge-to-edge insets.
- Primary actions reachable by thumb: bottom bar, FAB, or bottom sheet rather than top-right.
- Prefer `Grid` and `VerticalStackLayout`. Avoid deep nesting; avoid nested scroll views.
- Use `CollectionView` (never `ListView`) for lists. Use `Border` instead of `Frame`.

## Component patterns
- **Library grid:** 2-3 columns, 2:3 cover aspect ratio, rounded covers, thin progress
  bar under the cover, title max 2 lines, author in muted color. Placeholder cover for
  missing art (styled, with title text).
- **Cards (reviews, books):** `Border` with surface color, radius 12, padding 16.
- **Ratings:** star row with consistent size and gold accent; show numeric value beside it.
- **Reviews:** avatar, name, relative time, rating, text with a "Read more" collapse
  look, spoiler text visually blurred or covered (visual treatment only).
- **Comments:** indented, lighter surface, small avatar.
- **Buttons:** one filled primary, tonal secondary, text tertiary. Consistent height 48.
- **Inputs:** outlined, 12 radius, clear focus color, no default Android underline.
- **Empty states:** `EmptyView` with icon, one-line explanation, calm tone.
- **Loading:** `ActivityIndicator` or skeleton placeholders in the template; no layout jump.
- **Tabs / nav:** Shell bottom TabBar with clear selected state; 3-5 tabs max.
- **Dialogs/sheets:** bottom sheet look for pickers and actions where it is visual-only.

## Reader screens (PDF/EPUB)
- Reading surface gets maximum space. Chrome (toolbar, progress) is minimal and
  visually dismissible via existing controls; do not change how it is toggled.
- Comfortable defaults for the visual shell: generous margins around content, themed
  background that matches the reader theme, readable control icons over any background.
- Settings panels (font size, theme): large touch targets, live visual preview where
  the existing bindings already allow it.

## Motion
- Subtle only: 150-250ms, ease-out. Fade/scale on press, gentle page transitions.
- Android ripple/pressed states on all tappable items (use VisualStateManager).
- Respect reduced-motion: keep animations optional and short.

## Accessibility
- Contrast at least 4.5:1 for text in both themes.
- `SemanticProperties.Description` / `Hint` on icon-only buttons and images.
- Layouts must survive 200% font scale without clipping.
- Do not rely on color alone to convey state.

## Quality bar
- Every screen must look correct in light AND dark mode, portrait and landscape,
  small (360dp) and large phones.
