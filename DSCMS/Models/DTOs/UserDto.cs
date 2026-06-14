using System.ComponentModel.DataAnnotations;

namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for user list display (Index page)
/// </summary>
public class UserListDto
{
  public string Id { get; set; } = string.Empty;
  public string? DisplayName { get; set; }
  public string? Email { get; set; }
  public string? UserName { get; set; }
}

/// <summary>
/// DTO for detailed user view
/// </summary>
public class UserDetailDto
{
  public string Id { get; set; } = string.Empty;
  public string? DisplayName { get; set; }
  public string? Email { get; set; }
  public string? UserName { get; set; }
}

/// <summary>
/// DTO for creating new user
/// </summary>
public class UserCreateDto
{
  public string? DisplayName { get; set; }

  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  public string? UserName { get; set; }

  [Required]
  public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating existing user
/// </summary>
public class UserUpdateDto
{
  public string? DisplayName { get; set; }

  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  public string? UserName { get; set; }
}
