using System.ComponentModel.DataAnnotations;

namespace DBTask.Controllers
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Fullname is required")]
        public string FullName { get; set; }
    }
}