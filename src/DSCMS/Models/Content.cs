using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  public class Content
  {
    public int ContentId { get; set; }
    [Display(Name = "URL To Display")]
    public string UrlToDisplay { get; set; }
    public string Title { get; set; }
    [DataType(DataType.MultilineText)]
    public string Body { get; set; }
    [Display(Name = "Creation Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime CreationDate { get; set; }
    [Display(Name = "Last Updated Date")]
    [DataType(DataType.Date)]
    public DateTime LastUpdatedDate { get; set; }

    public int ContentTypeId { get; set; }
    public ContentType ContentType { get; set; }
    public int TemplateId { get; set; }
    public Template Template { get; set; }
    public int CreatedBy { get; set; }
    [ForeignKey("CreatedBy")]
    public User CreatedByUser { get; set; }
    public int LastUpdatedBy { get; set; }
    [ForeignKey("LastUpdatedBy")]
    public User LastUpdatedByUser { get; set; }
    public List<ContentItem> ContentItems { get; set; }

    public string GetValueFor(string contentTypeItemName)
    {
      return this.ContentItems.Where(ci => ci.ContentTypeItem.Name == contentTypeItemName).FirstOrDefault().Value ?? "";
    }
  }
}
