using Blog.Areas.Identity.Models;
using Blog.Services.MailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Blog.Areas.Identity.Controllers
{
    [AllowAnonymous]
    [Area("Identity")]
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
                var user = new IdentityUser(registerModel.Email); // tạo đối tượng IdentityUser với email
                var result = await _userManager.CreateAsync(user, registerModel.Password); // Thêm mới user với password và db

                if (result.Succeeded)
                {
                    _logger.LogInformation("Tạo mới người dùng thành công");

                    // Tạo token  để xác nhận email
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    // https://localhost:5001/confirm-email?userId=fdsfds&code=xyz&returnUrl=
                    //var callbackUrl = Url.ActionLink(
                    //      action: nameof(ConfirmEmail),
                    //      values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    //      protocol: Request.Scheme);

                    var mailContent = new MailContent(
                        registerModel.Email,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode("Bla")}'>clicking here</a>.");

                    await _sendMailService.SendEmailAsync(mailContent);

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //    return LocalRedirect(Url.Action(nameof(RegisterConfirmation)));
                    //}
                    //else
                    //{
                    //    await _signInManager.SignInAsync(user, isPersistent: false);
                    //    return LocalRedirect(returnUrl);
                    //}
                }
            }
            return View(registerModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            return null;
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return null;
        }
    }
}
