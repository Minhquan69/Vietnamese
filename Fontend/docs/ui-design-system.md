# Frontend UI design system

This document defines the **reusable UI architecture** for the Vietnamese learning platform. Feature modules consume these primitives; they do not own global layout or tokens.

## 1) Reusable component structure

```
src/app/
  core/
    theme/
      theme.service.ts          # Light/dark, persistence, document data-theme
  layout/
    main-layout/                  # Router parent: sidebar + topbar + outlet
    shell-sidebar/                # Collapsible nav, role-filtered items
    shell-topbar/                 # Breadcrumb area, search slot, theme toggle, user menu
  shared/
    ui/
      button/                     # ui-button: variants + sizes
      input/                      # ui-input: CVA, label, hint, error
      modal/                      # ui-modal: portal-style overlay, focus trap lite
      card/                       # ui-card: elevated / glass / hover lift
      skeleton/                   # ui-skeleton: pulse placeholders
      progress-donut/             # circular progress for XP / goals
    pages/                        # (optional) cross-cutting pages only
      dashboard/                  # Premium hub (widgets only; no feature domain logic)
```

**Rules**

- **Dumb UI** in `shared/ui` (inputs/outputs only; no `HttpClient`).
- **Layout** owns shell chrome, breakpoints, and sidebar state.
- **Feature pages** stay under `features/pages/…` and compose `shared/ui` later.

## 2) SCSS architecture

```
src/styles/
  main.scss                 # Single entry (referenced from angular.json)
  abstracts/
    _functions.scss         # strip-unit, etc. (minimal)
    _mixins.scss            # focus-ring, glass, truncate
  tokens/
    _css-variables.scss     # Design tokens as CSS custom properties (:root + dark)
  base/
    _reset.scss             # Box sizing, reduced motion respect
    _typography.scss        # Font stacks, type scale, heading rhythm
  layout/
    _app-shell.scss         # Grid for sidebar + main; responsive collapse
  utilities/
    _motion.scss            # Transition presets, keyframes (float-y, shimmer)
```

**Component styles** use `*.component.scss` only for host-scoped layout hooks; **tokens live globally** so budgets stay predictable and themes stay consistent.

## 3) Theme architecture

- **Single source of truth:** CSS variables on `:root` (light) and `[data-theme="dark"]` (dark).
- **`ThemeService`** sets `document.documentElement.dataset.theme` and `localStorage` key `vlp-theme`.
- **Semantic tokens** (e.g. `--color-surface-elevated`, `--color-accent`) map to brand ramps—not raw hex in components.
- **Glassmorphism** via mixin: `backdrop-filter`, translucent `background-color`, subtle border using `--color-glass-border`.

## 4) Layout architecture

- **Router tree**
  - **`MainLayoutComponent`** wraps authenticated/default app chrome: **sidebar + topbar + `<router-outlet>``**.
  - **Auth-only routes** (`/login`, `/user/register`) render **without** the main shell for a focused, Linear-style auth surface.
- **Responsive:** sidebar becomes **drawer** below `1024px` (overlay + toggle); desktop shows fixed rail.
- **Content region:** scrolls independently; sticky topbar.

## 5) Shared component strategy

| Primitive    | Responsibility |
|-------------|----------------|
| `ui-button` | Variants: `primary`, `secondary`, `ghost`, `danger`; sizes; loading state |
| `ui-input`  | CVA; floating label feel; focus glow from tokens |
| `ui-modal`  | Headless-ish: title, body projection, footer slot; ESC + backdrop close |
| `ui-card`   | `elevated` \| `glass`; hover lift + shadow transition |
| `ui-skeleton` | Shapes: `text`, `rect`, `circle`; used in dashboard loading rows |
| `ui-progress-donut` | Streak / daily goal visualization |

**Motion:** respect `prefers-reduced-motion`; shorten transitions to near-zero when set.

## 6) Inspiration mapping

| Reference | Applied as |
|-----------|------------|
| Duolingo  | Playful accent, rounded cards, progress donut, bouncy micro-interactions |
| LingQ     | Reader-first calm surfaces, generous whitespace |
| Linear    | Dense topbar, command palette slot (future), crisp borders |
| Notion    | Soft neutrals, sidebar hierarchy, subtle hover |
| SaaS      | Glass hero, dashboard stat grid, skeleton loading |

---

Implementation phases (this repo state): **layout → sidebar → navbar → dashboard → skeletons → button → input → modal** (done in code alongside this doc).

## 7) Implemented file map (reference)

| Area | Path |
|------|------|
| Global SCSS entry | `src/styles/main.scss` |
| Tokens (light/dark) | `src/styles/tokens/_css-variables.scss` |
| Mixins (glass, focus) | `src/styles/abstracts/_mixins.scss` |
| Shell layout CSS | `src/styles/layout/_app-shell.scss` |
| Theme service | `src/app/core/theme/theme.service.ts` |
| Main layout + outlet | `src/app/layout/main-layout/` |
| Sidebar / topbar | `src/app/layout/shell-sidebar/`, `shell-topbar/` |
| UI primitives | `src/app/shared/ui/*` |
| Dashboard hub (shell demo) | `src/app/pages/dashboard/` |
| Routes | `MainLayoutComponent` wraps app routes; `login` + `user/register` are full-bleed |
