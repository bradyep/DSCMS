using System.Diagnostics;
using System.Net.Http;
using Microsoft.Playwright;

namespace DSCMS.Tests.E2E;

/// <summary>
/// xUnit class fixture that manages a single Playwright browser instance
/// shared across all tests in a collection. Each test creates its own page.
///
/// The fixture automatically starts the DSCMS app before the first test runs
/// and shuts it down after the last test finishes, so no manual setup is
/// needed whether tests are launched from VS Test Explorer or the CLI.
///
/// SETUP: Install Playwright browsers once per machine:
///   pwsh DSCMS.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public const string BaseUrl = "http://localhost:5000";
    private const int AppStartTimeoutSec = 30;

    private Process? _appProcess;
    private bool _appWasAlreadyRunning;

    public async Task InitializeAsync()
    {
        await StartAppIfNeededAsync();

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !IsHeaded()
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();

        if (!_appWasAlreadyRunning)
            StopApp();
    }

    public async Task<IPage> NewPageAsync() => await Browser.NewPageAsync();

    // -------------------------------------------------------------------------

    private async Task StartAppIfNeededAsync()
    {
        // If the app is already listening (e.g. started manually or by the PS script)
        // leave it alone and don't kill it on teardown.
        if (await IsAppRespondingAsync())
        {
            _appWasAlreadyRunning = true;
            return;
        }

        // Locate the DSCMS project relative to the test assembly's output folder.
        // Output: DSCMS.Tests/bin/Debug/net10.0  →  repo root is four levels up.
        var assemblyDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", ".."));
        var projectPath = Path.Combine(repoRoot, "DSCMS", "DSCMS.csproj");

        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --urls \"{BaseUrl}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        _appProcess.Start();

        // Wait until the app is accepting connections.
        var deadline = DateTime.UtcNow.AddSeconds(AppStartTimeoutSec);
        while (DateTime.UtcNow < deadline)
        {
            if (await IsAppRespondingAsync())
                return;
            await Task.Delay(500);
        }

        throw new InvalidOperationException(
            $"DSCMS app did not become ready at {BaseUrl} within {AppStartTimeoutSec}s.");
    }

    private void StopApp()
    {
        try
        {
            _appProcess?.Kill(entireProcessTree: true);
            _appProcess?.WaitForExit(3000);
        }
        catch { /* best-effort */ }
        finally
        {
            _appProcess?.Dispose();
            _appProcess = null;
        }
    }

    private static async Task<bool> IsAppRespondingAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync(BaseUrl);
            return (int)response.StatusCode < 500;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsHeaded() =>
        Environment.GetEnvironmentVariable("HEADED") == "1";
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
