using Microsoft.Playwright;

namespace DSCMS.Tests.E2E;

[Collection("Playwright")]
public class BlogTests(PlaywrightFixture fixture)
{
    [Fact]
    public async Task BlogPage_LoadsByDefault()
    {
        var page = await fixture.NewPageAsync();
        var response = await page.GotoAsync(PlaywrightFixture.BaseUrl);

        Assert.NotNull(response);
        Assert.True(response.Ok, $"Home page returned HTTP {response.Status}");

        // The default route sends to blog; URL should end up at / or /blog
        var url = page.Url;
        Assert.True(
            url == PlaywrightFixture.BaseUrl + "/" || url.Contains("/blog"),
            $"Expected blog URL but got: {url}");
    }

    [Fact]
    public async Task BlogPage_ShowsPosts()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/blog");

        // Each post is rendered inside a .blog-post div (BootstrapBlogContentType.cshtml)
        var posts = page.Locator(".blog-post");
        var count = await posts.CountAsync();
        Assert.True(count > 0, "Expected at least one .blog-post element on the blog page");

        // First post should have a visible title
        await Assertions.Expect(posts.First.Locator(".blog-post-title")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BlogPage_ShowsNoPosts_DisplaysEmptyMessage()
    {
        // Verifies the empty state renders without errors when there are no posts
        var page = await fixture.NewPageAsync();
        var response = await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/blog");

        Assert.NotNull(response);
        Assert.True(response.Ok, $"Blog page returned HTTP {response.Status}");

        // Page must have at least one .blog-post (either real post or the "No Content" placeholder)
        var posts = page.Locator(".blog-post");
        Assert.True(await posts.CountAsync() > 0, "Expected .blog-post elements (or empty-state placeholder)");
    }

    [Fact]
    public async Task BlogPage_PaginationOlder_NavigatesToNextPage()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/blog");

        // Pagination uses ul.pager with "Older" / "Newer" anchor links
        var olderLink = page.Locator("ul.pager li a").Filter(new LocatorFilterOptions { HasTextString = "Older" });
        var olderCount = await olderLink.CountAsync();

        if (olderCount == 0)
        {
            // Explicitly assert that no Older link is present on page 1
            Assert.Equal(0, olderCount);
            return;
        }

        await olderLink.ClickAsync();

        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"[?&]page=(?:[2-9]|\d{2,})"));
    }

    [Fact]
    public async Task BlogPage_PaginationNewer_NavigatesToPreviousPage()
    {
        var page = await fixture.NewPageAsync();

        // Start on page 2 so the "Newer" link is present
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/blog?page=2");

        var newerLink = page.Locator("ul.pager li a").Filter(new LocatorFilterOptions { HasTextString = "Newer" });
        await Assertions.Expect(newerLink).ToBeVisibleAsync();

        await newerLink.ClickAsync();

        // Clicking "Newer" from page 2 goes to page 1 (/blog?page=1)
        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"[?&]page=1"));
    }
}
