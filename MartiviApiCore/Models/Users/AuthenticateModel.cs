using System.ComponentModel.DataAnnotations;

namespace MaleApi.Models.Users
{
    public class AuthenticateModel
    {
        [Required]
        public  string Username { get; set; }

        [Required]
        public  string Password { get; set; }

        public bool IsAdmin { get; set; }


    }
}