using System.Security.Claims;
using DSCMS.Controllers;
using DSCMS.Models.DTOs;
using DSCMS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
  public class BackupControllerTests
  {
    private readonly Mock<IBackupService> _backupService;
    private readonly Mock<IWebHostEnvironment> _environment;
    private readonly BackupController _controller;

    public BackupControllerTests()
    {
      _backupService = new Mock<IBackupService>();
      _environment = new Mock<IWebHostEnvironment>();
      _environment.Setup(e => e.EnvironmentName).Returns("Development");

      var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }, "test"));
      _controller = new BackupController(_backupService.Object, _environment.Object, new Mock<ILogger<BackupController>>().Object)
      {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
      };
    }

    private static IFormFile MakeFormFile(string fileName, int length = 10)
    {
      return new FormFile(new MemoryStream(new byte[length]), 0, length, "file", fileName);
    }

    [Fact]
    public async Task GetStatus_ReturnsServiceStatus()
    {
      var status = new BackupStatusDto { Counts = new SiteCountsDto { Contents = 3 } };
      _backupService.Setup(s => s.GetStatusAsync(It.IsAny<CancellationToken>())).ReturnsAsync(status);

      var result = await _controller.GetStatus(CancellationToken.None);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      Assert.Same(status, ok.Value);
    }

    [Fact]
    public async Task Download_WhenBusy_ReturnsConflict()
    {
      _backupService.Setup(s => s.CreateBackupAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new BackupBusyException());

      var result = await _controller.Download(CancellationToken.None);

      Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Download_ReturnsZipNamedForEnvironment()
    {
      var path = Path.GetTempFileName();
      _backupService.Setup(s => s.CreateBackupAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(() => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose));

      var result = await _controller.Download(CancellationToken.None);

      var file = Assert.IsType<FileStreamResult>(result);
      Assert.Equal("application/zip", file.ContentType);
      Assert.StartsWith("dscms-backup-development-", file.FileDownloadName);
      file.FileStream.Dispose();
    }

    [Fact]
    public async Task StageRestore_WithoutFile_ReturnsBadRequest()
    {
      var result = await _controller.StageRestore(null, CancellationToken.None);

      Assert.IsType<BadRequestObjectResult>(result.Result);
      _backupService.Verify(s => s.StageRestoreAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StageRestore_InvalidBackup_ReturnsBadRequestWithMessage()
    {
      _backupService.Setup(s => s.StageRestoreAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new BackupException("This is not a DSCMS backup."));

      var result = await _controller.StageRestore(MakeFormFile("backup.zip"), CancellationToken.None);

      var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
      Assert.Contains("not a DSCMS backup", bad.Value!.ToString());
    }

    [Fact]
    public async Task StageRestore_ReturnsPreview()
    {
      var preview = new RestorePreviewDto { Token = "abc", FileName = "backup.zip" };
      _backupService.Setup(s => s.StageRestoreAsync(It.IsAny<Stream>(), "backup.zip", It.IsAny<CancellationToken>())).ReturnsAsync(preview);

      var result = await _controller.StageRestore(MakeFormFile("backup.zip"), CancellationToken.None);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      Assert.Same(preview, ok.Value);
    }

    [Fact]
    public async Task CommitRestore_PassesCurrentUserId()
    {
      _backupService.Setup(s => s.CommitRestoreAsync("abc", "user-1", It.IsAny<CancellationToken>()))
        .ReturnsAsync(new RestoreResultDto { SnapshotName = "pre-restore-1.zip" });

      var result = await _controller.CommitRestore("abc", CancellationToken.None);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      Assert.Equal("pre-restore-1.zip", ((RestoreResultDto)ok.Value!).SnapshotName);
    }

    [Fact]
    public async Task CommitRestore_ExpiredToken_ReturnsBadRequest()
    {
      _backupService.Setup(s => s.CommitRestoreAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new BackupException("This restore has expired."));

      var result = await _controller.CommitRestore("old", CancellationToken.None);

      Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CommitRestore_WhenBusy_ReturnsConflict()
    {
      _backupService.Setup(s => s.CommitRestoreAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new BackupBusyException());

      var result = await _controller.CommitRestore("abc", CancellationToken.None);

      Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public void DownloadSnapshot_Unknown_ReturnsNotFound()
    {
      _backupService.Setup(s => s.GetSnapshotPath("nope.zip")).Returns((string?)null);

      Assert.IsType<NotFoundResult>(_controller.DownloadSnapshot("nope.zip"));
    }

    [Fact]
    public void DiscardRestore_DiscardsToken()
    {
      Assert.IsType<NoContentResult>(_controller.DiscardRestore("abc"));
      _backupService.Verify(s => s.DiscardStagedRestore("abc"), Times.Once);
    }
  }
}
