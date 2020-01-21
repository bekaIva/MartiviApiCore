using MartiviApiCore.Models;
using MartiviSharedLib.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MartiviApi.Models
{
    public class Order: OrderBase
    {
        public User User { get; set; }
        
    }
}