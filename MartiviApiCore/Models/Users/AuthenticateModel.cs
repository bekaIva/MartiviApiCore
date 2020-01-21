using MartiviSharedLib;
using System.ComponentModel.DataAnnotations;

namespace MartiviApi.Models.Users
{
    public class AuthenticateModel: AuthenticateModelBase
    {
        [Required]
        public override string Username { get; set; }

        [Required]
        public override string Password { get; set; }
    }
}