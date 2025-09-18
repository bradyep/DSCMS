using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DSCMS.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        
        // Navigation properties for content relationships
        public List<Content> CreatedContent { get; set; } = new List<Content>();
        public List<Content> UpdatedContent { get; set; } = new List<Content>();
    }
}
