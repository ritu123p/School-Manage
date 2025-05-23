using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace schoool_Management.Models.ViewModels
{
    public class CourseCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public int Price { get; set; }

        public string? ExistingImageName { get; set; }

        public bool RemoveImage { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}