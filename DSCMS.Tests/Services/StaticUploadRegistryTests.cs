using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using DSCMS.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DSCMS.Tests.Services
{
    public class StaticUploadRegistryTests
    {
        private static StaticUpload MakeUpload(string folder, params string[] files)
        {
            return new StaticUpload
            {
                FolderPath = folder,
                Files = files.Select(f => new StaticUploadFile { RelativePath = f }).ToList()
            };
        }

        [Fact]
        public void Index_MatchesFilesIgnoringCase_AndReturnsOnDiskCase()
        {
            var index = StaticUploadIndex.Build(new[] { MakeUpload("games/idemon", "iDemon/Title.MP3", "js/game.js") });

            Assert.True(index.TryGetFile("games/iDemon/idemon/title.mp3", out var path));
            Assert.Equal("games/idemon/iDemon/Title.MP3", path);
            Assert.True(index.TryGetFile("GAMES/IDEMON/JS/GAME.JS", out _));
            Assert.False(index.TryGetFile("games/idemon/js", out _));
            Assert.False(index.TryGetFile("games/idemon/missing.js", out _));
        }

        [Fact]
        public void Index_FindsDefaultDocuments_InUploadAndSubfolders()
        {
            var index = StaticUploadIndex.Build(new[]
            {
                MakeUpload("ruffle", "Index.html", "ruffle.js", "docs/index.htm"),
                MakeUpload("resume", "resume.pdf")
            });

            Assert.True(index.TryGetDefaultDocument("Ruffle", out var root));
            Assert.Equal("ruffle/Index.html", root);
            Assert.True(index.TryGetDefaultDocument("ruffle/docs", out var docs));
            Assert.Equal("ruffle/docs/index.htm", docs);
            Assert.False(index.TryGetDefaultDocument("resume", out _));
        }

        [Fact]
        public void Index_PrefersDefaultDocumentsInDefaultFilesOrder()
        {
            var index = StaticUploadIndex.Build(new[] { MakeUpload("site", "index.html", "index.htm", "default.html") });

            Assert.True(index.TryGetDefaultDocument("site", out var path));
            Assert.Equal("site/default.html", path);
        }

        private static (StaticUploadRegistry Registry, Mock<IStaticUploadRepository> Repo) MakeRegistry()
        {
            var repo = new Mock<IStaticUploadRepository>();
            var services = new ServiceCollection();
            services.AddScoped(_ => repo.Object);
            var provider = services.BuildServiceProvider();
            var registry = new StaticUploadRegistry(provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<StaticUploadRegistry>.Instance);
            return (registry, repo);
        }

        [Fact]
        public async Task Registry_CachesUntilInvalidated()
        {
            var (registry, repo) = MakeRegistry();
            repo.Setup(r => r.GetAllWithFilesAsync()).ReturnsAsync(new List<StaticUpload> { MakeUpload("resume", "resume.pdf") });

            var first = await registry.GetIndexAsync();
            var second = await registry.GetIndexAsync();
            Assert.Same(first, second);
            repo.Verify(r => r.GetAllWithFilesAsync(), Times.Once);

            repo.Setup(r => r.GetAllWithFilesAsync()).ReturnsAsync(new List<StaticUpload>());
            registry.Invalidate();

            var third = await registry.GetIndexAsync();
            Assert.False(third.TryGetFile("resume/resume.pdf", out _));
            repo.Verify(r => r.GetAllWithFilesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task Registry_LoadFails_ReturnsEmptyAndRetriesNextTime()
        {
            var (registry, repo) = MakeRegistry();
            repo.Setup(r => r.GetAllWithFilesAsync()).ThrowsAsync(new InvalidOperationException("no such table"));

            var index = await registry.GetIndexAsync();
            Assert.Equal(0, index.FileCount);

            repo.Setup(r => r.GetAllWithFilesAsync()).ReturnsAsync(new List<StaticUpload> { MakeUpload("resume", "resume.pdf") });
            index = await registry.GetIndexAsync();
            Assert.True(index.TryGetFile("resume/resume.pdf", out _));
        }
    }
}
