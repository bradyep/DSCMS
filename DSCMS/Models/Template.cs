using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  /// <summary>
  /// Razor template that contains all the HTML needed to represent specific content.
  /// </summary>
  public class Template
  {
    public int TemplateId { get; set; }
    public string? Name { get; set; }
    
    [Display(Name = "Template Source")]
    public string? TemplateSource { get; set; }
    
    [Display(Name = "Source Type")]
    public int SourceTypeId { get; set; }
    [ForeignKey("SourceTypeId")]
    public SourceType? SourceType { get; set; }
    
    [Display(Name = "Is For Multiple Contents")]
    public int IsForMultipleContents { get; set; }

    public int? LayoutId { get; set; }
    public Layout? Layout { get; set; }
    public List<Content> Contents { get; set; } = new List<Content>();

    [InverseProperty("MultipleContentsTemplate")]
    public List<ContentType> UsedAsMultipleContentsTemplate { get; set; } = new List<ContentType>();
    [InverseProperty("DefaultSingleContentTemplate")]
    public List<ContentType> UsedAsDefaultSingleContentTemplate { get; set; } = new List<ContentType>();
  }
}
