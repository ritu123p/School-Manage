using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using schoool_Management.Models;
using NuGet.DependencyResolver;

namespace schoool_Management.Models
{
    public class CourseModel
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public int Price { get; set; }

        public string? ImageName { get; set; }
   
        public int TeacherId { get; set; }

        // Navigation Property
        public TeacherModel Teacher { get; set; }

        //public ICollection<CourseModel> ECoursenrolls { get; set; }
    }

}
