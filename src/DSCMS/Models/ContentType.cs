using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  public class ContentType
  {
    public int ContentTypeId { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int ItemsPerPage { get; set; }


    [Display(Name = "Template")]
    public int TemplateId { get; set; }
    [ForeignKey("TemplateId")]
    public Template Template { get; set; }

    public int DefaultTemplateForContent { get; set; }
    [ForeignKey("DefaultTemplateForContent")]
    public Template DefaultContentTemplate { get; set; }

    public List<ContentTypeItem> ContentTypeItems { get; set; }
    public List<Content> Contents { get; set; }
  }
}
