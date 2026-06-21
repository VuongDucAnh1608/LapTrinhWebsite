using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Website_QuanLyKhoHangThucPham.Services;
using Website_QuanLyKhoHangThucPham.ViewModels;

namespace Website_QuanLyKhoHangThucPham.Controllers
{
    [Authorize(Policy = "SalesOrAbove")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IAuditService _auditService;

        public ProductController(IProductService productService, IAuditService auditService)
        {
            _productService = productService;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            var result = await _productService.GetPagedAsync(search, categoryId, page, 10);
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = new SelectList(await _productService.GetCategoriesAsync(), "Id", "Name");
            return View("Admin_ProductIndex", result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View("Admin_ProductDetails", product);
        }

        [Authorize(Policy = "AdminOrWarehouse")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View("Admin_ProductCreate", new ProductViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrWarehouse")]
        public async Task<IActionResult> Create([FromForm] ProductViewModel model)
        {
            if (model.ImageFiles == null || !model.ImageFiles.Any())
                ModelState.Remove("ImageFiles");

            if (!ModelState.IsValid) { await PopulateDropdowns(); return View("Admin_ProductCreate", model); }

            var product = await _productService.CreateAsync(model);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditService.LogAsync(userId, "CREATE", "Product", product.Id.ToString(),
                null, $"Thêm sản phẩm: {product.Name}");
            TempData["Success"] = $"Thêm sản phẩm '{product.Name}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOrWarehouse")]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _productService.GetByIdAsync(id);
            if (p == null) return NotFound();
            await PopulateDropdowns();
            ViewBag.CurrentStock = p.TotalStock;
            return View("Admin_ProductEdit", new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                CategoryId = p.CategoryId,
                SupplierId = p.SupplierId,
                Unit = p.Unit,
                CostPrice = p.CostPrice,
                SellPrice = p.SellPrice,
                MinStockLevel = p.MinStockLevel,
                Description = p.Description,
                ExistingImageUrl = p.ImageUrl,
                ExistingImageUrls = p.ImageUrls,
                ExistingVideoUrl = p.VideoUrl
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrWarehouse")]
        public async Task<IActionResult> Edit(int id, [FromForm] ProductViewModel model)
        {
            if (model.ImageFiles == null || !model.ImageFiles.Any())
                ModelState.Remove("ImageFiles");

            if (!ModelState.IsValid) { await PopulateDropdowns(); return View("Admin_ProductEdit", model); }

            var updated = await _productService.UpdateAsync(id, model);
            if (updated == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _auditService.LogAsync(userId, "UPDATE", "Product", id.ToString());
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrWarehouse")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Autocomplete(string term)
        {
            var suggestions = await _productService.SearchSuggestionsAsync(term ?? "");
            return Json(suggestions);
        }

        private async Task PopulateDropdowns()
        {
            ViewBag.Categories = new SelectList(await _productService.GetCategoriesAsync(), "Id", "Name");
            ViewBag.Suppliers = new SelectList(await _productService.GetSuppliersAsync(), "Id", "Name");
        }
    }
}