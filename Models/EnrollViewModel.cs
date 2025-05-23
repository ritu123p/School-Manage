using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;     

namespace schoool_Management.Models
{
    public class EnrollViewModel
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public string CourseName { get; set; }

        public CourseModel Course { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public string StudentName { get; set; }

        public StudentModel Student { get; set; }

        [DataType(DataType.Date)]
        public DateTime EnrollDate { get; set; } = DateTime.Now;
    }
}
