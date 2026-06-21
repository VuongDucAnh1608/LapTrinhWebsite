using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Website_QuanLyKhoHangThucPham.Data;
using Website_QuanLyKhoHangThucPham.Models;
using Website_QuanLyKhoHangThucPham.Services;
using Website_QuanLyKhoHangThucPham.ViewModels;

namespace Website_QuanLyKhoHangThucPham.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly ISePayService _sePayService;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CART_KEY = "CART_V1";

        public PaymentController(
            AppDbContext db,
            ISePayService sepay,
            UserManager<ApplicationUser> userManager,
            IConfiguration cfg) 
        {
            _db = db;
            _sePayService = sepay;
            _userManager = userManager;
            _cfg = cfg; 
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.Items.Any()) { TempData["Error"] = "Gio hang trong!"; return RedirectToAction("Cart", "Store"); }

            model.Items = cart.Items;
            if (!ModelState.IsValid) return View("~/Views/Store/Customer_StoreCheckout.cshtml", model);

            var userId = User.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : null;

            var orderCode = $"DH{DateTime.UtcNow:yyyyMMddHHmmss}";
            var order = new StoreOrder
            {
                UserId = userId,
                OrderCode = orderCode,
                TotalAmount = cart.Total,
                Status = model.PaymentMethod == "COD" ? "COD" : "Pending",
                PaymentMethod = model.PaymentMethod,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                DeliveryAddress = model.DeliveryAddress,
                Note = model.Note,
                CreatedAt = DateTime.UtcNow,
                Items = cart.Items.Select(i => new StoreOrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.SellPrice,
                    Quantity = i.Quantity
                }).ToList()
            };

            _db.StoreOrders.Add(order);
            await _db.SaveChangesAsync();

            if (model.PaymentMethod == "COD")
            {
                ClearCart();
                return RedirectToAction(nameof(Success), new { orderCode });
            }

            var qrUrl = _sePayService.GenerateQrUrl(orderCode, (long)cart.Total);
            var content = _sePayService.GenerateTransferContent(orderCode);
            ViewBag.QrUrl = qrUrl;
            ViewBag.OrderCode = orderCode;
            ViewBag.Amount = cart.Total;
            ViewBag.Content = content;
            ViewBag.Order = order;
            return View("Customer_PaymentQRPending");
        }

        [HttpGet]
        public async Task<IActionResult> CheckStatus(string orderCode)
        {
            var order = await _db.StoreOrders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order == null) return Json(new { status = "NotFound" });
            if (order.Status == "Paid") ClearCart();
            return Json(new { status = order.Status, paidAt = order.PaidAt });
        }

        [HttpPost]
        public async Task<IActionResult> SePayWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var rawBody = await reader.ReadToEndAsync();

            var authHeader = Request.Headers["Authorization"].ToString();
            var expectedApiKey = _cfg["SePay:WebhookSecret"];

            if (!string.IsNullOrEmpty(expectedApiKey))
            {
                if (authHeader != $"Apikey {expectedApiKey}")
                {
                    return Unauthorized(new { success = false, message = "Invalid API Key" });
                }
            }

            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(rawBody);
                var content = data.GetProperty("content").GetString() ?? "";
                var amount = data.GetProperty("transferAmount").GetInt64();

                var order = await _db.StoreOrders
                    .Where(o => o.Status == "Pending" && content.Contains(o.OrderCode))
                    .FirstOrDefaultAsync();

                if (order != null && amount >= (long)order.TotalAmount)
                {
                    order.Status = "Paid";
                    order.TransactionRef = data.TryGetProperty("referenceCode", out var refCode)
                                            ? refCode.GetString() : null;
                    order.PaidAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }

                return Ok(new { success = true });
            }
            catch
            {
                return BadRequest(new { success = false });
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AnyAuthenticated")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _db.StoreOrders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View("Customer_PaymentMyOrders", orders);
        }

        [HttpGet]
        public async Task<IActionResult> TrackOrder(string? orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User);
                    var since = DateTime.UtcNow.AddDays(-7);
                    ViewBag.RecentOrders = await _db.StoreOrders
                        .Include(o => o.Items)
                        .Where(o => o.UserId == userId && o.CreatedAt >= since)
                        .OrderByDescending(o => o.CreatedAt)
                        .ToListAsync();
                }
                return View("Customer_PaymentTrackOrder", (StoreOrder?)null);
            }

            var order = await _db.StoreOrders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
            {
                ViewBag.NotFound = true;
                return View("Customer_PaymentTrackOrder", (StoreOrder?)null);
            }

            return View("Customer_PaymentTrackOrder", order);
        }

        public async Task<IActionResult> Success(string orderCode)
        {
            var order = await _db.StoreOrders.Include(o => o.Items)
                                             .ThenInclude(i => i.Product)
                                             .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            if (order != null && (order.Status == "Paid" || order.Status == "COD")) ClearCart();
            return View("Customer_PaymentSuccess", order);
        }

        public async Task<IActionResult> Failed(string orderCode)
        {
            var order = await _db.StoreOrders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
            return View("Customer_PaymentFailed", order);
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> History(string? status, int page = 1)
        {
            int pageSize = 20;
            var query = _db.StoreOrders.Include(o => o.Items).AsQueryable();
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(o => o.CreatedAt)
                                   .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.TotalCount = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.StatusFilter = status;
            return View("Admin_PaymentHistory", items);
        }

        private CartViewModel GetCart()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            return string.IsNullOrEmpty(json) ? new CartViewModel()
                : JsonSerializer.Deserialize<CartViewModel>(json) ?? new CartViewModel();
        }

        private void ClearCart() => HttpContext.Session.Remove(CART_KEY);
    }
}