using MartiviSharedLib.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace MartiviApi.Models.Users
{
    public class RegisterModel: RegisterModelBase
    {
        [Required]
        public override string FirstName { get; set; }

        [Required]
        public override string LastName { get; set; }

        [Required]
        public override string Username { get; set; }

        [Required]
        public override string Password { get; set; }
    }
}