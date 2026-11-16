using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DSCMS.Filters;
using DSCMS.Models.DTOs;
using DSCMS.Services;

namespace DSCMS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
  private readonly IBackupService _backupService;
  private readonly IWebHostEnvironment _environment;
  private readonly ILogger<BackupController> _logger;

  public BackupController(IBackupService backupService, IWebHostEnvironment environment, ILogger<BackupController> logger)
  {
    _backupService = backupService;
    _environment = environment;
    _logger = logger;
  }

  // GET: api/backup/status
  [HttpGet("status")]
  public async Task<ActionResult<BackupStatusDto>> GetStatus(CancellationToken cancellationToken)
  {
    _logger.LogDebug("API backup status requested");
    return Ok(await _backupService.GetStatusAsync(cancellationToken));
  }

  // GET: api/backup/download
  [HttpGet("download")]
  public async Task<IActionResult> Download(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Backup download requested by {User}", User.Identity?.Name);

    FileStream backup;
    try
    {
      backup = await _backupService.CreateBackupAsync(cancellationToken);
    }
    catch (BackupBusyException ex)
    {
      return Conflict(new { error = ex.Message });
    }
    catch (BackupException ex)
    {
      _logger.LogWarning("Backup failed: {Reason}", ex.Message);
      return BadRequest(new { error = ex.Message });
    }

    var fileName = $"dscms-backup-{_environment.EnvironmentName.ToLowerInvariant()}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
    return File(backup, "application/zip", fileName);
  }

  // POST: api/backup/restore (multipart/form-data) - checks the backup and stages it; nothing is changed yet
  [HttpPost("restore")]
  [BackupRestoreSizeLimit]
  public async Task<ActionResult<RestorePreviewDto>> StageRestore(IFormFile? file, CancellationToken cancellationToken)
  {
    _logger.LogDebug("API stage restore requested: {FileName}", file?.FileName);

    if (file == null || file.Length == 0)
    {
      return BadRequest(new { error = "Please choose a backup file to upload." });
    }

    await using var stream = file.OpenReadStream();
    return await StageAsync(() => _backupService.StageRestoreAsync(stream, file.FileName, cancellationToken));
  }

  // POST: api/backup/snapshots/{name}/restore - stages a snapshot saved on the server
  [HttpPost("snapshots/{name}/restore")]
  public async Task<ActionResult<RestorePreviewDto>> StageSnapshot(string name, CancellationToken cancellationToken)
  {
    _logger.LogDebug("API stage snapshot restore requested: {SnapshotName}", name);
    return await StageAsync(() => _backupService.StageSnapshotAsync(name, cancellationToken));
  }

  // POST: api/backup/restore/{token}/commit
  [HttpPost("restore/{token}/commit")]
  public async Task<ActionResult<RestoreResultDto>> CommitRestore(string token, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Restore commit requested by {User}", User.Identity?.Name);

    try
    {
      var result = await _backupService.CommitRestoreAsync(token, User.FindFirstValue(ClaimTypes.NameIdentifier), cancellationToken);
      return Ok(result);
    }
    catch (BackupBusyException ex)
    {
      return Conflict(new { error = ex.Message });
    }
    catch (BackupException ex)
    {
      _logger.LogWarning("Restore failed: {Reason}", ex.Message);
      return BadRequest(new { error = ex.Message });
    }
  }

  // DELETE: api/backup/restore/{token} - throws away a staged restore
  [HttpDelete("restore/{token}")]
  public IActionResult DiscardRestore(string token)
  {
    _backupService.DiscardStagedRestore(token);
    return NoContent();
  }

  // GET: api/backup/snapshots/{name}
  [HttpGet("snapshots/{name}")]
  public IActionResult DownloadSnapshot(string name)
  {
    var path = _backupService.GetSnapshotPath(name);
    if (path == null)
    {
      _logger.LogWarning("Snapshot not found: {SnapshotName}", name);
      return NotFound();
    }

    return PhysicalFile(path, "application/zip", name);
  }

  // -------------------------------------------------------------------------

  private async Task<ActionResult<RestorePreviewDto>> StageAsync(Func<Task<RestorePreviewDto>> stage)
  {
    try
    {
      return Ok(await stage());
    }
    catch (BackupBusyException ex)
    {
      return Conflict(new { error = ex.Message });
    }
    catch (BackupException ex)
    {
      _logger.LogWarning("Backup rejected: {Reason}", ex.Message);
      return BadRequest(new { error = ex.Message });
    }
  }
}
