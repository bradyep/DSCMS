using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  public class User
  {
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; }

    [InverseProperty("CreatedByUser")]
    public List<Content> CreatedContent { get; set; }
    [InverseProperty("LastUpdatedByUser")]
    public List<Content> UpdatedContent { get; set; }
  }
}
