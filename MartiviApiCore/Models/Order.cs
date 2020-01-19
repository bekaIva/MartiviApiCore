using MartiviApiCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MartiviApi.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public User User { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        
    }
}