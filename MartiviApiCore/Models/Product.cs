﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MartiviApi.Models
{
    public class Product
    {
        public int CategoryId { get; set; }


        public int ProductId { get; set; }

        public string Description { get; set; }     

        public string Name { get; set; }

        public string Image { get; set; }

        public string Weight { get; set; }

        public double Price { get; set; }

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

        public double Price { get; set; }

        public int Quantity { get; set; }
    }

}