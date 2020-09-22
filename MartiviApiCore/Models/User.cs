using MaleApi.Models;
using MaleApi.Models.Users;
using MaleApiCore.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaleApi.Models
{
    public enum UserType
    {
        Admin,
        Client
    }
    public class User
    {
        public string ProfileImageUrl { get; set; }
        public int UserId { get; set; }
        public UserType Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }

        public virtual ICollection<UserAddress> UserAddresses { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }

        [JsonIgnore]
        public byte[] PasswordHash { get; set; }
        [JsonIgnore]
        public byte[] PasswordSalt { get; set; }
    }
}
