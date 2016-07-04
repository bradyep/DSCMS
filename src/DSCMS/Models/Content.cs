using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
    public class Content
    {
        public int ContentId { get; set; }
        public string UrlToDisplay { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }
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
    }
}
