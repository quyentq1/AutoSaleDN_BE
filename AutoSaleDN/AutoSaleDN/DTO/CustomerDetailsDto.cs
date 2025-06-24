using System;
using System.Collections.Generic;

namespace AutoSaleDN.Models
{
    public class CustomerDetailsDto
    {
        public CustomerBasicInfoDto BasicInfo { get; set; }
        public CustomerStatisticsDto Statistics { get; set; }
        public List<BookingDto> RecentBookings { get; set; }
        public List<ReviewDto> RecentReviews { get; set; }
        public List<PaymentDto> RecentPayments { get; set; }
        public List<BlogPostDto> RecentBlogPosts { get; set; }
    }

    public class CustomerBasicInfoDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CustomerStatisticsDto
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CanceledBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
    }

    public class BookingDto
    {
        public int BookingId { get; set; }
        public CarDetailsDto CarDetails { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public string BookingStatus { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public CarDetailsDto CarDetails { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime DateOfPayment { get; set; }
        public string TransactionId { get; set; }
    }

    public class BlogPostDto
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string CategoryName { get; set; }
        public List<BlogTagDto> Tags { get; set; }
    }

    public class BlogTagDto
    {
        public int TagId { get; set; }
        public string Name { get; set; }
    }

    public class CarDetailsDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
    }
}