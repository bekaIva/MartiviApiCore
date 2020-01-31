
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MartiviApi.Models.Users
{
    public class UpdateModel
    {
        public string ProfileImageUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string UserAddress { get; set; }
        public string Password { get; set; }
    }
}
