using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class BlogCategory
    {
        [Key]
        public int CategoryId { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<BlogPost>? BlogPosts { get; set; }
    }
}
