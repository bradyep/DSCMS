using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  /// <summary>
  /// Represents an actual child item of content (eg. blog post teaser text "my first post"). Associated with a specific content type and content field type.
  /// </summary>
  public class ContentItem
  {
    public int ContentItemId { get; set; }
    public string? Value { get; set; }

    [Display(Name = "Content Type Field")]
    public int ContentTypeFieldId { get; set; }
    public ContentTypeField? ContentTypeField { get; set; }
    [Display(Name = "Content")]
    public int ContentId { get; set; }
    public Content? Content { get; set; }
  }
}
