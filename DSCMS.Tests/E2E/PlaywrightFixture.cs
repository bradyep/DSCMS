using Microsoft.Playwright;

namespace DSCMS.Tests.E2E;

/// <summary>
/// xUnit class fixture that manages a single Playwright browser instance
/// shared across all tests in a collection. Each test creates its own page.
/// 
/// SETUP: Before running E2E tests, install Playwright browsers once:
///   pwsh DSCMS.Tests/bin/Debug/net10.0/playwright.ps1 install
/// 
/// The app must be running locally before executing these tests:
///   dotnet run --project DSCMS/DSCMS.csproj
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public const string BaseUrl = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    public async Task<IPage> NewPageAsync()
    {
        return await Browser.NewPageAsync();
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
