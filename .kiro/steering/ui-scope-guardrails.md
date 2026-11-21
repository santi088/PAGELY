---
inclusion: always
---
# Scope guardrails (read before every change)

## You MAY change
- `.xaml` files: layout, styling, templates, visual states, animations
- `Resources/Styles/Colors.xaml`, `Styles.xaml`, and new resource dictionaries
- `Resources/Fonts`, `Resources/Images`, `Resources/Raw` (visual assets)
- Code-behind (`*.xaml.cs`) ONLY for purely visual work (e.g., animation triggers)
- `Platforms/Android` styling: themes, `styles.xml`, `colors.xml`, status/nav bar setup
- Adding converters or behaviors that are purely presentational

## You MUST NOT change
- ViewModels, Services, Repositories, Models, DTOs, database code, API calls,
  sync/queue code, navigation routes, permissions logic, or DI registrations
- Binding paths, command names, or property names that the ViewModels expose
- Any behavior: what a button does, what gets saved, when data loads

## When a UI idea needs logic
Do NOT implement it. Add it to a list titled "Needs logic change (not done)" in your
summary and ask me. Examples: new loading flags, empty-state flags, sorting, new
settings.

## Safety rules
1. Before editing a screen, list the bindings it uses. After editing, confirm every
   one still exists and is unchanged.
2. Work ONE screen or ONE shared resource at a time. Run `dotnet build -f net9.0-android`
   (use the project's actual target framework) after each and fix errors before moving on.
3. Prefer editing shared styles over per-page hacks.
4. Never delete a control that is wired to a command or used by code-behind.
5. If a file's purpose is unclear, read it first. Don't guess.
6. Summarize each step: files touched, what visually changed, confirmation that no
   logic changed.
