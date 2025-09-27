using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    // Add custom properties to the IdentityUser
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
    }
}
