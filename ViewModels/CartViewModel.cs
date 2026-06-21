using System.ComponentModel.DataAnnotations;
namespace Website_QuanLyKhoHangThucPham.ViewModels
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal SellPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal SubTotal => SellPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.SubTotal);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(15)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [MaxLength(300)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(200)]
        public string? CustomerEmail { get; set; }

        [MaxLength(300)]
        public string? Note { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "SePay";

        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.SubTotal);
    }

    public class PermissionMatrixViewModel
    {
        public List<Website_QuanLyKhoHangThucPham.Models.AppPermission> Permissions { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, List<int>> RolePermMap { get; set; } = new();
    }
}
