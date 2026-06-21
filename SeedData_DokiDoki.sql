-- ============================================================
--  DOKIDOKI FOOD STORE - SEED DATA
--  Chạy trong SSMS sau khi đã Update-Database thành công
--  Thứ tự: Categories → Suppliers → Products → Batches
-- ============================================================

USE WarehouseDB;
GO

-- ============================================================
-- 1. DANH MỤC SẢN PHẨM
-- ============================================================
INSERT INTO Categories (Name, Description, IsActive, CreatedAt) VALUES
(N'Rau củ tươi',          N'Rau xanh, củ quả tươi từ nông trại',           1, GETUTCDATE()),
(N'Trái cây',             N'Trái cây nhiệt đới và nhập khẩu',              1, GETUTCDATE()),
(N'Thịt tươi',            N'Thịt heo, bò, gà tươi sạch hàng ngày',        1, GETUTCDATE()),
(N'Hải sản tươi sống',    N'Tôm, cua, cá, mực tươi sống',                 1, GETUTCDATE()),
(N'Sữa & Trứng',          N'Sữa tươi, trứng gà, phô mai, bơ',             1, GETUTCDATE()),
(N'Đồ uống',              N'Nước ngọt, nước trái cây, trà, sữa đậu nành', 1, GETUTCDATE()),
(N'Gia vị & Nước chấm',   N'Nước mắm, dầu ăn, muối, đường, tương',       1, GETUTCDATE()),
(N'Thực phẩm khô',        N'Gạo, mì, bún, miến, bánh',                    1, GETUTCDATE());
GO

-- ============================================================
-- 2. NHÀ CUNG CẤP
-- ============================================================
INSERT INTO Suppliers (Name, Code, Phone, Email, ContactPerson, Address, IsActive, CreatedAt) VALUES
(N'Nông trại Xanh Sạch Việt',  N'NSV001', N'0901111222', N'xanhsach@viet.vn',    N'Nguyễn Văn An',   N'Đà Lạt, Lâm Đồng',       1, GETUTCDATE()),
(N'Công ty Nông sản Mekong',   N'MEK002', N'0912222333', N'nongsan@mekong.vn',   N'Trần Thị Bích',   N'Cần Thơ, ĐBSCL',          1, GETUTCDATE()),
(N'Công ty Hải sản Biển Đông', N'BDE003', N'0923333444', N'haisambd@bien.vn',    N'Lê Văn Cường',    N'Vũng Tàu, BR-VT',         1, GETUTCDATE()),
(N'Vinamilk Distribution',     N'VNM004', N'1800123456', N'distribute@vmk.vn',   N'Phạm Thị Dung',   N'Bình Dương',               1, GETUTCDATE()),
(N'Công ty Thực phẩm Đà Lạt',  N'DAL005', N'0934444555', N'tpdalat@dalat.vn',   N'Hoàng Văn Em',    N'Đà Lạt, Lâm Đồng',        1, GETUTCDATE());
GO

-- ============================================================
-- 3. SẢN PHẨM (30 mặt hàng)
-- Ghi chú: CategoryId + SupplierId phải khớp với ID vừa insert
-- ============================================================
DECLARE @CatRauCu    INT = (SELECT Id FROM Categories WHERE Name = N'Rau củ tươi');
DECLARE @CatTraiCay  INT = (SELECT Id FROM Categories WHERE Name = N'Trái cây');
DECLARE @CatThit     INT = (SELECT Id FROM Categories WHERE Name = N'Thịt tươi');
DECLARE @CatHaiSan   INT = (SELECT Id FROM Categories WHERE Name = N'Hải sản tươi sống');
DECLARE @CatSuaTrung INT = (SELECT Id FROM Categories WHERE Name = N'Sữa & Trứng');
DECLARE @CatDoUong   INT = (SELECT Id FROM Categories WHERE Name = N'Đồ uống');
DECLARE @CatGiaVi    INT = (SELECT Id FROM Categories WHERE Name = N'Gia vị & Nước chấm');
DECLARE @CatKho      INT = (SELECT Id FROM Categories WHERE Name = N'Thực phẩm khô');

DECLARE @SupNSV INT = (SELECT Id FROM Suppliers WHERE Code = N'NSV001');
DECLARE @SupMEK INT = (SELECT Id FROM Suppliers WHERE Code = N'MEK002');
DECLARE @SupBDE INT = (SELECT Id FROM Suppliers WHERE Code = N'BDE003');
DECLARE @SupVNM INT = (SELECT Id FROM Suppliers WHERE Code = N'VNM004');
DECLARE @SupDAL INT = (SELECT Id FROM Suppliers WHERE Code = N'DAL005');

INSERT INTO Products (Name, SKU, CategoryId, SupplierId, Unit, CostPrice, SellPrice, MinStockLevel, Description, IsActive, CreatedAt) VALUES
-- 🥦 RAU CỦ TƯƠI
(N'Rau muống tươi',           N'DD001', @CatRauCu,    @SupNSV, N'bó',    7000,   12000,  30, N'Rau muống xanh mướt, hái buổi sáng',        1, GETUTCDATE()),
(N'Cải xanh Đà Lạt',          N'DD002', @CatRauCu,    @SupDAL, N'kg',    18000,  28000,  20, N'Cải xanh Đà Lạt tươi, không thuốc trừ sâu', 1, GETUTCDATE()),
(N'Cà chua bi đỏ',            N'DD003', @CatRauCu,    @SupDAL, N'kg',    14000,  22000,  20, N'Cà chua bi ngọt, màu đỏ đẹp',               1, GETUTCDATE()),
(N'Bắp cải trắng',            N'DD004', @CatRauCu,    @SupNSV, N'kg',    9000,   15000,  25, N'Bắp cải trắng tươi, lá dày',                1, GETUTCDATE()),
(N'Dưa leo Nhật',             N'DD005', @CatRauCu,    @SupDAL, N'kg',    15000,  24000,  20, N'Dưa leo Nhật giòn ngọt, ít hạt',            1, GETUTCDATE()),
(N'Khoai lang mật',           N'DD006', @CatRauCu,    @SupMEK, N'kg',    18000,  28000,  15, N'Khoai lang mật ngọt tự nhiên',              1, GETUTCDATE()),

-- 🍌 TRÁI CÂY
(N'Chuối tiêu già',           N'DD007', @CatTraiCay,  @SupMEK, N'nải',  22000,  35000,  15, N'Chuối tiêu già vàng, ngọt thơm',            1, GETUTCDATE()),
(N'Xoài cát Hòa Lộc',        N'DD008', @CatTraiCay,  @SupMEK, N'kg',   38000,  58000,  10, N'Xoài cát Hòa Lộc đặc sản Tiền Giang',       1, GETUTCDATE()),
(N'Bưởi da xanh Bến Tre',    N'DD009', @CatTraiCay,  @SupMEK, N'quả',  38000,  55000,  10, N'Bưởi da xanh múi ngọt, ít chua',            1, GETUTCDATE()),
(N'Thanh long ruột đỏ',      N'DD010', @CatTraiCay,  @SupMEK, N'kg',   28000,  42000,  10, N'Thanh long ruột đỏ Bình Thuận',             1, GETUTCDATE()),
(N'Dứa Queen Ninh Bình',     N'DD011', @CatTraiCay,  @SupNSV, N'quả',  18000,  28000,  12, N'Dứa Queen thơm ngọt, nhiều nước',           1, GETUTCDATE()),

-- 🥩 THỊT TƯƠI
(N'Thịt heo ba chỉ',         N'DD012', @CatThit,     @SupNSV, N'kg',  125000, 160000,  10, N'Thịt heo ba chỉ tươi, tỷ lệ nạc mỡ đều',  1, GETUTCDATE()),
(N'Thịt bò thăn nội',        N'DD013', @CatThit,     @SupNSV, N'500g',90000, 135000,   8, N'Thịt bò thăn trong nước, màu đỏ tươi',     1, GETUTCDATE()),
(N'Thịt gà ta nguyên con',   N'DD014', @CatThit,     @SupNSV, N'con',  95000, 130000,   8, N'Gà ta thả vườn, thịt chắc thơm',           1, GETUTCDATE()),
(N'Sườn heo non',            N'DD015', @CatThit,     @SupNSV, N'kg',  115000, 148000,   8, N'Sườn heo non mềm, ít xương',               1, GETUTCDATE()),
(N'Giò heo (chân giò)',      N'DD016', @CatThit,     @SupNSV, N'kg',   85000, 110000,  10, N'Chân giò heo tươi, thích hợp kho, hầm',    1, GETUTCDATE()),

-- 🦐 HẢI SẢN
(N'Tôm sú tươi sống',        N'DD017', @CatHaiSan,   @SupBDE, N'kg',  165000, 220000,   8, N'Tôm sú size 20 con/kg, còn sống',          1, GETUTCDATE()),
(N'Cá basa phi lê đông lạnh',N'DD018', @CatHaiSan,   @SupBDE, N'kg',   55000,  82000,  10, N'Cá basa phi lê sạch, đông IQF',            1, GETUTCDATE()),
(N'Mực ống tươi',            N'DD019', @CatHaiSan,   @SupBDE, N'kg',   78000, 110000,   8, N'Mực ống tươi ngày, thịt trắng giòn',       1, GETUTCDATE()),
(N'Cua biển Cà Mau',         N'DD020', @CatHaiSan,   @SupBDE, N'kg',  195000, 260000,   5, N'Cua biển Cà Mau gạch son, chắc thịt',      1, GETUTCDATE()),

-- 🥛 SỮA & TRỨNG
(N'Sữa tươi Vinamilk nguyên kem 1L', N'DD021', @CatSuaTrung, @SupVNM, N'hộp',  27000,  36000, 50, N'Sữa tươi tiệt trùng nguyên kem Vinamilk', 1, GETUTCDATE()),
(N'Trứng gà ta (vỉ 10 quả)', N'DD022', @CatSuaTrung, @SupNSV, N'vỉ',   26000,  38000,  30, N'Trứng gà ta thả vườn, vỏ nâu',             1, GETUTCDATE()),
(N'Phô mai Con Bò Cười (6 miếng)', N'DD023', @CatSuaTrung, @SupVNM, N'gói', 40000, 55000, 20, N'Phô mai Con Bò Cười nhập khẩu Pháp',      1, GETUTCDATE()),
(N'Sữa chua Vinamilk (lốc 4 hộp)', N'DD024', @CatSuaTrung, @SupVNM, N'lốc', 28000,  38000,  30, N'Sữa chua Vinamilk không đường, bổ dưỡng',  1, GETUTCDATE()),

-- 🥤 ĐỒ UỐNG
(N'Nước ngọt Pepsi (24 lon)', N'DD025', @CatDoUong,   @SupNSV, N'thùng',195000, 255000, 15, N'Pepsi lon 330ml, thùng 24 lon',            1, GETUTCDATE()),
(N'Nước suối Lavie 500ml',   N'DD026', @CatDoUong,   @SupNSV, N'chai',   3500,   5500,  50, N'Nước khoáng thiên nhiên Lavie',             1, GETUTCDATE()),
(N'Trà xanh Không Độ 500ml', N'DD027', @CatDoUong,   @SupNSV, N'chai',   8000,  12000,  30, N'Trà xanh Không Độ vị truyền thống',        1, GETUTCDATE()),
(N'Sữa đậu nành Fami 200ml', N'DD028', @CatDoUong,   @SupVNM, N'hộp',   5500,   8500,  40, N'Sữa đậu nành Fami thơm béo',               1, GETUTCDATE()),

-- 🧂 GIA VỊ & THỰC PHẨM KHÔ
(N'Nước mắm Chinsu 500ml',   N'DD029', @CatGiaVi,    @SupNSV, N'chai',  22000,  32000,  30, N'Nước mắm Chinsu cao cấp 40 độ đạm',        1, GETUTCDATE()),
(N'Gạo ST25 túi 5kg',        N'DD030', @CatKho,      @SupMEK, N'túi',   82000, 108000,  20, N'Gạo ST25 ngon nhất thế giới, thơm dẻo',    1, GETUTCDATE());
GO

-- ============================================================
-- 4. TỒN KHO (Inventory Batches) cho 30 sản phẩm
-- Mỗi sản phẩm 1 lô, số lượng đủ để test
-- ============================================================
DECLARE @Now DATETIME = GETUTCDATE();

INSERT INTO InventoryBatches (ProductId, BatchCode, Quantity, ExpiryDate, ManufactureDate, ReceivedDate)
SELECT
    p.Id,
    'BATCH-' + p.SKU + '-001',
    CASE
        WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Rau củ tươi')   THEN 100
        WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Trái cây')      THEN 80
        WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Thịt tươi')     THEN 40
        WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Hải sản tươi sống') THEN 30
        WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Sữa & Trứng')  THEN 150
        WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Đồ uống')       THEN 200
        WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Gia vị & Nước chấm') THEN 120
        ELSE 100
    END,
    DATEADD(MONTH,
        CASE
            WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Rau củ tươi')   THEN 1
            WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Thịt tươi')     THEN 1
            WHEN p.CategoryId = (SELECT Id FROM Categories WHERE Name = N'Hải sản tươi sống') THEN 1
            ELSE 12
        END,
        @Now),
    DATEADD(MONTH, -1, @Now),
    @Now
FROM Products p
WHERE p.SKU LIKE 'DD%';
GO
