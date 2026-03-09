# aoBlog

aoBlog is a C# .NET Framework 4.7.2 blogging addon for the Contensive CMS platform. It builds a signed assembly (aoBlogs2.dll) that gets packaged into a Contensive collection zip for deployment.

## Reference Contensive Development

- https://raw.githubusercontent.com/contensive/Contensive5/refs/heads/master/README.md

## Build

Must be run from the `scripts/` folder:

```cmd
cd scripts
build.cmd
```

The build script:
1. Auto-generates a version number (`YY.MM.DD.revision`)
2. Copies UI assets from `ui/` to the collection folder
3. Builds the solution with MSBuild (VS 2022 Community)
4. Packages everything into `collections/Blog/Blog.zip`
5. Deploys to `C:\deployments\aoBlog\Dev\{version}\`

There are no tests in this project.

## Architecture

MVC-style addon under namespace `Contensive.Blog` / `Contensive.Addons.Blog`:

- **Addons/** - Contensive addon entry points (implement `CPAddonBaseClass`). `BlogWidget.cs` is the main entry point; `LatestPostsWidget.cs` and `DashWidgetBlogViewsByArticle.cs` are secondary widgets.
- **Controllers/** - Business logic. `BlogBodyController.cs` orchestrates the main blog rendering. `MetadataController.cs` handles SEO. `LinkAliasController.cs` manages URL routing.
- **Models/Db/** - Database entity models mapping to Contensive content tables (ccBlogs, ccBlogEntries, ccBlogComments, etc.)
- **Models/Domain/** - Domain models. `ApplicationEnvironmentModel.cs` holds the application state passed through the rendering pipeline.
- **Models/View/** - View models for widget rendering.
- **Views/** - HTML generation. `ArticleView.cs` renders individual posts, `ListView.cs` renders post lists, `EditView.cs` renders the post editor, `SidebarView.cs` builds the sidebar.
- **constants.cs** - All content names, request parameter names, GUIDs, form IDs, and enums.

## Key Dependencies

- `Contensive.CPBaseClass` - Core Contensive API (the `CPBaseClass` / `CPClass` instance is injected into addons)
- `Contensive.DbModels` - Shared database model base classes
- `Contensive.DesignBlockBase` - Design block framework

## Collection Package

`collections/Blog/Blog.xml` defines the addon collection: addon registrations, resource mappings, metadata definitions, and help text. UI templates (`BlogListLayout.html`, `BlogStoryLayout.html`, `LatestPostLayout.html`) and static assets live in `ui/` and are copied into the collection at build time.
