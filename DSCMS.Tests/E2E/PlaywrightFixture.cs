using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.Playwright;

namespace DSCMS.Tests.E2E;

/// <summary>
/// xUnit class fixture that manages a single Playwright browser instance
/// shared across all tests in a collection. Each test creates its own page.
///
/// The fixture always starts its own DSCMS instance on port 5099 (separate
/// from the VS dev-run port 5000) so there is never a conflict with a running
/// dev server.  The instance is shut down after the last test finishes.
///
/// SETUP: Install Playwright browsers once per machine:
///   pwsh DSCMS.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    // Dedicated E2E port — intentionally different from the VS dev-run port (5000).
    public const string BaseUrl = "http://localhost:5099";
    private const int AppStartTimeoutSec = 60;

    private Process? _appProcess;

    public async Task InitializeAsync()
    {
        await StartAppAsync();

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !IsHeaded()
        });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
            await Browser.DisposeAsync();
        Playwright?.Dispose();

        StopApp();
    }

    public async Task<IPage> NewPageAsync() => await Browser.NewPageAsync();

    // -------------------------------------------------------------------------

    private async Task StartAppAsync()
    {
        // Kill any stale process still holding our dedicated E2E port before starting.
        await KillProcessOnPortAsync(5099);

        // Locate the DSCMS project directory relative to the test assembly's output folder.
        // Test output:  DSCMS.Tests/bin/{config}/net10.0
        // Repo root is four levels up from the test assembly directory.
        var assemblyDir = AppContext.BaseDirectory;
        var repoRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", ".."));
        var csproj = Path.Combine(repoRoot, "DSCMS", "DSCMS.csproj");

        // Match the same configuration (Debug/Release) used by the test assembly.
        var configSegment = assemblyDir
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .SkipWhile(p => !p.Equals("bin", StringComparison.OrdinalIgnoreCase))
            .Skip(1)
            .FirstOrDefault() ?? "Debug";

        var tfm = "net10.0";
        var appDll = Path.Combine(repoRoot, "DSCMS", "bin", configSegment, tfm, "DSCMS.dll");

        // Always use "dotnet run" so the content root and launchSettings environment
        // variables are set correctly.  Use --no-build when the DLL already exists
        // (i.e. VS already built it) to avoid a slow incremental rebuild.
        var noBuild = File.Exists(appDll) ? " --no-build" : "";
        var arguments = $"run --project \"{csproj}\"{noBuild} --urls \"{BaseUrl}\"";
        var workingDir = Path.Combine(repoRoot, "DSCMS");

        // Accumulate output asynchronously so the pipe buffer never fills and blocks
        // the app's logging thread (which would prevent Kestrel from ever starting).
        var stdoutBuilder = new System.Text.StringBuilder();
        var stderrBuilder = new System.Text.StringBuilder();

        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
            EnableRaisingEvents = true,
        };

        _appProcess.OutputDataReceived += (_, e) => { if (e.Data != null) stdoutBuilder.AppendLine(e.Data); };
        _appProcess.ErrorDataReceived  += (_, e) => { if (e.Data != null) stderrBuilder.AppendLine(e.Data); };

        _appProcess.Start();
        _appProcess.BeginOutputReadLine();
        _appProcess.BeginErrorReadLine();

        // Wait until the app is accepting connections.
        var deadline = DateTime.UtcNow.AddSeconds(AppStartTimeoutSec);
        while (DateTime.UtcNow < deadline)
        {
            if (await IsAppRespondingAsync())
                return;

            if (_appProcess.HasExited)
                break;

            await Task.Delay(500);
        }

        // Collect diagnostics before throwing.
        var exited = _appProcess.HasExited;
        var exitCode = exited ? _appProcess.ExitCode.ToString() : "still running";

        throw new InvalidOperationException(
            $"DSCMS app did not become ready at {BaseUrl} within {AppStartTimeoutSec}s.\n" +
            $"  arguments: {arguments}\n" +
            $"  process exited: {exited} (exitCode={exitCode})\n" +
            $"--- STDOUT ---\n{stdoutBuilder}\n" +
            $"--- STDERR ---\n{stderrBuilder}");
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

    /// <summary>
    /// Finds and kills any process bound to the given port using PowerShell's
    /// Get-NetTCPConnection, which covers all TCP states (not just LISTENING).
    /// </summary>
    private static async Task KillProcessOnPortAsync(int port)
    {
        // Quick TCP probe — if nothing connects, the port is free and we're done.
        try
        {
            using var tcp = new TcpClient();
            await tcp.ConnectAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(1));
        }
        catch
        {
            return; // Port is free.
        }

        // Something is on the port — kill it via PowerShell (catches all TCP states).
        try
        {
            var ps = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -Command \"" +
                            $"Get-NetTCPConnection -LocalPort {port} -ErrorAction SilentlyContinue " +
                            $"| Select-Object -ExpandProperty OwningProcess " +
                            $"| Sort-Object -Unique " +
                            $"| ForEach-Object {{ Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            })!;
            await ps.WaitForExitAsync();

            // Give the OS a moment to release the socket.
            await Task.Delay(1000);
        }
        catch { /* best-effort */ }
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
