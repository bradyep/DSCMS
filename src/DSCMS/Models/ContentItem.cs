using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
    public class ContentItem
    {
        public int ContentItemId { get; set; }
        public string Value { get; set; }

        public int ContentTypeItemId { get; set; }
        public ContentTypeItem ContentTypeItem { get; set; }
        public int ContentId { get; set; }
        public Content Content { get; set; }
    }
}
