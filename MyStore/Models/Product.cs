using System.ComponentModel.DataAnnotations;

namespace MyStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "سعر المنتج مطلوب")]
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; } // Path to the image in wwwroot

        // Foreign Key
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
    }
}
