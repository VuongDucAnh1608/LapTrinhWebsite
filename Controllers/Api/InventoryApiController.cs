using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_QuanLyKhoHangThucPham.Data;

namespace Website_QuanLyKhoHangThucPham.Controllers.Api
{
    [Route("api/inventory")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class InventoryApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        public InventoryApiController(AppDbContext db) => _db = db;

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary()
        {
            var now   = DateTime.UtcNow;
            var today = now.Date;

            var data = new
            {
                TongSanPham    = await _db.Products.CountAsync(p => p.IsActive),
                TongDanhMuc    = await _db.Categories.CountAsync(c => c.IsActive),
                TongNCC        = await _db.Suppliers.CountAsync(s => s.IsActive),
                DonHangHomNay  = await _db.StoreOrders.CountAsync(o => o.CreatedAt >= today),
                DoanhThuHomNay = await _db.StoreOrders
                    .Where(o => o.CreatedAt >= today && (o.Status == "Paid" || o.Status == "COD"))
                    .SumAsync(o => o.TotalAmount),
                YCXuatChoDuyet = await _db.ExportRequests
                    .CountAsync(r => r.Status == Website_QuanLyKhoHangThucPham.Models.ExportStatus.Pending),
                LoSapHetHan    = await _db.InventoryBatches
                    .CountAsync(b => b.ExpiryDate <= now.AddDays(30) && b.ExpiryDate > now && b.Quantity > 0)
            };

            return Ok(new { success = true, data });
        }

        [HttpGet("expiring")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExpiringBatches([FromQuery] int days = 30)
        {
            var now  = DateTime.UtcNow;
            var data = await _db.InventoryBatches
                .Include(b => b.Product).ThenInclude(p => p!.Category)
                .Where(b => b.ExpiryDate <= now.AddDays(days) && b.ExpiryDate > now && b.Quantity > 0)
                .OrderBy(b => b.ExpiryDate)
                .Select(b => new
                {
                    b.BatchCode,
                    b.Quantity,
                    HanSD          = b.ExpiryDate,
                    ConLaiNgay     = (int)(b.ExpiryDate - now).TotalDays,
                    SanPham        = b.Product!.Name,
                    DanhMuc        = b.Product.Category!.Name,
                    DonVi          = b.Product.Unit
                }).ToListAsync();

            return Ok(new { success = true, total = data.Count, data });
        }
    }
}
