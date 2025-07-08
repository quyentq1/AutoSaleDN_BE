using System;
namespace AutoSaleDN.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}