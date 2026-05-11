# UI/UX Improvement Plan

## Current UI Findings

- Layout is desktop-first with a fixed left menu.
- Header, menu, content, and footer are all in `app.component`.
- Navigation labels are useful but long and not grouped by learner tasks.
- The product does not yet feel like a guided learning app; it feels like an admin/content management system with learner pages.
- Color usage is mostly a single purple accent.
- There is no design token system.
- Loading, empty, error, success, and disabled states are inconsistent or missing.
- Some text encoding appears broken in comments/content.
- Accessibility semantics are limited: nav landmarks, focus states, ARIA, and keyboard behavior need work.

## Target Experience

The application should feel:

- Friendly for foreigners.
- Calm and confidence-building.
- Clear for complete beginners.
- Interactive without being childish.
- Mobile-friendly for short daily lessons.
- Structured enough for serious learners.

## Design System

Use design tokens:

```css
:root {
  --color-primary: #3b82f6;
  --color-primary-strong: #2563eb;
  --color-accent: #16a34a;
  --color-warning: #f59e0b;
  --color-danger: #dc2626;
  --color-bg: #f8fafc;
  --color-surface: #ffffff;
  --color-text: #0f172a;
  --color-muted: #64748b;
  --radius-sm: 6px;
  --radius-md: 8px;
  --shadow-sm: 0 1px 2px rgba(15, 23, 42, 0.08);
}
```

Avoid a one-note purple UI. Use blue for learning structure, green for progress, amber for reminders, red only for destructive/error states.

## App Shell

Replace the current fixed sidebar with:

- Desktop: compact sidebar with grouped nav.
- Tablet: collapsible rail.
- Mobile: top bar + bottom navigation for core learner actions.

Recommended learner nav:

- Today
- Learn
- Practice
- Search
- Profile

Recommended admin nav:

- Dashboard
- Content
- Tests
- Media
- Users

## Learner Dashboard

Add first-screen dashboard sections:

- Today's lesson.
- Current level and progress.
- Review queue.
- Streak.
- Weak skills.
- Recommended practice.

No marketing-style landing page for logged-in users. The first screen should be actionable.

## Component Rules

- Use shared UI components for button, input, select, modal, toast, table, pagination, progress indicator, badge, and empty state.
- Keep components below 300 lines where possible.
- Move API calls into services/facades.
- Move complex quiz state into dedicated state services.
- Avoid `alert()` for errors. Use toast and inline validation.

## Responsive Rules

- Use flexible grids and container width constraints.
- Keep tap targets at least 44x44 px.
- Avoid fixed sidebar widths on small screens.
- Ensure quiz cards, answer choices, and media fit on mobile.
- Keep forms one column on mobile and two columns only where space allows.

## Accessibility Rules

- Use semantic landmarks: `header`, `nav`, `main`, `footer`.
- Add visible focus states.
- All buttons need readable labels.
- Use `aria-current` for active route.
- Use proper labels for forms.
- Do not rely on color alone to show correctness.
- Support keyboard navigation for quiz options and modals.

## High-Impact UI Refactors

1. Create `AppShellComponent`.
2. Move sidebar nav into `NavigationComponent`.
3. Create `LearnerDashboardComponent`.
4. Create `AdminDashboardComponent`.
5. Create shared `LoadingStateComponent`, `EmptyStateComponent`, and `ToastService`.
6. Standardize CSS tokens in `src/styles.css`.
7. Add responsive route layout classes.
