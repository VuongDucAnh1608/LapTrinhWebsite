using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_QuanLyKhoHangThucPham.Data;
using Website_QuanLyKhoHangThucPham.Models;

namespace Website_QuanLyKhoHangThucPham.Controllers.Api
{
    [Route("api/orders")]
    [ApiController]
    [Produces("application/json")]
    public class OrderApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OrderApiController(AppDbContext db) => _db = db;

        [HttpGet("{orderCode}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckStatus(string orderCode)
        {
            var order = await _db.StoreOrders
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

            return Ok(new
            {
                success   = true,
                orderCode = order.OrderCode,
                status    = order.Status,
                amount    = order.TotalAmount,
                paidAt    = order.PaidAt,
                customer  = order.CustomerName
            });
        }

        [HttpGet("{orderCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(string orderCode)
        {
            var order = await _db.StoreOrders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

            return Ok(new
            {
                success = true,
                data    = new
                {
                    order.OrderCode,
                    order.Status,
                    order.TotalAmount,
                    order.PaymentMethod,
                    order.CustomerName,
                    order.CustomerPhone,
                    order.CustomerEmail,
                    order.DeliveryAddress,
                    order.Note,
                    order.CreatedAt,
                    order.PaidAt,
                    Items = order.Items.Select(i => new
                    {
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        ThanhTien = i.SubTotal
                    })
                }
            });
        }

        [HttpPost("sepay-webhook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookDto payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.Content))
                return BadRequest(new { success = false });

            var order = await _db.StoreOrders
                .Where(o => o.Status == "Pending" && payload.Content.Contains(o.OrderCode))
                .FirstOrDefaultAsync();

            if (order != null && payload.TransferAmount >= (double)order.TotalAmount)
            {
                order.Status         = "Paid";
                order.TransactionRef = payload.ReferenceCode;
                order.PaidAt         = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }
    }

    public class SePayWebhookDto
    {
        public string Content { get; set; } = string.Empty;
        public double TransferAmount { get; set; }
        public string? ReferenceCode { get; set; }
        public string? BankName { get; set; }
    }
}
