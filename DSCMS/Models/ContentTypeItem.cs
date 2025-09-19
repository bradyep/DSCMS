using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  /// <summary>
  /// Represents a type of item associated with a specific content type (such as teaser text for a blog post), including its metadata and related content.
  /// </summary>
  /// <remarks>This class is used to define and manage items that belong to a particular content type.  It
  /// includes properties for identifying the item, associating it with a content type, and linking related content
  /// items.</remarks>
  public class ContentTypeItem
  {
    public int ContentTypeItemId { get; set; }
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Content Type")]
    public int ContentTypeId { get; set; }
    public ContentType? ContentType { get; set; }
    public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
  }
}
