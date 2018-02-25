using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  public class Template
  {
    public int TemplateId { get; set; }
    public string Name { get; set; }
    [Display(Name = "File Location")]
    public string FileLocation { get; set; }
    [Display(Name = "File Contents")]
    public string FileContents { get; set; }
    [Display(Name = "Type")]
    public int IsForContentType { get; set; }

    public int LayoutId { get; set; }
    public Layout Layout { get; set; }
    public List<Content> Contents { get; set; }

    [InverseProperty("Template")]
    public List<ContentType> ContentTypes { get; set; }
    [InverseProperty("DefaultContentTemplate")]
    public List<ContentType> HasAsDefaultContentTemplate { get; set; }
  }
}
