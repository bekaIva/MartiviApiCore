using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MaleApi.Models
{
    public class Product
    {
        public int CategoryId { get; set; }


        public int ProductId { get; set; }

        public string Description { get; set; }     

        public string Name { get; set; }

        public string Image { get; set; }

        public string Weight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        public int QuantityInSupply { get; set; }

        public int Quantity { get; set; }
    }
    public class OrderedProduct
    {
        public int OrderedProductId { get; set; }

        public int CategoryId { get; set; }


        public int ProductId { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public string Weight { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }

}