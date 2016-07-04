using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
    public class Layout
    {
        public int LayoutId { get; set; }
        public string Name { get; set; }
        public string FileLocation { get; set; }
        public string FileContents { get; set; }

        public List<Template> Templates { get; set; }
    }
}
