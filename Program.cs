using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Website_QuanLyKhoHangThucPham.Data;
using Website_QuanLyKhoHangThucPham.Filters;
using Website_QuanLyKhoHangThucPham.Models;
using Website_QuanLyKhoHangThucPham.Services;

var builder = WebApplication.CreateBuilder(args);
var viVN = new System.Globalization.CultureInfo("vi-VN");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = viVN;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = viVN;

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                             | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 8;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        o.CallbackPath = "/signin-google";
        o.SaveTokens = true;

        if (builder.Environment.IsDevelopment())
        {
            o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            o.CorrelationCookie.SameSite = SameSiteMode.Lax;
        }
        else
        {
            o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            o.CorrelationCookie.SameSite = SameSiteMode.None;
        }
        o.CorrelationCookie.HttpOnly = true;
    });

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.LogoutPath = "/Account/Logout";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.HttpOnly = true;
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    o.AddPolicy("AdminOrWarehouse", p => p.RequireRole("Admin", "WarehouseStaff"));
    o.AddPolicy("SalesOrAbove", p => p.RequireRole("Admin", "WarehouseStaff", "SalesStaff"));
    o.AddPolicy("CustomerOnly", p => p.RequireRole("Customer"));
    o.AddPolicy("AnyAuthenticated", p => p.RequireRole("Admin", "WarehouseStaff", "SalesStaff", "Customer"));
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<ISePayService, SePayService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<DynamicPermissionFilter>();

builder.Services.AddControllersWithViews();

builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DokiDoki Food Store API",
        Version = "v1",
        Description = "API quản lý kho hàng thực phẩm DokiDoki — Nhóm SuperStar"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "JWT Bearer token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    await SeedAsync(scope.ServiceProvider);

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DokiDoki API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "DokiDoki API Docs";
    c.DefaultModelsExpandDepth(-1);
});

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Store}/{action=Index}/{id?}");

app.Run();

static async Task SeedAsync(IServiceProvider svc)
{
    var rm = svc.GetRequiredService<RoleManager<IdentityRole>>();
    var um = svc.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var r in new[] { "Admin", "WarehouseStaff", "SalesStaff", "Customer" })
        if (!await rm.RoleExistsAsync(r))
            await rm.CreateAsync(new IdentityRole(r));

    const string email = "vuongducanh1608@gmail.com";
    var admin = await um.FindByEmailAsync(email);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Vuong Doan Duc Anh",
            EmailConfirmed = true,
            IsActive = true
        };
        await um.CreateAsync(admin, "Admin@12345");
        await um.AddToRoleAsync(admin, "Admin");
    }
}