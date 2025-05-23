using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using ASPNETCoreIdentityDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using schoool_Management.Models;
using schoool_Management.Services;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly EmailSenderService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EmailSenderService emailSender, IConfiguration configuration, JwtService jwtService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _configuration = configuration;
        _jwtService = jwtService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            ViewBag.Error = "Invalid login attempt.";
            return View();
        }

        if (!user.EmailConfirmed)
        {
            ViewBag.Error = "Your email address is not confirmed. Please confirm your email before logging in.";
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Id, role);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            // Save tokens to DB
            user.JwtToken = token;
            user.JwtTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiry;
            await _userManager.UpdateAsync(user);

            // Set tokens in cookies
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshTokenExpiry
            });

            if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Admin");
            else if (roles.Contains("Teacher"))
                return RedirectToAction("Index", "Teacher");
            else if (roles.Contains("Student"))
                return RedirectToAction("Index", "Student");

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Invalid login attempt.";
        return View();
    }

    // Add this helper method inside the same class
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }


    [HttpPost("api/login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginApi([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized("Invalid login attempt.");

        if (!user.EmailConfirmed)
            return Unauthorized("Please confirm your email before logging in.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var token = _jwtService.GenerateToken(user.Id, role);

        Response.Cookies.Append("AuthToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });


        return Ok(new { Message = "Login successful" });
    }




    private string GenerateJwtToken(IdentityUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id)
    };
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            user.JwtToken = string.Empty;
            user.JwtTokenExpiry = DateTime.MinValue;
            user.RefreshToken = string.Empty;
            user.RefreshTokenExpiry = DateTime.MinValue;
            await _userManager.UpdateAsync(user);
        }
        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }

        Response.Cookies.Delete("AuthToken");
        Response.Cookies.Delete("RefreshToken");

        await _signInManager.SignOutAsync();

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ForceLogout()
    {
        await _signInManager.SignOutAsync();

        // Clear all cookies
        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }

        return RedirectToAction("Login");
    }


    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                await SendForgotPasswordEmail(user.Email, user);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            return RedirectToAction("ForgotPasswordConfirmation", "Account");
        }

        return View(model);
    }


    private async Task SendForgotPasswordEmail(string? email, ApplicationUser? user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var passwordResetLink = Url.Action("ResetPassword", "Account",
            new { Email = email, Token = token }, protocol: HttpContext.Request.Scheme);

        var safeLink = HtmlEncoder.Default.Encode(passwordResetLink);

        var subject = "Reset Your Password";

        var messageBody = $@"
    <div style=""font-family: Arial, Helvetica, sans-serif; font-size: 16px; color: #333; line-height: 1.5; padding: 20px;"">
        <h2 style=""color: #007bff; text-align: center;"">Password Reset Request</h2>
        <p style=""margin-bottom: 20px;"">Hi {user},</p>
        
        <p>We received a request to reset your password for your <strong>School Management System</strong> account. If you made this request, please click the button below to reset your password:</p>
        
        <div style=""text-align: center; margin: 20px 0;"">
            <a href=""{safeLink}"" 
               style=""background-color: #007bff; color: #fff; padding: 10px 20px; text-decoration: none; font-weight: bold; border-radius: 5px; display: inline-block;"">
                Reset Password
            </a>
        </div>
        
        <p>If the button above doesn’t work, copy and paste the following URL into your browser:</p>
        <p style=""background-color: #f8f9fa; padding: 10px; border: 1px solid #ddd; border-radius: 5px;"">
            <a href=""{safeLink}"" style=""color: #007bff; text-decoration: none;"">{safeLink}</a>
        </p>
        
        <p>If you did not request to reset your password, please ignore this email or contact support if you have concerns.</p>
        
        <p style=""margin-top: 30px;"">Thank you,<br />School Management Team</p>
    </div>";

        await _emailSender.SendEmailAsync(email, subject, messageBody, IsBodyHtml: true);
    }


    [AllowAnonymous]
    public ActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string Token, string Email)
    {
        if (Token == null || Email == null)
        {
            ViewBag.ErrorTitle = "Invalid Password Reset Token";
            ViewBag.ErrorMessage = "The Link is Expired or Invalid";
            return View("Error");
        }
        else
        {
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Token = Token;
            model.Email = Email;
            return View(model);
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            return RedirectToAction("ResetPasswordConfirmation", "Account"); 
        }

        return View(model);
    }

    [AllowAnonymous]
    public ActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}