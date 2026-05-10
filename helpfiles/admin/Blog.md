# Blog Administration Guide

The Blog collection provides a full-featured blogging solution for your website. It supports multiple blog instances, rich text posts with images, categories, tags, comments with moderation, RSS feeds, email subscriptions, social media links, and sidebar widgets. Each blog is added to a page as a widget and configured independently, allowing different blogs with different settings on a single site.

## Key Features

- Multiple blogs per site, each with independent settings
- Rich text post editor with image galleries
- Categories and tags for organizing posts
- Comment system with moderation and spam protection
- RSS feed generation with podcast support
- Email subscription for readers
- Sidebar widgets: search, archive list, social media links, call-to-action
- SEO metadata at the blog and post level
- View tracking and analytics dashboard
- Latest Posts widget for promoting blog content on other pages

---

## Admin Activities

### Creating a Blog

1. Navigate to the page where you want the blog to appear.
2. Enable **Advanced Edit** in the Contensive toolbar.
3. Add the **Blog** widget to the page content area.
4. The system automatically creates a default blog with a sample post and a sample Call-to-Action widget.
5. Open the blog settings through the **Options** icon on the widget to configure the blog name, caption, description, and features.

When a blog is first created, it uses these defaults:
- **Posts to Display**: 5
- **Overview Length**: 500 characters
- **Thumbnail Image Width**: 200px
- **Max Image Width**: 400px
- **Allow Anonymous Comments**: Yes
- **Auto-Approve Comments**: No
- **Allow Categories**: Yes
- **Allow Article CTA**: Yes

### Adding a Blog Post

1. With **Advanced Edit** enabled, navigate to the blog page.
2. Click the **Create** button on the blog widget to open the post editor.
3. Enter a **Title** for the post. This title becomes the headline displayed on the blog.
4. Write the post content using the **WYSIWYG editor**.
5. Optionally add a **Tag List** (comma-separated values) if tagging is desired.
6. Optionally select a **Category** from the dropdown if categories are enabled on the blog.
7. Upload images using the image upload section (see Managing Images below).
8. Click **Post** to publish, or **Cancel** to discard.

The post's **Date Published** defaults to the current date. The post author is set to the currently logged-in user.

An RSS feed entry is automatically created for each new post.

### Editing a Blog Post

1. Navigate to the blog and find the post you want to edit.
2. With **Advanced Edit** enabled, click the **Edit** link that appears on the post.
3. Modify the title, content, tags, category, or images as needed.
4. Click **Apply** to save changes.

Alternatively, use the **Content Management Portal**:
1. Open the portal and navigate to **Blog List**.
2. Select the blog, then select **Post List**.
3. Click on the post to open the **Post Details** editor.
4. Edit the post content and click **Save** or **OK**.

The portal also provides separate tabs for **Post Info** (metadata, dates, tags), **Post SEO** (meta title, description, keywords), and **Post RSS** (RSS title, description, podcast links).

### Managing Images

#### Primary Image

The **Primary Image** is the featured image for a post. It appears at the top of the article and is used for social media sharing.

- Upload via the post editing form or the admin portal Post Info tab.
- Set the **Primary Image Description** for accessibility (alt text).
- Control how it displays in the post list with **Primary Image Position**: Per Stylesheet (1), Right (2), Left (3), or Hide (4).
- Control how it displays in the article view with **Article Primary Image Position** using the same options.

#### Secondary Images

Additional images can be added to a post through the image upload section in the post editor:

1. In the post editor, use the **Add Image** button to add a new image upload row.
2. Upload the image file.
3. Set the **Name** (alt text) and optional **Description** for each image.
4. Set the **Order** value to control display sequence.
5. To remove an image, check the **Delete** checkbox next to it.

Secondary image display is controlled by the **Image Display Type** field: Hidden, List After Content, or Slider After Content.

#### Default Blog Image

Set a **Default Image** at the blog level to serve as a fallback for posts that have no primary image. This is configured in the blog settings.

#### Image Sizing

- **Thumbnail Image Width**: Controls the width of images in the post list view (default: 200px in new blogs).
- **Max Image Width**: Controls the maximum width of secondary images when displayed (default: 400px).

### Updating the Publish Date

Keeping your blog fresh improves SEO and reader engagement. To update a post's publish date:

1. Open the post in the admin portal or the front-end editor.
2. Update the **Date Published** field to the desired date.
3. Save the post.

If the **Date Published** field is blank, the system uses the original **Date Added** value. Posts are displayed in reverse chronological order by publish date, so updating this date moves the post to a more recent position in the list.

The blog also has a **Blog Update Alarm Days** setting (default: 30) that triggers an alert if no new post has been added within the specified number of days.

### Managing Categories

Categories provide a way to organize posts into groups. Each post can belong to one category.

1. Enable categories by checking **Allow Categories** in the blog settings.
2. Create categories by adding records to the **Blog Categories** content definition.
3. When editing a post, select a category from the dropdown.
4. Visitors can filter posts by category on the blog.

Categories also support **group-based access restrictions** through the Blog Category Group Rules. This allows you to restrict certain categories so only members of specific groups can view posts in those categories.

### Managing Tags

Tags provide flexible, keyword-based organization for posts. Unlike categories, a post can have multiple tags.

1. When creating or editing a post, enter tags in the **Tag List** field as comma-separated values (e.g., "news, updates, product launch").
2. Tags appear on each post and are clickable, allowing visitors to find other posts with the same tag.
3. Tags are used in blog search results.

### Managing Comments

The blog supports a full comment system with moderation controls.

#### Blog-Level Comment Settings

Configure these in the blog settings:

- **Allow Anonymous Comments**: When enabled, visitors can comment without logging in. When disabled, visitors must authenticate first. A login form appears automatically, and if the site allows member registration, a "Join?" link is displayed.
- **Auto-Approve Comments**: When enabled, comments appear immediately. When disabled, comments require manual approval before they are visible.
- **Email Comment Notifications**: When enabled, the blog author receives an email notification whenever a new comment is posted. The notification includes the blog name and the post title.
- **reCAPTCHA**: When enabled, a reCAPTCHA challenge is displayed on the comment form to prevent spam.

#### Post-Level Comment Settings

Each post has an **Allow Comments** field that can enable or disable comments on that specific post, independent of blog-level settings.

#### Moderating Comments

1. Open the **Blog Comments** content definition in the admin area.
2. Review pending comments (those with **Approved** unchecked).
3. Check **Approved** to publish the comment, or delete it to remove spam.
4. Comments display the commenter's name, the comment title, the comment body, and the associated post.

### Configuring the Sidebar

The blog sidebar contains optional widgets that can be individually enabled or disabled in the blog settings. Each widget adds functionality to the sidebar area of the blog.

#### Search Widget

- **Allow Search**: Enabled by default. Displays a search box in the sidebar that allows visitors to search blog posts by keyword.

#### Archive List Widget

- **Allow Archive List**: Displays a month-by-month archive list in the sidebar. Visitors can click a month to view all posts published during that period.

#### Email Subscription Widget

- **Allow Email Subscribe**: Displays a subscription form where visitors can enter their email address to receive blog updates.
- **Email Subscribe Group**: Select the group that email subscribers are added to. This group can then be used for email campaigns.

#### RSS Subscription Widget

- **Allow RSS Subscribe**: Displays an RSS feed link in the sidebar, allowing visitors to subscribe to the blog using an RSS reader. The RSS feed is automatically generated and maintained.

#### Social Media Follow Links

These widgets display links to your social media profiles in the sidebar:

- **Allow Facebook Link**: Enable and enter your **Facebook Link** URL.
- **Allow Twitter Link**: Enable and enter your **Twitter Link** URL.
- **Allow Google Plus Link** (Legacy): This feature references Google+, which is no longer active. It remains in the system for backward compatibility but should not be enabled for new blogs.
- **Follow Us Caption**: Customize the heading above social media links (default: "Follow Us").

#### Call-to-Action Widgets

- **Allow Article CTA**: When enabled, posts can include Call-to-Action widgets in the sidebar. CTAs are promotional blocks with a headline, brief description, and a link.
- Each post can have multiple CTAs assigned through the **Calls to Action** relationship field on the post.
- CTAs are managed as separate content records in the **Calls to Action** content definition, with fields for headline, brief text, link URL, and image.

### Managing the Latest Posts Widget

The **Latest Posts Widget** is a standalone widget that can be placed on any page (not just the blog page) to display recent blog posts.

1. Add the **Latest Posts Widget** to a page using the content editor.
2. Configure it through the widget options. It has its own settings record.
3. Set a **Default Post Image** to display for posts that have no primary image.
4. The widget includes design block styling options (padding, background, theme).

### Configuring RSS Feeds

An RSS feed is automatically created when a new blog is set up. Each blog has an associated **RSS Feed** record.

- The RSS feed updates automatically when posts are added, edited, or deleted.
- Each post has optional RSS-specific fields: **RSS Title**, **RSS Description**, **RSS Link**, **RSS Date Publish**, and **RSS Date Expire**.
- For podcasts, use the **Podcast Media Link** field on a post to add an audio or video enclosure to the RSS feed entry. Set the **Podcast Size** to indicate the file size.

### Managing SEO and Metadata

SEO metadata can be set at both the blog level and individual post level.

#### Blog-Level SEO

These fields apply to the blog's landing/list page:

- **Meta Title**: The page title shown in search results.
- **Meta Description**: The description shown in search results.
- **Meta Keyword List**: Keywords for the blog landing page.

#### Post-Level SEO

Each post has its own metadata fields:

- **Meta Title**: Overrides the page title when viewing this post.
- **Meta Description**: Description for this specific post in search results.
- **Meta Keyword List**: Keywords specific to this post.

### Display Settings

These settings control how the blog list page appears:

- **Posts to Display**: Number of posts shown per page before pagination begins (default: 5).
- **Overview Length**: Maximum number of characters displayed in the post preview on the list page. HTML is stripped from the preview (default: 500 in new blogs, 150 in XML definition).
- **Thumbnail Image Width**: Width in pixels for post thumbnail images on the list page (default: 200px in new blogs).
- **Max Image Width**: Maximum width in pixels for secondary images displayed within a post (default: 400px).

### Blog Update Alarm

- **Blog Update Alarm Days**: Set the number of days after which the system alerts you if no new post has been published (default: 30). This helps ensure the blog stays current.

### Authoring Group

- **Authoring Group**: Optionally assign a group whose members are allowed to create blog posts. If not set, the default **Blog Authors** group is used. Users must be members of the authoring group to access the post editor on the front end.

### Blog Caption and Description

- **Caption**: The public-facing title of the blog, displayed as the blog headline.
- **Copy**: An HTML description shown at the top of the blog, above the post list. Use this for an introduction or instructions for readers.

---

## Field Reference

### Blog Settings (Blogs)

Table: `ccBlogs`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| name | Text | Auto-generated | Internal name of the blog (not displayed publicly). |
| caption | Text | Auto-generated | Public-facing blog title displayed as the headline. |
| copy | HTML | Sample text | Blog description displayed at the top of the post list. |
| ownerMemberId | Lookup (People) | Current user | The creator and primary owner of the blog. Receives comment notifications. |
| defaultImageFilename | File | (none) | Default fallback image for posts without a primary image. |
| rssFeedId | Lookup (RSS Feeds) | Auto-created | The RSS feed associated with this blog. |
| postsToDisplay | Integer | 5 | Number of posts shown per page on the blog list view. |
| overviewLength | Integer | 150 | Maximum character count for post previews on the list page. |
| thumbnailImageWidth | Integer | 150 | Width in pixels for thumbnail images on the list page. |
| imageWidthMax | Integer | 400 | Maximum width in pixels for secondary images in posts. |
| blogUpdateAlarmDays | Integer | 30 | Days before a stale-blog alert is triggered. |
| allowAnonymous | Boolean | false | Allow comments from unauthenticated visitors. |
| autoApproveComments | Boolean | false | Automatically approve new comments without moderation. |
| emailComment | Boolean | false | Send email notification to blog owner when a comment is posted. |
| recaptcha | Boolean | false | Display reCAPTCHA on the comment form for spam protection. |
| allowCategories | Boolean | false | Enable category selection on posts. |
| allowArticleCTA | Boolean | false | Enable Call-to-Action widgets on posts. |
| allowSearch | Boolean | true | Display a search box in the blog sidebar. |
| allowArchiveList | Boolean | false | Display a monthly archive list in the sidebar. |
| allowEmailSubscribe | Boolean | false | Display an email subscription form in the sidebar. |
| emailSubscribeGroupId | Lookup (Groups) | (none) | Group that email subscribers are added to. |
| allowRSSSubscribe | Boolean | false | Display an RSS feed subscription link in the sidebar. |
| allowFacebookLink | Boolean | false | Display a Facebook follow link in the sidebar. |
| facebookLink | Text | (none) | URL to your Facebook page. |
| allowTwitterLink | Boolean | false | Display a Twitter follow link in the sidebar. |
| twitterLink | Text | (none) | URL to your Twitter profile. |
| allowGooglePlusLink | Boolean | false | (Legacy) Display a Google+ follow link. Google+ is discontinued. |
| googlePlusLink | Text | (none) | (Legacy) URL to Google+ profile. |
| followUsCaption | Text | "Follow Us" | Heading text displayed above social media links. |
| metaTitle | Text | (none) | SEO page title for the blog landing page. |
| metaDescription | Text | (none) | SEO description for the blog landing page. |
| metaKeywordList | Text | (none) | SEO keywords for the blog landing page. |
| authoringGroupId | Lookup (Groups) | (none) | Group whose members can create posts. Defaults to "Blog Authors" group if not set. |

### Blog Entry (Post) Fields

Table: `ccBlogCopy`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| name | Text | (none) | Post headline/title displayed on the blog. |
| copy | HTML | (none) | Full HTML content of the blog post. |
| datePublished | Date | dateAdded | Public publish date. If blank, the system uses the date the post was created. |
| blogId | Lookup (Blogs) | (none) | The blog this post belongs to. |
| blogCategoryId | Lookup (Blog Categories) | (none) | Optional category assignment for this post. |
| tagList | Text | (none) | Comma-separated list of tags for the post. |
| authorMemberId | Lookup (People) | (none) | Post author. Must be a member of the Blog Authors group. |
| primaryImage | File | (none) | Featured image for the post. Used in list view and social sharing. |
| primaryImageDescription | Text | (none) | Alt text/description for the primary image. |
| primaryImageAltSizeList | Text | (none) | Responsive image size list for the primary image (developer use). |
| primaryImagePositionId | Integer | 1 | How the primary image displays in the post list: 1 = Per Stylesheet, 2 = Right, 3 = Left, 4 = Hide. |
| articlePrimaryImagePositionId | Integer | (none) | How the primary image displays in the article detail view. Same options as above. |
| imageDisplayTypeId | Integer | (none) | How secondary images display: Hidden, List After Content, or Slider After Content. |
| allowComments | Boolean | false | Enable or disable comments on this specific post. |
| viewings | Integer | 0 | Read-only count of how many times this post has been viewed. |
| podcastMediaLink | Text | (none) | URL to a podcast audio or video file. Creates an RSS enclosure. |
| podcastSize | Integer | 0 | File size of the podcast media in bytes. |
| rssTitle | Text | (none) | Custom title for the RSS feed entry. If blank, uses the post title. |
| rssDescription | Text | (none) | Custom description for the RSS feed entry. If blank, auto-generated from post content. |
| rssLink | Text | (none) | Custom link URL for the RSS feed entry. |
| rssDatePublish | Date | (none) | Date the post becomes available in the RSS feed. |
| rssDateExpire | Date | (none) | Date the post is removed from the RSS feed. |
| metaTitle | Text | (none) | SEO page title when viewing this post. |
| metaDescription | Text | (none) | SEO description for this post. |
| metaKeywordList | Text | (none) | SEO keywords for this post. |
| callsToAction | Many-to-Many | (none) | Call-to-Action widgets associated with this post (requires allowArticleCTA on blog). |

### Blog Categories

Table: `BlogCategories`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| name | Text | (none) | Display name of the category. |
| active | Boolean | true | Whether the category is active and available for selection. |
| blockingGroups | Many-to-Many (Groups) | (none) | Groups that are allowed to view posts in this category. If set, only members of these groups see the posts. |
| userBlocking | Boolean | false | Enable user-level blocking for this category. |

### Blog Comments

Table: `ccBlogComments`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| name | Text | (none) | Comment subject or title. |
| copyText | Text | (none) | The body text of the comment. |
| approved | Boolean | false | Whether the comment is approved and visible. Auto-set to true if autoApproveComments is enabled on the blog. |
| blogId | Lookup (Blogs) | (auto) | The blog this comment belongs to (read-only). |
| entryId | Lookup (Blog Entries) | (auto) | The post this comment is attached to. |
| formKey | Text | (auto) | Anti-spam token (system-managed). |

### Blog Images

Table: `BlogImages`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| name | Text | (none) | Alt text for the image. |
| filename | File | (none) | The uploaded image file. |
| description | Text | (none) | Optional description of the image. |
| height | Integer | 0 | Image height in pixels. |
| width | Integer | 0 | Image width in pixels. |
| altSizeList | Text | (none) | Responsive image sizes for different viewport widths (developer use). |
| blogEntryId | Lookup (Blog Entries) | (none) | The post this image is directly attached to. Blank for shared images. |
| sortOrder | Integer | 0 | Display order of the image within a post. Lower numbers appear first. |

### Latest Posts Widget

Table: `LatestPostWidgets`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| defaultPostImage | File | (none) | Default image displayed for posts that have no primary image. |

The Latest Posts Widget also inherits design block styling fields from its base class:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| padTop | Integer | 0 | Top padding in pixels. |
| padRight | Integer | 0 | Right padding in pixels. |
| padBottom | Integer | 0 | Bottom padding in pixels. |
| padLeft | Integer | 0 | Left padding in pixels. |
| backgroundImageFilename | File | (none) | Background image for the widget container. |
| asFullBleed | Boolean | false | Render the widget as full-width, edge to edge. |
| styleHeight | Text | (none) | Custom CSS height value with units (e.g., "300px"). |
| themeStyleId | Lookup | (none) | Optional theme wrapper class for styling. |
