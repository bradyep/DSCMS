using Microsoft.Playwright;

namespace DSCMS.Tests.E2E;

[Collection("Playwright")]
public class SectionSmokeTests(PlaywrightFixture fixture)
{
    [Theory]
    [InlineData("games")]
    [InlineData("projects")]
    [InlineData("about")]
    public async Task Section_LoadsWithoutError(string section)
    {
        var page = await fixture.NewPageAsync();
        var response = await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/{section}");

        Assert.NotNull(response);
        Assert.True(response.Ok, $"/{section} returned HTTP {response.Status}");
    }

    [Theory]
    [InlineData("games")]
    [InlineData("projects")]
    [InlineData("about")]
    public async Task Section_RendersContentArea(string section)
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/{section}");

        // All content type templates render inside .blog-main
        var contentArea = page.Locator(".blog-main");
        await Assertions.Expect(contentArea).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ProjectsPage_ShowsProjectItems()
    {
        var page = await fixture.NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/projects");

        // BootstrapProjects.cshtml wraps each project in a .blog-post div
        var projects = page.Locator(".blog-post");
        Assert.True(await projects.CountAsync() > 0, "Expected at least one project item on the projects page");
    }
}
