using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Website_QuanLyKhoHangThucPham.Models;
using Website_QuanLyKhoHangThucPham.Services;
using Website_QuanLyKhoHangThucPham.ViewModels;

namespace Website_QuanLyKhoHangThucPham.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;

        public AccountController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm,
            RoleManager<IdentityRole> rm, IEmailService email, IAuditService audit)
        {
            _userManager = um; _signInManager = sm;
            _roleManager = rm; _emailService = email; _auditService = audit;
        }
        private async Task<IActionResult> RedirectToLocalByRole(string? returnUrl, ApplicationUser user)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin") || roles.Contains("WarehouseStaff") || roles.Contains("SalesStaff"))
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Store");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Store");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            { ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác."); return View(model); }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                await _auditService.LogAsync(user.Id, "LOGIN", "User", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
                return await RedirectToLocalByRole(returnUrl, user);
            }
            if (result.IsLockedOut) { ModelState.AddModelError("", "Tài khoản bị khóa tạm thời."); return View(model); }
            ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginWithGoogle(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleCallback", "Account", new { returnUrl });
            var props = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            props.Parameters["prompt"] = "select_account";
            return Challenge(props, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Đăng nhập Google thất bại. Vui lòng thử lại.";
                return RedirectToAction("Login");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email)!;
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    IsActive = true
                };
                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, "Customer");
                await _userManager.AddLoginAsync(user, info);
            }
            else
            {
                var logins = await _userManager.GetLoginsAsync(user);
                if (!logins.Any(l => l.LoginProvider == info.LoginProvider))
                    await _userManager.AddLoginAsync(user, info);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            await _auditService.LogAsync(user.Id, "LOGIN_GOOGLE", "User", user.Id,
                HttpContext.Connection.RemoteIpAddress?.ToString());

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin") || roles.Contains("WarehouseStaff") || roles.Contains("SalesStaff"))
            {
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
            }

            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl) : RedirectToAction("Index", "Store");
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public async Task<IActionResult> Logout()
        {
            var uid = _userManager.GetUserId(User);
            await _auditService.LogAsync(uid, "LOGOUT", "User", uid);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        [HttpGet, Authorize(Policy = "AdminOnly")]
        public IActionResult CreateStaff()
        {
            ViewBag.Roles = new List<string> { "Admin", "WarehouseStaff", "SalesStaff" };
            return View("Admin_AccountCreateStaff");
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateStaff(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "Admin", "WarehouseStaff", "SalesStaff" };
                return View("Admin_AccountCreateStaff", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                    await _userManager.AddToRoleAsync(user, model.Role);

                TempData["Success"] = $"Tạo tài khoản nhân viên thành công cho {model.FullName}.";
                return RedirectToAction("UserList");
            }
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            ViewBag.Roles = new List<string> { "Admin", "WarehouseStaff", "SalesStaff" };
            return View("Admin_AccountCreateStaff", model);
        }
        [HttpGet]
        public IActionResult RegisterCustomer() => View("Customer_AccountRegister", new RegisterViewModel { Role = "Customer" });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCustomer(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View("Customer_AccountRegister", model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            { ModelState.AddModelError("Email", "Email này đã được sử dụng."); return View("Customer_AccountRegister", model); }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                TempData["Success"] = $"Đăng ký thành công! Chào mừng {model.FullName}.";
                return RedirectToAction("Login");
            }
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View("Customer_AccountRegister", model);
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.IsActive)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? Request.Host.Value;
                var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
                var link = Url.Action("ResetPassword", "Account",
                    new { token = token, email = user.Email },
                        scheme, host) ?? "";
                await _emailService.SendPasswordResetAsync(user.Email!, link);
            }
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
             => View(new ResetPasswordViewModel { Token = token, Email = email });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded) { TempData["Success"] = "Đặt lại mật khẩu thành công!"; return RedirectToAction("Login"); }
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            }
            return View(model);
        }

        [Authorize] public IActionResult AccessDenied() => View();

        [HttpGet, Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UserList()
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.FindByIdAsync(currentUserId!);
            var currentRoles = await _userManager.GetRolesAsync(currentUser!);
            var isAdmin = currentRoles.Contains("Admin");

            var users = _userManager.Users.ToList();
            var result = new List<(ApplicationUser User, IList<string> Roles, bool CanEdit)>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var canEdit = isAdmin && u.Id != currentUserId;
                result.Add((u, roles, canEdit));
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.AllRoles = new List<string> { "WarehouseStaff", "SalesStaff", "Customer" };
            return View("Admin_AccountUserList", result);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            { TempData["Error"] = "Không thể đổi vai trò của chính mình."; return RedirectToAction("UserList"); }

            var allowedRoles = new[] { "WarehouseStaff", "SalesStaff", "Customer" };
            if (!allowedRoles.Contains(newRole))
            { TempData["Error"] = "Vai trò không hợp lệ."; return RedirectToAction("UserList"); }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
            { TempData["Error"] = "Không thể đổi vai trò của Admin."; return RedirectToAction("UserList"); }

            var oldRole = string.Join(", ", userRoles);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            await _auditService.LogAsync(currentUserId, "CHANGE_ROLE", "User", userId,
                null, $"{oldRole} → {newRole}");
            TempData["Success"] = $"Đã đổi vai trò {user.FullName} thành {newRole}.";
            return RedirectToAction("UserList");
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ToggleUserActive(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            { TempData["Error"] = "Không thể khóa tài khoản của chính mình."; return RedirectToAction("UserList"); }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains("Admin"))
            { TempData["Error"] = "Không thể khóa tài khoản Admin khác."; return RedirectToAction("UserList"); }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var action = user.IsActive ? "mở khóa" : "khóa";
            await _auditService.LogAsync(currentUserId, $"USER_{action.ToUpper()}", "User", userId);
            TempData["Success"] = $"Đã {action} tài khoản {user.FullName}.";
            return RedirectToAction("UserList");
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var vm = new RegisterViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ??""
            };
            return View("Customer_AccountEditProfile", vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public async Task<IActionResult> EditProfile(RegisterViewModel model)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Role");
            ModelState.Remove("Email");
            if (!ModelState.IsValid) return View("Customer_AccountEditProfile", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Index", "Store");
        }
    }
}