using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        public string Name { get; set; }
        public string? LogoUrl { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
