---
inclusion: manual
---
# UI polish playbook (invoke with #ui-polish-playbook)

Run in this order. Stop after each step for my review.

1. **Audit:** read every XAML page and shared resource. Produce a report: current
   inconsistencies (colors, spacing, fonts, touch targets), hard-coded values, missing
   dark-mode support, missing empty/loading visuals, accessibility gaps. No edits.
2. **Design system:** create/refactor `Colors.xaml` and `Styles.xaml` with the tokens
   from ui-ux-guidelines. Light + dark. Build and verify nothing else broke.
3. **Shell and navigation:** bottom tab bar, toolbar/title styling, status bar color.
4. **Library screen:** book grid, covers, progress bars, empty state, import button.
5. **Reader chrome:** PDF and EPUB toolbars, progress indicator, settings panel,
   TOC and bookmarks lists. Theme-matched backgrounds.
6. **Search and book detail:** results list, detail header, aggregate rating, actions.
7. **Reviews and comments:** review cards, rating stars, spoiler visual, comment list,
   composer inputs.
8. **Profile, feed, auth, settings:** consistent forms, buttons, avatars.
9. **Polish pass:** press states, subtle transitions, accessibility labels, 200% font
   check, light/dark and landscape check.
10. **Final report:** per-screen before/after summary, plus the "Needs logic change
    (not done)" list.
