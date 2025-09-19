using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  /// <summary>
  /// Razor page that contains all the HTML needed to represent a layout. Includes metadata, file location, and associated templates.
  /// </summary>
  /// <remarks>A layout defines the structure and content of a specific configuration, including its name, file
  /// location,  and file contents. It also maintains a collection of associated templates.</remarks>
  public class Layout
  {
    public int LayoutId { get; set; }
    public string Name { get; set; } = string.Empty;
    [Display(Name = "File Location")]
    /// <remarks>Specifies the path where the layout file is stored, which can be a relative or absolute path.</remarks>
    public string FileLocation { get; set; } = string.Empty;
    [Display(Name = "File Contents")]
    /// <remarks>Holds the actual content of the layout file, which may include HTML, CSS, and Razor syntax.</remarks>
    public string? FileContents { get; set; }

    public List<Template> Templates { get; set; } = new List<Template>();
  }
}
