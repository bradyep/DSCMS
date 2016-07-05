using System;
using System.Collections.Generic;
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

        public int TemplateId { get; set; }
        public Template Template { get; set; }
        public List<ContentTypeItem> ContentTypeItems { get; set; }
        public List<Content> Contents { get; set; }
    }
}
