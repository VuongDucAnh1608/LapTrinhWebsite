using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Website_QuanLyKhoHangThucPham.Services;
using Website_QuanLyKhoHangThucPham.ViewModels;

namespace Website_QuanLyKhoHangThucPham.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _permService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionController(IPermissionService permService, RoleManager<IdentityRole> roleManager)
        {
            _permService = permService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var permissions = await _permService.GetAllAsync();
            var roles       = _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
            var rolePermMap = await _permService.GetRolePermissionMapAsync();

            var model = new PermissionMatrixViewModel
            {
                Permissions  = permissions,
                Roles        = roles,
                RolePermMap  = rolePermMap
            };
            return View("Admin_PermissionIndex", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRolePermissions(string roleName, List<int>? permissionIds)
        {
            if (roleName == "Admin")
            {
                TempData["Error"] = "Không thể chỉnh sửa quyền của Admin.";
                return RedirectToAction(nameof(Index));
            }

            await _permService.SetRolePermissionsAsync(roleName, permissionIds ?? new List<int>());
            TempData["Success"] = $"Cập nhật quyền cho '{roleName}' thành công! Có hiệu lực ngay lập tức.";
            return RedirectToAction(nameof(Index));
        }
    }
}
