using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using DSCMS.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSCMS.Tests.Services
{
    /// <summary>
    /// Exercises the real file system in a throwaway temp folder.
    /// </summary>
    public class StaticFileStorageServiceTests : IDisposable
    {
        private readonly string _tempRoot;
        private readonly StaticFileSettings _settings;
        private readonly StaticFileStorageService _service;

        public StaticFileStorageServiceTests()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "dscms-tests-" + Guid.NewGuid().ToString("N"));
            _settings = new StaticFileSettings
            {
                StoragePath = "static",
                MaxUploadBytes = 1024 * 1024,
                MaxExtractedBytes = 1024 * 1024,
                MaxArchiveEntries = 100
            };
            _service = new StaticFileStorageService(_settings, _tempRoot, NullLogger<StaticFileStorageService>.Instance);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, recursive: true);
            }
        }

        private string StagingRoot => _service.RootPath + "-staging";

        private static MemoryStream MakeZip(params (string Name, string Content)[] entries)
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (name, content) in entries)
                {
                    using var writer = new StreamWriter(archive.CreateEntry(name).Open());
                    writer.Write(content);
                }
            }
            stream.Position = 0;
            return stream;
        }

        private static MemoryStream MakeText(string content) => new MemoryStream(Encoding.UTF8.GetBytes(content));

        [Theory]
        [InlineData("Shapeshifter Game", "shapeshifter-game")]
        [InlineData("games/Shapeshifter", "games/shapeshifter")]
        [InlineData("/games//shapeshifter/", "games/shapeshifter")]
        [InlineData("games\\shapeshifter", "games/shapeshifter")]
        [InlineData("my_resume.v2", "my_resume.v2")]
        [InlineData("..", null)]
        [InlineData("../etc", null)]
        [InlineData("!!!", null)]
        [InlineData("", null)]
        [InlineData("   ", null)]
        [InlineData("a/b/c/d/e/f", null)]
        public void NormalizeFolderPath_HandlesInput(string input, string? expected)
        {
            Assert.Equal(expected, _service.NormalizeFolderPath(input));
        }

        [Fact]
        public async Task StageAndCommit_SingleFile_WritesFileWithHash()
        {
            var staged = await _service.StageAsync(MakeText("hello"), "My Resume.pdf", extractArchive: true);

            var file = Assert.Single(staged.Files);
            Assert.Equal("My-Resume.pdf", file.RelativePath);
            Assert.Equal("My-Resume.pdf", staged.EntryPoint);
            Assert.Equal("application/pdf", file.ContentType);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("hello"))).ToLowerInvariant(), file.Sha256);
            Assert.False(_service.FolderExistsOnDisk("resume"));

            _service.Commit(staged, "resume");

            Assert.Equal("hello", File.ReadAllText(Path.Combine(_service.RootPath, "resume", "My-Resume.pdf")));
            Assert.Empty(Directory.EnumerateFileSystemEntries(StagingRoot));
        }

        [Fact]
        public async Task Stage_ZipWithSingleRootFolder_FlattensIt()
        {
            using var zip = MakeZip(("shapeshifter/index.html", "<html>"), ("shapeshifter/js/game.js", "x"));

            var staged = await _service.StageAsync(zip, "shapeshifter.zip", extractArchive: true);

            Assert.True(staged.IsArchive);
            Assert.Equal(new[] { "index.html", "js/game.js" }, staged.Files.Select(f => f.RelativePath).OrderBy(p => p));
            Assert.Equal("index.html", staged.EntryPoint);
        }

        [Fact]
        public async Task Stage_ZipWithMacJunk_SkipsJunkEntries()
        {
            using var zip = MakeZip(("game/index.html", "<html>"), ("__MACOSX/game/._index.html", "junk"), ("game/.DS_Store", "junk"));

            var staged = await _service.StageAsync(zip, "game.zip", extractArchive: true);

            Assert.Equal("index.html", Assert.Single(staged.Files).RelativePath);
        }

        [Fact]
        public async Task Stage_ZipWithoutExtract_KeepsZipAsSingleFile()
        {
            using var zip = MakeZip(("index.html", "<html>"));

            var staged = await _service.StageAsync(zip, "bundle.zip", extractArchive: false);

            Assert.False(staged.IsArchive);
            Assert.Equal("bundle.zip", Assert.Single(staged.Files).RelativePath);
        }

        [Theory]
        [InlineData("../evil.txt")]
        [InlineData("game/../../evil.txt")]
        [InlineData("game/./evil.txt")]
        public async Task Stage_ZipSlipEntry_IsRejectedAndCleanedUp(string entryName)
        {
            using var zip = MakeZip(("index.html", "<html>"), (entryName, "evil"));

            await Assert.ThrowsAsync<StaticUploadException>(() => _service.StageAsync(zip, "evil.zip", extractArchive: true));

            Assert.Empty(Directory.EnumerateFileSystemEntries(StagingRoot));
            Assert.False(File.Exists(Path.Combine(_tempRoot, "evil.txt")));
        }

        [Fact]
        public async Task Stage_ZipOverExtractedSizeLimit_IsRejected()
        {
            _settings.MaxExtractedBytes = 10;
            using var zip = MakeZip(("a.txt", "123456"), ("b.txt", "789012"));

            await Assert.ThrowsAsync<StaticUploadException>(() => _service.StageAsync(zip, "big.zip", extractArchive: true));
            Assert.Empty(Directory.EnumerateFileSystemEntries(StagingRoot));
        }

        [Fact]
        public async Task Stage_ZipOverEntryLimit_IsRejected()
        {
            _settings.MaxArchiveEntries = 2;
            using var zip = MakeZip(("a.txt", "a"), ("b.txt", "b"), ("c.txt", "c"));

            await Assert.ThrowsAsync<StaticUploadException>(() => _service.StageAsync(zip, "many.zip", extractArchive: true));
        }

        [Fact]
        public async Task Stage_InvalidZip_IsRejected()
        {
            await Assert.ThrowsAsync<StaticUploadException>(() => _service.StageAsync(MakeText("not a zip"), "broken.zip", extractArchive: true));
            Assert.Empty(Directory.EnumerateFileSystemEntries(StagingRoot));
        }

        [Fact]
        public async Task Commit_ExistingFolder_ReplacesContents()
        {
            _service.Commit(await _service.StageAsync(MakeText("v1"), "old.pdf", true), "resume");

            _service.Commit(await _service.StageAsync(MakeText("v2"), "new.pdf", true), "resume");

            var folder = Path.Combine(_service.RootPath, "resume");
            Assert.Equal(new[] { "new.pdf" }, Directory.GetFiles(folder).Select(Path.GetFileName));
            Assert.Empty(Directory.EnumerateFileSystemEntries(StagingRoot));
        }

        [Fact]
        public async Task DeleteFolder_Nested_PrunesEmptyParents()
        {
            _service.Commit(await _service.StageAsync(MakeText("x"), "a.html", true), "games/shapeshifter");

            _service.DeleteFolder("games/shapeshifter");

            Assert.False(Directory.Exists(Path.Combine(_service.RootPath, "games")));
            Assert.True(Directory.Exists(_service.RootPath));
        }

        [Fact]
        public void BuildUrl_EscapesSegments()
        {
            Assert.Equal("/static/games/shapeshifter/", _service.BuildUrl("games/shapeshifter"));
            Assert.Equal("/static/games/shapeshifter/assets/My%20Sprite.png", _service.BuildUrl("games/shapeshifter", "assets/My Sprite.png"));
        }
    }
}
