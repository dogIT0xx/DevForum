using Blog.Models.Account;
using Blog.Services.MailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Org.BouncyCastle.Asn1.X509;
using System.Data;
using System.Text;
using System.Text.Encodings.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Blog.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ISendMailService _sendMailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ISendMailService sendMailService, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _sendMailService = sendMailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string? returnUrl, RegisterModel registerModel)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new IdentityUser(); // tạo đối tượng IdentityUser với GUID
                user.UserName = registerModel.UserName;
                user.Email = registerModel.Email;
                var result = await _userManager.CreateAsync(user, registerModel.Password); // Thêm mới user với password và db

                if (result.Succeeded)
                {
                    _logger.LogInformation("Tạo mới người dùng thành công");

                    // Tạo token  để xác nhận email
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    // ActionLink tạo ra a tag 
                    // Cấu hình url token
                    // https://localhost:7277/Identity/Account/ConfirmEmail?userId=fdsfds&code=xyz&returnUrl=
                    var callbackUrl = Url.ActionLink(
                          action: nameof(RegisterConfirmation),
                          values: new { area = "Identity", userId, code, returnUrl },
                    protocol: Request.Scheme);

                    var mailContent = new MailContent(
                        registerModel.Email,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");
                    await _sendMailService.SendEmailAsync(mailContent);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToAction();
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> RegisterConfirmation(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                {
                    ViewData["Message"] = "Xác thực email thành công";
                    return View();
                }
            }
            ViewData["Message"] = "Xác thực email thất bại";
            return View("Notify");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager
                    .PasswordSignInAsync(loginModel.UserName, loginModel.Password, loginModel.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    // Chuyển hướng về trang Home
                    return LocalRedirect("/");
                }
            }
            ViewBag.Error = true;
            return View(loginModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("/");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AccountInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var accounInfoModel = new AccountInfoModel()
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                };
                return View(accounInfoModel);
            }
            ViewData["Message"] = "Lỗi";
            return View("Notify");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AccountInfoModel accountInfoModel)
        {
            var user = await _userManager.GetUserAsync(User);
            user.UserName = accountInfoModel.UserName;
            user.Email = accountInfoModel.Email;
            user.PhoneNumber = accountInfoModel.PhoneNumber;
            await _userManager.UpdateAsync(user);
            return View(nameof(AccountInfo));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
                if (user != null)
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.ActionLink(
                        action: nameof(ResetPassword),
                        values: new { area = "Identity", code },
                        protocol: Request.Scheme);

                    var mailContent = new MailContent(
                                forgotPasswordModel.Email,
                                "Quên mật khẩu",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");
                    await _sendMailService.SendEmailAsync(mailContent);
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string? code)
        {
            if (code != null)
            {
                BadRequest("Không có code");
            }

            ViewData["Code"] = code;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
                if (user != null)
                {
                    await _userManager.ResetPasswordAsync(user, resetPasswordModel.Code, resetPasswordModel.Password);
                    ViewData["Message"] = "Đổi mật khẩu thành công";
                    return View("Notify");
                }
            }
            ViewData["Message"] = "Đổi mật khẩu thất bại";
            return View("Notify");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            var userId = _userManager.GetUserId(User);
            var changePasswordModel = new ChangePasswordModel
            {
                UserId = userId
            };

            return View();
        }
    }
}
