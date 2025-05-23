using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.DependencyResolver;
using schoool_Management.Models;
using schoool_Management.Models.ViewModels;
using System.Threading.Tasks;
using schoool_Management.Controllers;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using ASPNETCoreIdentityDemo.Services;
using Microsoft.EntityFrameworkCore;
using schoool_Management.Filters;

[Authorize(Roles = "Teacher, Admin")]

[NoCache]
public class TeacherController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly EmailSenderService _emailSender;

    public TeacherController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, EmailSenderService emailSender)
    {
        _userManager = userManager;
        _context = context;
        _emailSender = emailSender;
    }

    private async Task SendConfirmationEmail(string email, ApplicationUser user, string fullName, string subject, string gender, int experience, string password)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action("ConfirmEmail", "Teacher",
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
            <li><strong>Subject:</strong> {subject}</li>
            <li><strong>Gender:</strong> {gender}</li>
            <li><strong>Experience:</strong> {experience} years</li>
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
            return View("ConfirmationEmailSent", "Teacher");
        }
        var teacher = _context.Teachers.FirstOrDefault(t => t.Email == Email);
        if (teacher == null)
        {
            ViewBag.ErrorMessage = "Unable to find teacher details for the provided email.";
            return View();
        }
        await SendConfirmationEmail(Email, user, teacher.FullName, teacher.Subject, teacher.Gender, teacher.Experience, user.PasswordHash);

        return View("ConfirmationEmailSent", "Teacher");
    }


    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(TeacherRegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
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
                await SendConfirmationEmail(model.Email, user, model.FullName, model.Subject, model.Gender, model.Experience, model.Password);
                await _userManager.AddToRoleAsync(user, "Teacher");

                var teacher = new TeacherModel
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Subject = model.Subject,
                    Gender = model.Gender,
                    Experience = model.Experience,
                    IdentityUserId = user.Id
                };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
                return View("RegistrationSuccessful");
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
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var allCourses = await _context.Courses
                .Include(c => c.Teacher)
                .ToListAsync();

            return View("Index", allCourses);
        }
        var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == user.Email);
        if (teacher == null)
        {
            ViewBag.ErrorMessage = "Email does not exist please regiter";
            return RedirectToAction("Login", "Account");
        }
        var teacherCourses = await _context.Courses
            .Where(c => c.TeacherId == teacher.Id)
            .ToListAsync();
        return View(teacherCourses);
    }

    public async Task<IActionResult> AllCourse()
    {
        var courses = await _context.Courses
            .Include(c => c.Teacher)
            .ToListAsync();

        return View(courses);
    }

    public IActionResult Student()
    {
        ViewData["Students"] = _context.Students.ToList();
        return View();
    }

    [HttpGet]
    public IActionResult StudentEdit(int id)
    {
        var student = _context.Students.Find(id);
        if (student == null)
        {
            return NotFound();
        }
        return View(student);
    }


    [HttpPost]
    public async Task<IActionResult> StudentEdit(StudentModel student)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is invalid.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(student);
        }

        Console.WriteLine("Editing teacher with ID: " + student.Id);

        var existingStudent = await _context.Students.FindAsync(student.Id);
        if (existingStudent == null)
        {
            return NotFound();
        }

        existingStudent.FullName = student.FullName;
        existingStudent.Email = student.Email;
        existingStudent.RollNo = student.RollNo;
        existingStudent.Gender = student.Gender;

        _context.Students.Update(existingStudent);
        await _context.SaveChangesAsync();

        return RedirectToAction("Student" , "Teacher");
    }

    public async Task<IActionResult> StudentDelete(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        // Remove associated Identity user
        var user = await _context.Users.FindAsync(student.IdentityUserId);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return RedirectToAction("Student", "Teacher");
    }
}
