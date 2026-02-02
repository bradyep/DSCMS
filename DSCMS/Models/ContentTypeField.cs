using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  /// <summary>
  /// Represents a field associated with a specific content type (such as teaser text for a blog post).
  /// </summary>
  public class ContentTypeField
  {
    public int ContentTypeFieldId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Required")]
    public bool Required { get; set; } = false;

    [Display(Name = "Content Type")]
    public int ContentTypeId { get; set; }
    public ContentType? ContentType { get; set; }
    public List<ContentTypeFieldItem> ContentTypeFieldItems { get; set; } = new List<ContentTypeFieldItem>();
  }
}
