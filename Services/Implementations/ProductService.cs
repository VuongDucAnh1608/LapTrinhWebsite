using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Website_QuanLyKhoHangThucPham.Data;
using Website_QuanLyKhoHangThucPham.Models;
using Website_QuanLyKhoHangThucPham.ViewModels;

namespace Website_QuanLyKhoHangThucPham.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly ICloudinaryService _cloudinary;

        public ProductService(AppDbContext db, ICloudinaryService cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        public async Task<PagedResult<Product>> GetPagedAsync(string? search, int? categoryId, int page = 1, int pageSize = 10)
        {
            var q = _db.Products.Include(p => p.Category).Include(p => p.Supplier)
                                .Include(p => p.Batches).Where(p => p.IsActive);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.Name.Contains(search) || (p.SKU != null && p.SKU.Contains(search)));
            if (categoryId.HasValue) q = q.Where(p => p.CategoryId == categoryId);
            var total = await q.CountAsync();
            var items = await q.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<Product> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<Product?> GetByIdAsync(int id)
            => await _db.Products.Include(p => p.Category).Include(p => p.Supplier)
                                  .Include(p => p.Batches).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Product> CreateAsync(ProductViewModel model)
        {
            string? imageUrl = null;
            string? imageUrls = null;
            string? videoUrl = null;

            if (model.ImageFiles != null && model.ImageFiles.Count > 0)
            {
                var urls = new List<string>();
                foreach (var file in model.ImageFiles.Where(f => f.Length > 0))
                {
                    CloudinaryUploadResult result;
                    if (file.ContentType.StartsWith("video/"))
                        result = await _cloudinary.UploadVideoAsync(file, "products");
                    else
                        result = await _cloudinary.UploadAsync(file, "products");

                    if (result.Success) urls.Add(result.Url);
                }
                imageUrl = urls.FirstOrDefault(u => !u.Contains("/video/"));
                imageUrls = string.Join(",", urls.Where(u => !u.Contains("/video/")).Skip(1));
                videoUrl = urls.FirstOrDefault(u => u.Contains("/video/"));
            }

            var product = new Product
            {
                Name = model.Name,
                SKU = model.SKU,
                CategoryId = model.CategoryId,
                SupplierId = model.SupplierId,
                Unit = model.Unit,
                CostPrice = model.CostPrice,
                SellPrice = model.SellPrice,
                MinStockLevel = model.MinStockLevel,
                Description = model.Description,
                ImageUrl = imageUrl,
                ImageUrls = imageUrls,
                VideoUrl = videoUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            if (model.StockQuantity > 0)
                await AddStockBatchAsync(product, model.StockQuantity, model.StockExpiryDate);

            return product;
        }

        public async Task<Product?> UpdateAsync(int id, ProductViewModel model)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return null;

            if (model.ImageFiles != null && model.ImageFiles.Any(f => f.Length > 0))
            {
                var urls = new List<string>();
                foreach (var file in model.ImageFiles.Where(f => f.Length > 0))
                {
                    CloudinaryUploadResult result;
                    if (file.ContentType.StartsWith("video/"))
                        result = await _cloudinary.UploadVideoAsync(file, "products");
                    else
                        result = await _cloudinary.UploadAsync(file, "products");

                    if (result.Success) urls.Add(result.Url);
                }

                if (urls.Any())
                {
                    product.ImageUrl = urls.FirstOrDefault(u => !u.Contains("/video/"));
                    var imageUrlsList = string.Join(",", urls.Where(u => !u.Contains("/video/")).Skip(1));
                    product.ImageUrls = string.IsNullOrEmpty(imageUrlsList) ? null : imageUrlsList;
                    product.VideoUrl = urls.FirstOrDefault(u => u.Contains("/video/"));
                }
            }

            product.Name = model.Name;
            product.SKU = model.SKU;
            product.CategoryId = model.CategoryId;
            product.SupplierId = model.SupplierId;
            product.Unit = model.Unit;
            product.CostPrice = model.CostPrice;
            product.SellPrice = model.SellPrice;
            product.MinStockLevel = model.MinStockLevel;
            product.Description = model.Description;

            await _db.SaveChangesAsync();

            if (model.StockQuantity > 0)
                await AddStockBatchAsync(product, model.StockQuantity, model.StockExpiryDate);

            return product;
        }

        private async Task AddStockBatchAsync(Product product, int quantity, DateTime? expiryDate)
        {
            var batch = new InventoryBatch
            {
                ProductId = product.Id,
                BatchCode = $"BATCH-{(product.SKU ?? "SP" + product.Id)}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                Quantity = quantity,
                ManufactureDate = DateTime.UtcNow,
                ExpiryDate = expiryDate ?? DateTime.UtcNow.AddMonths(6),
                ReceivedDate = DateTime.UtcNow
            };
            _db.InventoryBatches.Add(batch);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return false;
            p.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> SearchSuggestionsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<object>();

            var normalizedTerm = RemoveDiacritics(term).ToLower().Trim();

            if (normalizedTerm.Length < 2)
                return new List<object>();

            var allActiveProducts = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Batches)
                .Where(p => p.IsActive)
                .ToListAsync();

            var result = allActiveProducts
                .Where(p => RemoveDiacritics(p.Name).ToLower().Contains(normalizedTerm))
                .Select(p => new
                {
                    id = p.Id,
                    label = p.Name,
                    category = p.Category != null ? p.Category.Name : "",
                    unit = p.Unit,
                    stock = p.Batches != null ? p.Batches.Sum(b => b.Quantity) : 0,
                    price = p.SellPrice
                })
                .Take(10)
                .ToList();

            return result.Cast<object>().ToList();
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public async Task<List<Category>> GetCategoriesAsync()
            => await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

        public async Task<List<Supplier>> GetSuppliersAsync()
            => await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
    }
}