using System.Text;
using DSCMS.Middleware;
using DSCMS.Models;
using DSCMS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DSCMS.Tests.Middleware
{
    /// <summary>
    /// Runs the middleware against real files in a throwaway temp folder, with the upload index and CMS lookup mocked.
    /// </summary>
    public class StaticUploadMiddlewareTests : IDisposable
    {
        private const int PassedOnStatus = 299;

        private readonly string _tempRoot;
        private readonly StaticFileStorageService _storage;
        private readonly Mock<IStaticUploadRegistry> _registry = new();
        private readonly Mock<ICmsPageLookup> _cmsPages = new();
        private readonly StaticUploadMiddleware _middleware;
        private PathString? _pathSeenByNext;

        public StaticUploadMiddlewareTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "dscms-tests-" + Guid.NewGuid().ToString("N"));
            _storage = new StaticFileStorageService(new StaticFileSettings { StoragePath = "static" }, _tempRoot, NullLogger<StaticFileStorageService>.Instance);

            AddUpload("games/shapeshifter", ("index.html", "game page"), ("assets/Title.TXT", "title"));
            AddUpload("ruffle", ("index.html", "ruffle page"), ("ruffle.js", "js"));
            AddUpload("images/logos", ("logo.txt", "uploaded logo"));
            // Listed in the database, but missing on disk
            _uploads.Add(new StaticUpload { FolderPath = "gone", Files = { new StaticUploadFile { RelativePath = "file.txt" } } });
            _registry.Setup(r => r.GetIndexAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => StaticUploadIndex.Build(_uploads));

            _middleware = new StaticUploadMiddleware(
                context =>
                {
                    _pathSeenByNext = context.Request.Path;
                    context.Response.StatusCode = PassedOnStatus;
                    return Task.CompletedTask;
                },
                _registry.Object, _storage, new Mock<IWebHostEnvironment>().Object, NullLoggerFactory.Instance);
        }

        private readonly List<StaticUpload> _uploads = new();

        private void AddUpload(string folder, params (string Path, string Content)[] files)
        {
            var upload = new StaticUpload { FolderPath = folder };
            foreach (var (path, content) in files)
            {
                var fullPath = Path.Combine(_storage.RootPath, folder, path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.WriteAllText(fullPath, content);
                upload.Files.Add(new StaticUploadFile { RelativePath = path });
            }
            _uploads.Add(upload);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, recursive: true);
            }
        }

        private async Task<HttpContext> SendAsync(string path, string method = "GET", string? range = null, string query = "")
        {
            var services = new ServiceCollection();
            services.AddScoped(_ => _cmsPages.Object);

            var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
            context.Request.Method = method;
            context.Request.Path = path;
            context.Request.QueryString = new QueryString(query);
            if (range != null)
            {
                context.Request.Headers.Range = range;
            }
            context.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(context);
            return context;
        }

        private static string Body(HttpContext context)
        {
            context.Response.Body.Position = 0;
            return new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEnd();
        }

        [Fact]
        public async Task File_IsServedAtItsFolderPath()
        {
            var context = await SendAsync("/games/shapeshifter/assets/Title.TXT");

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("title", Body(context));
        }

        [Fact]
        public async Task File_MatchesIgnoringCase()
        {
            var context = await SendAsync("/GAMES/Shapeshifter/ASSETS/title.txt");

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("title", Body(context));
            Assert.Equal("/GAMES/Shapeshifter/ASSETS/title.txt", context.Request.Path.Value);
        }

        [Fact]
        public async Task File_IsServedUnderStaticAlias()
        {
            var context = await SendAsync("/static/games/shapeshifter/assets/Title.TXT");

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("title", Body(context));
        }

        [Fact]
        public async Task DeepPaths_DoNotQueryCmsPages()
        {
            _cmsPages.Setup(c => c.IsCmsPageAsync(It.IsAny<string>())).ReturnsAsync(true);

            var context = await SendAsync("/games/shapeshifter/assets/Title.TXT");

            Assert.Equal(200, context.Response.StatusCode);
            _cmsPages.Verify(c => c.IsCmsPageAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("/games/shapeshifter")]
        [InlineData("/games/shapeshifter/")]
        public async Task CmsPage_WinsOverUploadFolder(string path)
        {
            _cmsPages.Setup(c => c.IsCmsPageAsync("games/shapeshifter")).ReturnsAsync(true);

            var context = await SendAsync(path);

            Assert.Equal(PassedOnStatus, context.Response.StatusCode);
        }

        [Fact]
        public async Task CmsPage_DoesNotAffectStaticAlias()
        {
            _cmsPages.Setup(c => c.IsCmsPageAsync(It.IsAny<string>())).ReturnsAsync(true);

            var context = await SendAsync("/static/games/shapeshifter/");

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("game page", Body(context));
        }

        [Fact]
        public async Task Folder_WithoutTrailingSlash_RedirectsKeepingQuery()
        {
            var context = await SendAsync("/ruffle", query: "?x=1");

            Assert.Equal(301, context.Response.StatusCode);
            Assert.Equal("/ruffle/?x=1", context.Response.Headers.Location.ToString());
        }

        [Fact]
        public async Task Folder_WithTrailingSlash_ServesIndex()
        {
            var context = await SendAsync("/ruffle/");

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("ruffle page", Body(context));
        }

        [Fact]
        public async Task Upload_WinsOverWwwrootPath()
        {
            // "images" is a wwwroot folder; nothing stops an upload from living under it, and the upload is served first
            var context = await SendAsync("/images/logos/logo.txt");

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("uploaded logo", Body(context));
        }

        [Theory]
        [InlineData("/css/site.css")]
        [InlineData("/games")]
        [InlineData("/")]
        [InlineData("/ruffle/missing.js")]
        [InlineData("/games/shapeshifter/assets/Title.TXT/")]
        public async Task OtherPaths_PassThrough(string path)
        {
            var context = await SendAsync(path);

            Assert.Equal(PassedOnStatus, context.Response.StatusCode);
        }

        [Fact]
        public async Task NonGetRequests_PassThrough()
        {
            var context = await SendAsync("/ruffle/ruffle.js", method: "POST");

            Assert.Equal(PassedOnStatus, context.Response.StatusCode);
        }

        [Fact]
        public async Task FileMissingOnDisk_PassesThroughWithOriginalPath()
        {
            var context = await SendAsync("/Gone/file.txt");

            Assert.Equal(PassedOnStatus, context.Response.StatusCode);
            Assert.Equal("/Gone/file.txt", _pathSeenByNext?.Value);
        }

        [Fact]
        public async Task RangeRequest_ReturnsPartialContent()
        {
            var context = await SendAsync("/ruffle/index.html", range: "bytes=0-5");

            Assert.Equal(206, context.Response.StatusCode);
            Assert.Equal("ruffle", Body(context));
        }
    }
}
