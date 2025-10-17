using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  /// <summary>
  /// Represents an actual value for a content type field (e.g., blog post teaser text "my first post").
  /// </summary>
  public class ContentTypeFieldItem
  {
    public int ContentTypeFieldItemId { get; set; }
    public string? Value { get; set; }

    [Display(Name = "Content Type Field")]
    public int ContentTypeFieldId { get; set; }
    public ContentTypeField? ContentTypeField { get; set; }
    
    [Display(Name = "Content")]
    public int ContentId { get; set; }
    public Content? Content { get; set; }
  }
}
