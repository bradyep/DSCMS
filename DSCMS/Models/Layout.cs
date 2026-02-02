using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  /// <summary>
  /// Razor page that contains all the HTML needed to represent a layout. Includes metadata, layout source, and associated templates.
  /// </summary>
  /// <remarks>A layout defines the structure and content of a specific configuration, including its name, layout
  /// source, and source type. It also maintains a collection of associated templates.</remarks>
  public class Layout
  {
    public int LayoutId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Layout Source")]
    /// <remarks>Specifies the path where the layout file is stored or contains inline content.</remarks>
    public string LayoutSource { get; set; } = string.Empty;
    
    [Display(Name = "Source Type")]
    public int SourceTypeId { get; set; }
    [ForeignKey("SourceTypeId")]
    public SourceType? SourceType { get; set; }

    public List<Template> Templates { get; set; } = new List<Template>();
  }
}
