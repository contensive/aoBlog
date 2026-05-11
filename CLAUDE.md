# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Read `README.md` first before reading this file.

## Code Style

- Prefer string interpolation over concatenation
- The codebase historically used VB.NET; some VB patterns remain (e.g., `Microsoft.VisualBasic.Constants.vbCrLf` via the `cr` constant)

## Testing

Follow the [Contensive Testing Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/testing-pattern.md) for all testing conventions.

For the reference E2E implementation with full documentation, see [Contensive5 E2E README](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/tests/e2e/README.md).

- E2E tests: `tests/e2e/` (Playwright, TypeScript) -- not yet created, follow the pattern to set up
- No xUnit integration tests exist in this project
- Key pages to test: blog list view, article view, search, archive, comments, email subscription, latest posts widget, admin portal features (blog list, blog details, post list, post details)
