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
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ItemsPerPage { get; set; }


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
