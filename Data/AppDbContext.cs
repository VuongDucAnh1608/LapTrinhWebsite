using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Website_QuanLyKhoHangThucPham.Models;

namespace Website_QuanLyKhoHangThucPham.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryBatch> InventoryBatches { get; set; }
        public DbSet<ImportReceipt> ImportReceipts { get; set; }
        public DbSet<ImportReceiptDetail> ImportReceiptDetails { get; set; }
        public DbSet<ExportRequest> ExportRequests { get; set; }
        public DbSet<ExportRequestDetail> ExportRequestDetails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<StoreOrder> StoreOrders { get; set; }
        public DbSet<StoreOrderItem> StoreOrderItems { get; set; }
        public DbSet<AppPermission> AppPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Product>().HasIndex(p => p.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");
            b.Entity<InventoryBatch>().HasIndex(x => x.BatchCode).IsUnique();
            b.Entity<ImportReceipt>().HasIndex(x => x.ReceiptCode).IsUnique();
            b.Entity<ExportRequest>().HasIndex(x => x.RequestCode).IsUnique();
            b.Entity<StoreOrder>().HasIndex(x => x.OrderCode).IsUnique();

            b.Entity<Product>().HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Product>().HasOne(p => p.Supplier).WithMany(s => s.Products).HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<InventoryBatch>().HasOne(x => x.Product).WithMany(p => p.Batches).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<ImportReceipt>().HasOne(r => r.Supplier).WithMany().HasForeignKey(r => r.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<ImportReceipt>().HasOne(r => r.CreatedBy).WithMany().HasForeignKey(r => r.CreatedById).OnDelete(DeleteBehavior.Restrict);
            b.Entity<ImportReceipt>().HasOne(r => r.ConfirmedBy).WithMany().HasForeignKey(r => r.ConfirmedById).OnDelete(DeleteBehavior.Restrict);
            b.Entity<ImportReceiptDetail>().HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<ExportRequest>().HasOne(r => r.RequestedBy).WithMany().HasForeignKey(r => r.RequestedById).OnDelete(DeleteBehavior.Restrict);
            b.Entity<ExportRequest>().HasOne(r => r.ProcessedBy).WithMany().HasForeignKey(r => r.ProcessedById).OnDelete(DeleteBehavior.Restrict);
            b.Entity<ExportRequestDetail>().HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<StoreOrderItem>().HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<AppPermission>().HasData(
                new AppPermission { Id=1,  PermissionKey="Category.Index",         DisplayName="Xem danh muc",         Controller="Category",       Action="Index"    },
                new AppPermission { Id=2,  PermissionKey="Category.Create",        DisplayName="Tao danh muc",         Controller="Category",       Action="Create"   },
                new AppPermission { Id=3,  PermissionKey="Category.Edit",          DisplayName="Sua danh muc",         Controller="Category",       Action="Edit"     },
                new AppPermission { Id=4,  PermissionKey="Supplier.Index",         DisplayName="Xem NCC",              Controller="Supplier",        Action="Index"    },
                new AppPermission { Id=5,  PermissionKey="Supplier.Create",        DisplayName="Tao NCC",              Controller="Supplier",        Action="Create"   },
                new AppPermission { Id=6,  PermissionKey="Product.Index",          DisplayName="Xem san pham",         Controller="Product",         Action="Index"    },
                new AppPermission { Id=7,  PermissionKey="Product.Create",         DisplayName="Tao san pham",         Controller="Product",         Action="Create"   },
                new AppPermission { Id=8,  PermissionKey="Product.Edit",           DisplayName="Sua san pham",         Controller="Product",         Action="Edit"     },
                new AppPermission { Id=9,  PermissionKey="ImportReceipt.Index",    DisplayName="Xem phieu nhap",       Controller="ImportReceipt",   Action="Index"    },
                new AppPermission { Id=10, PermissionKey="ImportReceipt.Create",   DisplayName="Lap phieu nhap",       Controller="ImportReceipt",   Action="Create"   },
                new AppPermission { Id=11, PermissionKey="ExportRequest.Index",    DisplayName="Xem yeu cau xuat",     Controller="ExportRequest",   Action="Index"    },
                new AppPermission { Id=12, PermissionKey="ExportRequest.Create",   DisplayName="Tao yeu cau xuat",     Controller="ExportRequest",   Action="Create"   },
                new AppPermission { Id=13, PermissionKey="ExportRequest.Process",  DisplayName="Duyet xuat kho",       Controller="ExportRequest",   Action="Process"  },
                new AppPermission { Id=14, PermissionKey="AuditLog.Index",         DisplayName="Xem audit log",        Controller="AuditLog",        Action="Index"    },
                new AppPermission { Id=15, PermissionKey="Payment.History",        DisplayName="Xem lich su thanh toan",Controller="Payment",        Action="History"  }
            );

            b.Entity<RolePermission>().HasData(
                new RolePermission { Id=1,  RoleName="Admin", PermissionId=1  },
                new RolePermission { Id=2,  RoleName="Admin", PermissionId=2  },
                new RolePermission { Id=3,  RoleName="Admin", PermissionId=3  },
                new RolePermission { Id=4,  RoleName="Admin", PermissionId=4  },
                new RolePermission { Id=5,  RoleName="Admin", PermissionId=5  },
                new RolePermission { Id=6,  RoleName="Admin", PermissionId=6  },
                new RolePermission { Id=7,  RoleName="Admin", PermissionId=7  },
                new RolePermission { Id=8,  RoleName="Admin", PermissionId=8  },
                new RolePermission { Id=9,  RoleName="Admin", PermissionId=9  },
                new RolePermission { Id=10, RoleName="Admin", PermissionId=10 },
                new RolePermission { Id=11, RoleName="Admin", PermissionId=11 },
                new RolePermission { Id=12, RoleName="Admin", PermissionId=12 },
                new RolePermission { Id=13, RoleName="Admin", PermissionId=13 },
                new RolePermission { Id=14, RoleName="Admin", PermissionId=14 },
                new RolePermission { Id=15, RoleName="Admin", PermissionId=15 },
                new RolePermission { Id=16, RoleName="WarehouseStaff", PermissionId=6  },
                new RolePermission { Id=17, RoleName="WarehouseStaff", PermissionId=7  },
                new RolePermission { Id=18, RoleName="WarehouseStaff", PermissionId=8  },
                new RolePermission { Id=19, RoleName="WarehouseStaff", PermissionId=9  },
                new RolePermission { Id=20, RoleName="WarehouseStaff", PermissionId=10 },
                new RolePermission { Id=21, RoleName="WarehouseStaff", PermissionId=11 },
                new RolePermission { Id=22, RoleName="WarehouseStaff", PermissionId=13 },
                new RolePermission { Id=23, RoleName="SalesStaff", PermissionId=6  },
                new RolePermission { Id=24, RoleName="SalesStaff", PermissionId=11 },
                new RolePermission { Id=25, RoleName="SalesStaff", PermissionId=12 }
            );
        }
    }
}
