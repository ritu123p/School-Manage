using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using schoool_Management.Filters;
using schoool_Management.Models;

namespace schoool_Management.Controllers
{
    [Authorize(Roles = "Admin")]
    [NoCache]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Index()
        {
            ViewData["Teachers"] = _context.Teachers.ToList();
            ViewData["Students"] = _context.Students.ToList();
            return View();
        }

        //[AllowAnonymous]
        public IActionResult TeacherEdit(int id)
        {
            var teacher = _context.Teachers.Find(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> TeacherEdit(TeacherModel teacher)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(teacher);
            }

            Console.WriteLine("Editing teacher with ID: " + teacher.Id);

            var existingTeacher = await _context.Teachers.FindAsync(teacher.Id);
            if (existingTeacher == null)
            {
                return NotFound();
            }

            existingTeacher.FullName = teacher.FullName;
            existingTeacher.Email = teacher.Email;
            existingTeacher.Subject = teacher.Subject;
            existingTeacher.Gender = teacher.Gender;
            existingTeacher.Experience = teacher.Experience;

            _context.Teachers.Update(existingTeacher);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _context.Students.Find(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(StudentModel student)
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

            return RedirectToAction("Index");
        }



        public async Task<IActionResult> TeacherDelete(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            // Remove associated Identity user
            var user = await _context.Users.FindAsync(teacher.IdentityUserId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Delete(int id)
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

            return RedirectToAction("Index");
        }

    }
}
