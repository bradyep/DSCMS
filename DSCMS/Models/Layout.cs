﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DSCMS.Models
{
  public class Layout
  {
    public int LayoutId { get; set; }
    public string Name { get; set; }
    [Display(Name = "File Location")]
    public string FileLocation { get; set; }
    [Display(Name = "File Contents")]
    public string FileContents { get; set; }

    public List<Template> Templates { get; set; }
  }
}
