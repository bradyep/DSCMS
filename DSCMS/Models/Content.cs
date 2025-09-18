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
    [RegularExpression(@"^[a-z0-9-]+$"), Required, StringLength(100)]
    public string UrlToDisplay { get; set; } = string.Empty;
    public string? Title { get; set; }
    [DataType(DataType.MultilineText)]
    public string? Body { get; set; }
    [Display(Name = "Creation Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime CreationDate { get; set; }
    [Display(Name = "Last Updated Date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime LastUpdatedDate { get; set; }

    [Display(Name = "Content Type")]
    public int ContentTypeId { get; set; }
    public ContentType? ContentType { get; set; }
    [Display(Name = "Template")]
    public int TemplateId { get; set; }
    public Template? Template { get; set; }

    public string? CreatedBy { get; set; }
    [ForeignKey("CreatedBy")]
    public ApplicationUser? CreatedByUser { get; set; }

    public string? LastUpdatedBy { get; set; }
    [ForeignKey("LastUpdatedBy")]
    public ApplicationUser? LastUpdatedByUser { get; set; }

    public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();

    public string GetValueFor(string contentTypeItemName)
    {
      ContentItem? contentItem = this.ContentItems.Where(ci => ci.ContentTypeItem!.Name == contentTypeItemName).FirstOrDefault();
      return contentItem == null ? "" : contentItem.Value ?? "";
    }
  }
}
