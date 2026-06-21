using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Website_QuanLyKhoHangThucPham.Models
{
    public class StoreOrder
    {
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public string? UserId { get; set; }  
        public ApplicationUser? User { get; set; }
        [MaxLength(50)] public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "Pending";
        [MaxLength(20)] public string PaymentMethod { get; set; } = "SePay";
        [MaxLength(100)] public string? CustomerName { get; set; }
        [MaxLength(100)] public string? CustomerEmail { get; set; }
        [MaxLength(20)] public string? CustomerPhone { get; set; }
        [MaxLength(300)] public string? DeliveryAddress { get; set; }
        [MaxLength(300)] public string? Note { get; set; }
        [MaxLength(100)] public string? TransactionRef { get; set; }
        public DateTime? PaidAt { get; set; }
        public ICollection<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();
    }

    public class StoreOrderItem
    {
        public int Id { get; set; }
        public int StoreOrderId { get; set; }
        public StoreOrder Order { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        [MaxLength(200)] public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
    }
}
