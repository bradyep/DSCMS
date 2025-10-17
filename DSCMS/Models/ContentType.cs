using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
    /// <summary>
    /// Defines a certain type of content, such as "Blog Post" or "News Article". 
    /// </summary>
    public class ContentType
  {
    public int ContentTypeId { get; set; }
    /// <summary>
    /// Used for url when displaying multiple contents of this type.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Displays a title for this content type.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Can be used to display a paragraph for this content type.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the number of items to display per page in a paginated list. If set to 0 displays all items on a single page.
    /// </summary>
    public int ItemsPerPage { get; set; }

    /// <summary>
    /// Template used to display multiple contents
    /// </summary>
    [Display(Name = "Multiple Contents Template")]
    public int MultipleContentsTemplateId { get; set; }
    [ForeignKey("MultipleContentsTemplateId")]
    public Template? MultipleContentsTemplate { get; set; }
    
    /// <summary>
    /// Default template to use if no template is specified when creating content of this type.
    /// </summary>
    [Display(Name = "Default Single Content Template")]
    public int? DefaultSingleContentTemplateId { get; set; }
    [ForeignKey("DefaultSingleContentTemplateId")]
    public Template? DefaultSingleContentTemplate { get; set; }
    
    /// <summary>
    /// Indicates if this is the default content type for the system.
    /// </summary>
    [Display(Name = "Is Default Content Type")]
    public bool IsDefaultContentType { get; set; } = false;

    public List<ContentTypeField> ContentTypeFields { get; set; } = new List<ContentTypeField>();
    public List<Content> Contents { get; set; } = new List<Content>();
  }
}
