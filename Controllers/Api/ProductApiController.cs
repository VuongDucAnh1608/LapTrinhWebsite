using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Website_QuanLyKhoHangThucPham.Data;

namespace Website_QuanLyKhoHangThucPham.Controllers.Api
{
    [Route("api/products")]
    [ApiController]
    [Produces("application/json")]
    public class ProductApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProductApiController(AppDbContext db) => _db = db;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Batches)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            var now = DateTime.UtcNow;
            var data = await query.Select(p => new
            {
                p.Id,
                p.Name,
                p.SKU,
                p.Unit,
                p.SellPrice,
                p.ImageUrl,
                p.Description,
                Category  = p.Category!.Name,
                TonKho    = p.Batches.Where(b => b.ExpiryDate > now).Sum(b => b.Quantity),
                SapHetHan = p.Batches.Any(b => b.ExpiryDate <= now.AddDays(30) && b.ExpiryDate > now)
            }).OrderBy(p => p.Name).ToListAsync();

            return Ok(new { success = true, total = data.Count, data });
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var now = DateTime.UtcNow;
            var p = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Batches)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (p == null)
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    p.Id, p.Name, p.SKU, p.Unit, p.SellPrice,
                    p.ImageUrl, p.Description, p.MinStockLevel,
                    Category        = p.Category?.Name,
                    NhaCungCap      = p.Supplier?.Name,
                    TonKhoHienTai   = p.Batches.Where(b => b.ExpiryDate > now).Sum(b => b.Quantity),
                    SoLoBatches     = p.Batches.Count,
                    TinhTrang       = p.TotalStock == 0 ? "Hết hàng"
                                    : p.IsLowStock   ? "Sắp hết hàng"
                                                     : "Còn hàng"
                }
            });
        }

        [HttpGet("autocomplete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Autocomplete([FromQuery] string q = "")
        {
            if (q.Length < 1) return Ok(new { success = true, data = Array.Empty<object>() });

            var isAuth = User.Identity?.IsAuthenticated == true;
            var now    = DateTime.UtcNow;
            var query  = _db.Products
                .Include(p => p.Category).Include(p => p.Batches)
                .Where(p => p.IsActive && p.Name.Contains(q))
                .Take(8);

            if (isAuth)
            {
                var data = await query.Select(p => new
                {
                    p.Id, label = p.Name, p.Unit,
                    price    = p.SellPrice,
                    category = p.Category!.Name,
                    stock    = p.Batches.Where(b => b.ExpiryDate > now).Sum(b => b.Quantity),
                    sku      = p.SKU
                }).ToListAsync();
                return Ok(new { success = true, data });
            }
            else
            {
                var data = await query.Select(p => new
                {
                    p.Id, label = p.Name, p.Unit,
                    price    = p.SellPrice,
                    category = p.Category!.Name
                }).ToListAsync();
                return Ok(new { success = true, data });
            }
        }

        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            var now  = DateTime.UtcNow;
            var data = await _db.Categories
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Id, c.Name, c.Description,
                    SoSanPham = c.Products.Count(p => p.IsActive)
                })
                .OrderBy(c => c.Name).ToListAsync();

            return Ok(new { success = true, data });
        }
    }
}
