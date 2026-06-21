using System.ComponentModel.DataAnnotations;

namespace Website_QuanLyKhoHangThucPham.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [MaxLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Mã SKU")]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp")]
        [Display(Name = "Nhà cung cấp")]
        public int SupplierId { get; set; }

        [MaxLength(20)]
        [Display(Name = "Đơn vị tính")]
        public string Unit { get; set; } = "Cái";

        [Required]
        [Range(0, 10000000, ErrorMessage = "Giá nhập phải từ 0 đến 10.000.000đ")]
        [Display(Name = "Giá nhập (₫)")]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, 10000000, ErrorMessage = "Giá bán phải từ 0 đến 10.000.000đ")]
        [Display(Name = "Giá bán (₫)")]
        public decimal SellPrice { get; set; }

        [Display(Name = "Tồn kho tối thiểu")]
        public int MinStockLevel { get; set; } = 10;

        [Range(0, 99999, ErrorMessage = "Số lượng phải từ 0 đến 99.999")]
        [Display(Name = "Nhập thêm tồn kho")]
        public int StockQuantity { get; set; } = 0;

        [DataType(DataType.Date)]
        [Display(Name = "Hạn sử dụng (nếu có nhập tồn kho)")]
        public DateTime? StockExpiryDate { get; set; }

        [MaxLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh sản phẩm (có thể chọn nhiều)")]
        public List<IFormFile>? ImageFiles { get; set; }

        public string? ExistingImageUrl { get; set; }
        public string? ExistingImageUrls { get; set; }
        public string? ExistingVideoUrl { get; set; }
    }
}
