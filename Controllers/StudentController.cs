using System.Net.WebSockets;
using System.Security.Claims;
using System.Text.Encodings.Web;
using ASPNETCoreIdentityDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using schoool_Management.Models;
using System.Security.Claims;
using schoool_Management.Filters;

namespace schoool_Management.Controllers
{
    [Authorize]
    [NoCache]
    public class StudentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly EmailSenderService _emailSender;

        public StudentController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, EmailSenderService emailSender)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
        }

        private async Task SendConfirmationEmail(string email, ApplicationUser user, string fullName, int rollNo, string gender, string password)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action("ConfirmEmail", "Student",
                new { UserId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);

            var safeLink = HtmlEncoder.Default.Encode(confirmationLink);

            var emailSubject = "Welcome to School Management System! Please Confirm Your Email";

            var messageBody = $@"
    <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
        <p>Hi {fullName},</p>

        <p>Thank you for creating an account at <strong>School Management System</strong>.
        Here are the details you provided:</p>

        <ul>
            <li><strong>Full Name:</strong> {fullName}</li>
            <li><strong>Email:</strong> {email}</li>
            <li><strong>Roll No:</strong> {rollNo}</li>
            <li><strong>Gender:</strong> {gender}</li>
            <li><strong>Password:</strong> {password}</li>
        </ul>

        <p>To start enjoying all of our features, please confirm your email address by clicking the button below:</p>

        <p>
            <a href=""{safeLink}""
               style=""background-color:#007bff;color:#fff;padding:10px 20px;text-decoration:none;
                      font-weight:bold;border-radius:5px;display:inline-block;"">
                Confirm Email
            </a>
        </p>

        <p>If the button doesn’t work for you, copy and paste the following URL into your browser:
            <br />
            <a href=""{safeLink}"" style=""color:#007bff;text-decoration:none;"">{safeLink}</a>
        </p>

        <p>If you did not sign up for this account, please ignore this email.</p>

        <p>Thanks,<br />
        School Management Team</p>
    </div>
    ";

            await _emailSender.SendEmailAsync(email, emailSubject, messageBody, true);
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string Token)
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Token))
            {
                ViewBag.ErrorMessage = "The link is invalid or has expired. Please request a new one if needed.";
                return View();
            }

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "We could not find a user associated with the given link.";
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, Token);
            if (result.Succeeded)
            {
                ViewBag.Message = "Thank you for confirming your email address. Your account is now verified!";
                return View();
            }

            ViewBag.ErrorMessage = "We were unable to confirm your email address. Please try again or request a new link.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(bool IsResend = true)
        {
            if (IsResend)
            {
                ViewBag.Message = "Resend Confirmation Email";
            }
            else
            {
                ViewBag.Message = "Send Confirmation Email";
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                return View("ConfirmationEmailSent", "Student");
            }

            var teacher = _context.Students.FirstOrDefault(t => t.Email == Email);
            if (teacher == null)
            {
                ViewBag.ErrorMessage = "Unable to find teacher details for the provided email.";
                return View();
            }

            await SendConfirmationEmail(Email, user, teacher.FullName, teacher.RollNo, teacher.Gender, user.PasswordHash);

            return View("ConfirmationEmailSent", "Student");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(StudentRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool rollNoExists = _context.Students.Any(s => s.RollNo == model.RollNo);
                if (rollNoExists)
                {
                    ModelState.AddModelError("RollNo", "This Roll Number is already registered.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    JwtToken = "",
                    RefreshToken = ""
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendConfirmationEmail(model.Email, user, model.FullName, model.RollNo, model.Gender, model.Password);
                    await _userManager.AddToRoleAsync(user, "Student");

                    var student = new StudentModel
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        Gender = model.Gender,
                        RollNo = model.RollNo,
                        IdentityUserId = user.Id
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    return View("RegistrationSuccessful");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                var allCourses = await _context.Courses
                    .Include(c => c.Teacher)
                    .ToListAsync();

                ViewBag.Message = "Courses found: " + allCourses.Count;
                return View(allCourses);
            }

            ViewBag.Message = "User is not a student.";
            return View(new List<CourseModel>());
        }

        public async Task<IActionResult> Course()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.IdentityUserId == userId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var enrolledCourses = await _context.CourseEnrolls
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                .Include(e => e.Course.Teacher)
                .ToListAsync();

            return View(enrolledCourses);
        }
    }
}
