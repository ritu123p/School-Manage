using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace schoool_Management.Models
{
    public class CourseEnroll
    {
        [Key]
        public int Id { get; set; }

        public int CourseId { get; set; }

        public CourseModel Course { get; set; }

        public int StudentId { get; set; }

        public StudentModel Student { get; set; }

        public DateTime EnrollDate { get; set; } = DateTime.Now;
    }
}
