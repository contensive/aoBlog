# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Read `README.md` first before reading this file.

## Contensive Patterns Reference

This project is built on the Contensive platform. Before implementing any Contensive-specific code (admin UI, addons, database models, collection XML, portals, page widgets, remote methods, etc.), you MUST read the relevant pattern documentation.

**Start here — read the patterns index to find the right pattern for your task:**
- [Contensive Patterns Index](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/index.md)

Then read the specific pattern(s) that apply to the work you are doing. Key patterns include:
- [AdminUI Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/adminui-pattern.md) — LayoutBuilderList, LayoutBuilderNameValue, filters, sorting, pagination, CSV export
- [Addon Collection Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/addon-collection-pattern.md) — collection XML, CDefs, Fields, packaging
- [Database Models Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/database-models-pattern.md) — typed C# model classes
- [Addon Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/addon-pattern.md) — addon architecture and types
- [Portal Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/portal-pattern.md) — portal-style admin interfaces
- [Page Widget Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/addon-page-widget-pattern.md) — design block widgets
- [Best Practices](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/best-practices-pattern.md) — error handling, try/catch, coding conventions
- [Security Best Practices](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/security-best-practices.md) — authentication, authorization, secure coding

Do NOT implement Contensive features from memory — always fetch and read the current pattern documentation first.

## Code Style

- Prefer string interpolation over concatenation
- The codebase historically used VB.NET; some VB patterns remain (e.g., `Microsoft.VisualBasic.Constants.vbCrLf` via the `cr` constant)

## Testing

Follow the [Contensive Testing Pattern](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/patterns/testing-pattern.md) for all testing conventions.

For the reference E2E implementation with full documentation, see [Contensive5 E2E README](https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/tests/e2e/README.md).

- E2E tests: `tests/e2e/` (Playwright, TypeScript) -- not yet created, follow the pattern to set up
- No xUnit integration tests exist in this project
- Key pages to test: blog list view, article view, search, archive, comments, email subscription, latest posts widget, admin portal features (blog list, blog details, post list, post details)
