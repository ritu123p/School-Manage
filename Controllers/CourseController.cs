using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using schoool_Management.Filters;
using schoool_Management.Models;
using schoool_Management.Models.ViewModels;
//using schoool_Management.ViewModels;
using System.IO;

namespace schoool_Management.Controllers
{
    [NoCache]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var teacher = _context.Teachers.FirstOrDefault(t => t.Email == user.Email);
            if (teacher == null)
            {
                return RedirectToAction("Create", "Teacher");
            }
            var courses = _context.Courses.Where(c => c.TeacherId == teacher.Id).ToList();
            return View(courses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseCreateViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var teacher = _context.Teachers.FirstOrDefault(t => t.Email == user.Email);

            if (teacher == null)
            {
                ModelState.AddModelError("", "Teacher not found.");
                return View(viewModel);
            }
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            string? uniqueFileName = null;

            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageFile.CopyToAsync(fileStream);
                }
            }

            var model = new CourseModel
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Price = viewModel.Price,
                ImageName = uniqueFileName,
                TeacherId = teacher.Id
            };
            _context.Courses.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Teacher");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new CourseCreateViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price,
                ExistingImageName = course.ImageName
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseCreateViewModel viewModel)
        {
            if (viewModel.Id == 0 && viewModel.ImageFile == null && !viewModel.RemoveImage)
            {
                ModelState.AddModelError("ImageFile", "Please upload an image.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var course = await _context.Courses.FindAsync(viewModel.Id);
            if (course == null)
            {
                return NotFound();
            }

            course.Name = viewModel.Name;
            course.Description = viewModel.Description;
            course.Price = viewModel.Price;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            // Remove image if checkbox is checked
            if (viewModel.RemoveImage && !string.IsNullOrEmpty(course.ImageName))
            {
                string oldImagePath = Path.Combine(uploadsFolder, course.ImageName);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                course.ImageName = null;
            }

            // If a new image is uploaded
            if (viewModel.ImageFile != null)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageFile.CopyToAsync(fileStream);
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(course.ImageName))
                {
                    string oldImagePath = Path.Combine(uploadsFolder, course.ImageName);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                course.ImageName = uniqueFileName;
            }

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Teacher");
        }


        public IActionResult Delete(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null)
            {
                return NotFound();
            }
            _context.Courses.Remove(course);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public async Task<IActionResult> Enroll(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = _context.Students.FirstOrDefault(s => s.Email == user.Email);
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var course = _context.Courses.Find(id);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Create EnrollViewModel and pass it to the view
            var model = new EnrollViewModel
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                CourseId = course.Id,
                CourseName = course.Name
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollViewModel model)
        {
            // Validate Model State
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}
            var existingEnrollment = await _context.CourseEnrolls
                .FirstOrDefaultAsync(e => e.CourseId == model.CourseId && e.StudentId == model.StudentId);

            if (existingEnrollment != null)
            {
                ModelState.AddModelError("", "You are already enrolled in this course.");
                return View(model);
            }
            var enrollment = new CourseEnroll
            {
                CourseId = model.CourseId,
                StudentId = model.StudentId,
                EnrollDate = DateTime.Now
            };
            try
            {
                _context.CourseEnrolls.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Student");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Database error: {ex.InnerException?.Message ?? ex.Message}");
                return View(model);
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetStudentsByCourseId(int id)
        {
            var students = await _context.CourseEnrolls
                .Where(e => e.CourseId == id)
                .Select(e => new
                {
                    FullName = e.Student.FullName
                })
                .ToListAsync();

            return Json(students);
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseByCourseId(int id)
        {
            var courses = await _context.CourseEnrolls
                .Where(e => e.CourseId == id)
                .Select(e => new
                {
                    FullName = e.Course.Teacher.FullName,
                    CourseName = e.Course.Name,
                    CourseDescription = e.Course.Description,
                    CoursePrice = e.Course.Price,
                    Experience = e.Course.Teacher.Experience,
                })
                .ToListAsync();

            return Json(courses);
        }

    }
}