# Plan: Responsive Sidebar & Column Layout Changes

## Goal
- **Blog List View**: 3 columns of post cells (no sidebar) or 2 columns + sidebar (with sidebar)
- **Blog Article View**: Full-width article (no sidebar) or 2/3 article + 1/3 sidebar (with sidebar)
- **Mobile**: Single column with sidebar stacked at the bottom

## Current State
- `BlogLayout.html` line 55: content is `col-12` (no sidebar) or `col-md-8` (with sidebar), sidebar is `col-md-4`
- `BlogPostCell.html` line 19: list-view cells are `col-12 col-md-6` (2 per row), article-view cells are `col-12` (full width)
- `BlogWidgetViewModel.cs` line 124-127: `hasSidebar` computed from checkbox fields
- `blogs.css` lines 341-383: legacy float-based sidebar layout (partially overridden by Bootstrap grid in the HTML)

## Changes

### 1. BlogPostCell.html — Dynamic column width based on sidebar
**File**: `ui/layoutFiles/BlogPostCell.html`, line 19

Change the list-view cell classes from a fixed `col-md-6` to sidebar-aware sizing:
```html
<!-- Before -->
<div class="{{#isArticleView}}col-12{{/isArticleView}} {{^isArticleView}}col-12 col-md-6 d-flex{{/isArticleView}}">

<!-- After -->
<div class="{{#isArticleView}}col-12{{/isArticleView}} {{^isArticleView}}col-12 {{#hasSidebar}}col-md-6{{/hasSidebar}} {{^hasSidebar}}col-md-4{{/hasSidebar}} d-flex{{/isArticleView}}">
```

- **No sidebar** → `col-md-4` = 3 posts per row
- **With sidebar** → `col-md-6` = 2 posts per row (within the 8-column container)

This is the cleanest approach: no extra CSS needed, the Bootstrap grid handles everything, and the Mustache `hasSidebar` variable is already available in the post cell template context.

### 2. BlogWidgetViewModel.cs — Pass `hasSidebar` to post cell context
**File**: `source/aoBlogs2/Models/View/BlogWidgetViewModel.cs`

Verify that `hasSidebar` is available in the post cell rendering context. Currently `hasSidebar` is on the outer `BlogWidgetViewModel` but post cells are rendered via `BlogPostCellViewModel`. We need to ensure `hasSidebar` is passed through.

- Check `BlogPostCellViewModel.cs` for a `hasSidebar` property — if missing, add it
- Check where post cells are rendered (likely in the list/article view code) and ensure `hasSidebar` is set on each cell's ViewModel

### 3. BlogLayout.html — No changes needed
**File**: `ui/layoutFiles/BlogLayout.html`

The current layout already handles the sidebar conditional correctly:
- Line 55: `col-12 {{#hasSidebar}}col-md-8{{/hasSidebar}}` — content gets `col-md-8` with sidebar, full width without
- Lines 59-178: sidebar wrapped in `{{#hasSidebar}}` block with `col-md-4`

This already gives us:
- **List view with sidebar**: 8-col content + 4-col sidebar (post cells at `col-md-6` = 2 per row within 8 cols)
- **List view no sidebar**: 12-col content (post cells at `col-md-4` = 3 per row)
- **Article view with sidebar**: 8-col content + 4-col sidebar (article at `col-12` = full width of 8 cols = 2/3 page)
- **Article view no sidebar**: 12-col content (article at `col-12` = full width)

### 4. Mobile Responsiveness — Already handled
Bootstrap's grid is mobile-first:
- Below `md` (768px): all `col-md-*` classes are ignored, everything falls back to `col-12`
- Post cells stack to single column
- Sidebar stacks below content (it comes after content in the DOM order)
- No additional CSS or changes needed for mobile

### 5. CSS cleanup (optional, low risk)
**File**: `ui/layoutFiles/blogs/blogs.css`, lines 341-383

The legacy float-based `.blogColumn1` / `.blogSidebar` styles with fixed pixel widths are no longer used (the HTML uses Bootstrap grid classes instead). These could be removed to avoid confusion, but this is cosmetic and not required for the feature to work.

## Files to Modify
1. **`ui/layoutFiles/BlogPostCell.html`** — Change post cell column classes (line 19)
2. **`source/aoBlogs2/Models/View/BlogPostCellViewModel.cs`** — Add `hasSidebar` property (if not present)
3. **Post cell rendering code** — Pass `hasSidebar` value to each post cell ViewModel

## Files Unchanged
- `BlogLayout.html` — Already correct for sidebar/no-sidebar layout
- `BlogListView.html` — Just a wrapper, no changes needed
- `BlogArticleView.html` — Just a wrapper, no changes needed
- `blogs.css` — No changes required (optional cleanup only)

## Risk Assessment
- **Low risk**: The Mustache conditional approach is consistent with existing patterns throughout the templates
- **Backward compatible**: Existing blogs with sidebar enabled will see 2 columns (same as current `col-md-6`); blogs without sidebar get the new 3-column layout
- **Mobile behavior unchanged**: Bootstrap responsive breakpoints handle the stacking automatically
