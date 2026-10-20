using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSCMS.Models
{
  public class LayoutValue
  {
    public int LayoutValueId { get; set; }

    [Required, StringLength(100)]
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public int LayoutId { get; set; }
    [ForeignKey("LayoutId")]
    public Layout? Layout { get; set; }
  }
}
