using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
    public class Template
    {
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public string FileLocation { get; set; }
        public string FileContents { get; set; }

        public int LayoutId { get; set; }
        public Layout Layout { get; set; }
        public List<Content> Contents { get; set; }
        public List<ContentType> ContentTypes { get; set; }
    }
}
