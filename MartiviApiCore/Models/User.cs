using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MartiviApiCore.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
    }
}
