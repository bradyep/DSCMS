﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
    public class ContentTypeItem
    {
        public int ContentTypeItemId { get; set; }
        public string Name { get; set; }

        public int ContentTypeId { get; set; }
        public ContentType ContentType { get; set; }
        public List<ContentItem> ContentItems { get; set; }
    }
}
