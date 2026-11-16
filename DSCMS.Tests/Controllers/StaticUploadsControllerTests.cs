using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using DSCMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class StaticUploadsControllerTests
    {
        private readonly Mock<IStaticUploadRepository> _uploadRepo;
        private readonly Mock<IStaticFileStorageService> _storage;
        private readonly Mock<ILogger<StaticUploadsController>> _logger;
        private readonly StaticUploadsController _controller;

        public StaticUploadsControllerTests()
        {
            _uploadRepo = new Mock<IStaticUploadRepository>();
            _storage = new Mock<IStaticFileStorageService>();
            _logger = new Mock<ILogger<StaticUploadsController>>();

            _storage.Setup(s => s.NormalizeFolderPath(It.IsAny<string?>()))
                .Returns((string? p) => string.IsNullOrWhiteSpace(p) || p.Contains("..") ? null : p.Trim('/').ToLowerInvariant());
            _storage.Setup(s => s.BuildUrl(It.IsAny<string>(), It.IsAny<string?>()))
                .Returns((string folder, string? file) => $"/static/{folder}/{file}");
            _uploadRepo.Setup(r => r.GetAllFolderPathsAsync()).ReturnsAsync(new List<string>());

            _controller = new StaticUploadsController(_uploadRepo.Object, _storage.Object, _logger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        private static IFormFile MakeFormFile(string fileName, int length = 10)
        {
            return new FormFile(new MemoryStream(new byte[length]), 0, length, "file", fileName);
        }

        private static StagedUpload MakeStaged(bool isArchive = false, params string[] paths)
        {
            return new StagedUpload
            {
                StagingPath = "staging",
                IsArchive = isArchive,
                EntryPoint = paths.FirstOrDefault(),
                Files = paths.Select(p => new StagedFile { RelativePath = p, SizeBytes = 5, Sha256 = "abc", ContentType = "text/html" }).ToList()
            };
        }

        private void SetupStage(StagedUpload staged)
        {
            _storage.Setup(s => s.StageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(staged);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithUploads()
        {
            var uploads = new List<StaticUpload>
            {
                new StaticUpload { StaticUploadId = 1, FolderPath = "resume", EntryPoint = "resume.pdf", FileCount = 1 }
            };
            _uploadRepo.Setup(r => r.GetAllWithUploaderAsync()).ReturnsAsync(uploads);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.Single(Assert.IsAssignableFrom<IEnumerable<StaticUploadListDto>>(ok.Value));
            Assert.Equal("/static/resume/resume.pdf", dto.EntryUrl);
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _uploadRepo.Setup(r => r.GetByIdWithFilesAsync(99)).ReturnsAsync((StaticUpload?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_Found_ReturnsFilesWithUrls()
        {
            var upload = new StaticUpload
            {
                StaticUploadId = 1,
                FolderPath = "games/shapeshifter",
                Files = new List<StaticUploadFile> { new StaticUploadFile { RelativePath = "index.html" } }
            };
            _uploadRepo.Setup(r => r.GetByIdWithFilesAsync(1)).ReturnsAsync(upload);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<StaticUploadDetailDto>(ok.Value);
            Assert.Equal("/static/games/shapeshifter/index.html", Assert.Single(dto.Files).Url);
        }

        [Fact]
        public async Task Create_NoFile_ReturnsBadRequest()
        {
            var result = await _controller.Create(null, "resume", null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_InvalidFolder_ReturnsBadRequest()
        {
            var result = await _controller.Create(MakeFormFile("resume.pdf"), "../etc", null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _storage.Verify(s => s.StageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("resume")]
        [InlineData("games")]
        [InlineData("games/shapeshifter/extra")]
        public async Task Create_FolderOverlapsExistingUpload_ReturnsConflict(string folder)
        {
            _uploadRepo.Setup(r => r.GetAllFolderPathsAsync()).ReturnsAsync(new List<string> { "resume", "games/shapeshifter" });

            var result = await _controller.Create(MakeFormFile("game.zip"), folder, null);

            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_FolderExistsOnDiskOnly_ReturnsConflict()
        {
            _storage.Setup(s => s.FolderExistsOnDisk("resume")).Returns(true);

            var result = await _controller.Create(MakeFormFile("resume.pdf"), "resume", null);

            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_StageRejected_ReturnsBadRequestWithoutCommitting()
        {
            _storage.Setup(s => s.StageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new StaticUploadException("bad zip"));

            var result = await _controller.Create(MakeFormFile("game.zip"), "game", null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _storage.Verify(s => s.Commit(It.IsAny<StagedUpload>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Create_NoFolderGiven_DefaultsToFileName()
        {
            SetupStage(MakeStaged(false, "Resume.pdf"));

            var result = await _controller.Create(MakeFormFile("Resume.pdf"), null, null);

            Assert.IsType<CreatedAtActionResult>(result.Result);
            _storage.Verify(s => s.Commit(It.IsAny<StagedUpload>(), "resume"), Times.Once);
        }

        [Fact]
        public async Task Create_Valid_CommitsSavesAndReturnsCreated()
        {
            var staged = MakeStaged(true, "index.html", "js/game.js");
            SetupStage(staged);
            StaticUpload? saved = null;
            _uploadRepo.Setup(r => r.AddAsync(It.IsAny<StaticUpload>()))
                .Callback<StaticUpload>(u => saved = u)
                .Returns(Task.CompletedTask);

            var result = await _controller.Create(MakeFormFile("shapeshifter.zip"), "games/shapeshifter", "A game");

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), created.ActionName);
            _storage.Verify(s => s.Commit(staged, "games/shapeshifter"), Times.Once);
            Assert.NotNull(saved);
            Assert.Equal("games/shapeshifter", saved!.FolderPath);
            Assert.True(saved.IsArchive);
            Assert.Equal(2, saved.FileCount);
            Assert.Equal(10, saved.TotalSizeBytes);
            Assert.Equal("index.html", saved.EntryPoint);
            Assert.Equal("A game", saved.Description);
        }

        [Fact]
        public async Task Create_DatabaseSaveFails_RemovesCommittedFolder()
        {
            SetupStage(MakeStaged(false, "resume.pdf"));
            _uploadRepo.Setup(r => r.AddAsync(It.IsAny<StaticUpload>())).ThrowsAsync(new InvalidOperationException("db down"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(MakeFormFile("resume.pdf"), "resume", null));

            _storage.Verify(s => s.DeleteFolder("resume"), Times.Once);
        }

        [Fact]
        public async Task ReplaceFiles_NotFound_ReturnsNotFound()
        {
            _uploadRepo.Setup(r => r.GetByIdWithFilesAsync(99)).ReturnsAsync((StaticUpload?)null);

            var result = await _controller.ReplaceFiles(99, MakeFormFile("resume.pdf"));

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task ReplaceFiles_Valid_CommitsToSameFolderAndReplacesRecords()
        {
            var upload = new StaticUpload
            {
                StaticUploadId = 1,
                FolderPath = "resume",
                OriginalFileName = "old.pdf",
                Files = new List<StaticUploadFile> { new StaticUploadFile { RelativePath = "old.pdf" } }
            };
            _uploadRepo.Setup(r => r.GetByIdWithFilesAsync(1)).ReturnsAsync(upload);
            var staged = MakeStaged(false, "new.pdf");
            SetupStage(staged);

            var result = await _controller.ReplaceFiles(1, MakeFormFile("new.pdf"));

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<StaticUploadDetailDto>(ok.Value);
            Assert.Equal("new.pdf", Assert.Single(dto.Files).RelativePath);
            Assert.NotNull(upload.LastReplacedAt);
            _storage.Verify(s => s.Commit(staged, "resume"), Times.Once);
            _uploadRepo.Verify(r => r.ReplaceFilesAsync(upload, It.Is<List<StaticUploadFile>>(f => f.Single().RelativePath == "new.pdf")), Times.Once);
        }

        [Fact]
        public async Task Update_ChangesDescription()
        {
            var upload = new StaticUpload { StaticUploadId = 1, FolderPath = "resume" };
            _uploadRepo.Setup(r => r.GetByIdWithFilesAsync(1)).ReturnsAsync(upload);

            var result = await _controller.Update(1, new StaticUploadUpdateDto { Description = "  My resume " });

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("My resume", upload.Description);
            _uploadRepo.Verify(r => r.UpdateAsync(upload), Times.Once);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _uploadRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StaticUpload?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Found_DeletesRecordAndFolder()
        {
            var upload = new StaticUpload { StaticUploadId = 1, FolderPath = "games/shapeshifter" };
            _uploadRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(upload);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
            _uploadRepo.Verify(r => r.DeleteAsync(upload), Times.Once);
            _storage.Verify(s => s.DeleteFolder("games/shapeshifter"), Times.Once);
        }
    }
}
