using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class BlogTag
    {
        [Key]
        public int TagId { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }
        public string? Slug { get; set; } // Thuộc tính bổ sung
        public ICollection<BlogPostTag>? BlogPostTags { get; set; }
    }
}
