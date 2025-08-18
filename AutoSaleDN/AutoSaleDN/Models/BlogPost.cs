using System.ComponentModel.DataAnnotations;

namespace AutoSaleDN.Models
{
    public class BlogPost
    {
        [Key]
        public int PostId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CategoryId { get; set; }
        public BlogCategory Category { get; set; }

        [Required, StringLength(255)]
        public string Title { get; set; }

        [Required, StringLength(255)]
        public string Slug { get; set; } // Thuộc tính bổ sung

        [Required]
        public string Content { get; set; }

        public string? Excerpt { get; set; } // Thuộc tính bổ sung

        public string? FeaturedImage { get; set; } // Thuộc tính bổ sung

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedDate { get; set; }

        public int ViewCount { get; set; } // Thuộc tính bổ sung

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<BlogPostTag>? BlogPostTags { get; set; }
    }
}
