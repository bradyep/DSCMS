using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;

namespace DSCMS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
  private readonly IUserRepository _userRepository;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ILogger<UsersController> _logger;

  public UsersController(
    IUserRepository userRepository,
    UserManager<ApplicationUser> userManager,
    ILogger<UsersController> logger)
  {
    _userRepository = userRepository;
    _userManager = userManager;
    _logger = logger;
  }

  // GET: api/users
  [HttpGet]
  public async Task<ActionResult<IEnumerable<UserListDto>>> GetAll()
  {
    _logger.LogDebug("API GetAll users requested");

    var users = await _userRepository.GetAllAsync();

    var dtos = users.Select(u => new UserListDto
    {
      Id = u.Id,
      DisplayName = u.DisplayName,
      Email = u.Email,
      UserName = u.UserName
    }).ToList();

    _logger.LogInformation("Returning {UserCount} users", dtos.Count);
    return Ok(dtos);
  }

  // GET: api/users/{id}
  [HttpGet("{id}")]
  public async Task<ActionResult<UserDetailDto>> GetById(string id)
  {
    _logger.LogDebug("API GetById requested for user id: {UserId}", id);

    var user = await _userRepository.GetByIdAsync(id);
    if (user == null)
    {
      _logger.LogWarning("User not found with id: {UserId}", id);
      return NotFound();
    }

    var dto = new UserDetailDto
    {
      Id = user.Id,
      DisplayName = user.DisplayName,
      Email = user.Email,
      UserName = user.UserName
    };

    _logger.LogDebug("Found user: {UserId} - {UserName}", user.Id, user.UserName);
    return Ok(dto);
  }

  // POST: api/users
  [HttpPost]
  public async Task<ActionResult<UserDetailDto>> Create([FromBody] UserCreateDto dto)
  {
    _logger.LogDebug("API Create user requested for email: {Email}", dto.Email);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for user creation: {Email}", dto.Email);
      return BadRequest(ModelState);
    }

    var user = new ApplicationUser
    {
      DisplayName = dto.DisplayName,
      Email = dto.Email,
      UserName = string.IsNullOrEmpty(dto.UserName) ? dto.Email : dto.UserName
    };

    var result = await _userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded)
    {
      _logger.LogWarning("User creation failed for {Email}: {Errors}", dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
      var errors = result.Errors.Select(e => e.Description).ToList();
      return BadRequest(new { Errors = errors });
    }

    _logger.LogInformation("Created new user: {UserId} - {UserName}", user.Id, user.UserName);

    var resultDto = new UserDetailDto
    {
      Id = user.Id,
      DisplayName = user.DisplayName,
      Email = user.Email,
      UserName = user.UserName
    };

    return CreatedAtAction(nameof(GetById), new { id = user.Id }, resultDto);
  }

  // PUT: api/users/{id}
  [HttpPut("{id}")]
  public async Task<ActionResult<UserDetailDto>> Update(string id, [FromBody] UserUpdateDto dto)
  {
    _logger.LogDebug("API Update user requested for: {UserId} - {Email}", id, dto.Email);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for user update: {UserId}", id);
      return BadRequest(ModelState);
    }

    var existingUser = await _userManager.FindByIdAsync(id);
    if (existingUser == null)
    {
      _logger.LogWarning("User not found for update with id: {UserId}", id);
      return NotFound();
    }

    existingUser.DisplayName = dto.DisplayName;
    existingUser.Email = dto.Email;
    existingUser.UserName = string.IsNullOrEmpty(dto.UserName) ? dto.Email : dto.UserName;

    try
    {
      var result = await _userManager.UpdateAsync(existingUser);
      if (!result.Succeeded)
      {
        _logger.LogWarning("User update failed for {UserId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));
        var errors = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(new { Errors = errors });
      }

      _logger.LogInformation("Updated user: {UserId} - {UserName}", existingUser.Id, existingUser.UserName);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogError(ex, "Concurrency exception updating user: {UserId}", id);
      if (!await _userRepository.ExistsAsync(id))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }

    var resultDto = new UserDetailDto
    {
      Id = existingUser.Id,
      DisplayName = existingUser.DisplayName,
      Email = existingUser.Email,
      UserName = existingUser.UserName
    };

    return Ok(resultDto);
  }

  // DELETE: api/users/{id}
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
    _logger.LogDebug("API Delete requested for user id: {UserId}", id);

    var user = await _userRepository.GetByIdAsync(id);
    if (user == null)
    {
      _logger.LogWarning("User not found for delete with id: {UserId}", id);
      return NotFound();
    }

    var result = await _userManager.DeleteAsync(user);
    if (!result.Succeeded)
    {
      _logger.LogWarning("User deletion failed for {UserId}: {Errors}", id, string.Join(", ", result.Errors.Select(e => e.Description)));
      var errors = result.Errors.Select(e => e.Description).ToList();
      return BadRequest(new { Errors = errors });
    }

    _logger.LogInformation("Deleted user: {UserId} - {UserName}", user.Id, user.UserName);

    return NoContent();
  }
}