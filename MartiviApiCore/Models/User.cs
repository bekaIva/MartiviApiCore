using MartiviSharedLib;
using MartiviSharedLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MartiviApiCore.Models
{
    public class User : UserBase
    {
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
