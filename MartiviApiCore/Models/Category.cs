using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MaleApi.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
   
  
}