using MartiviApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MartiviApi.Models
{
    public enum OrderStatus
    {
        Accepted,
        Canceled,
        Completed
    }
    public class Order
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public User User { get; set; }
        public long OrderTimeTicks { get; set; }

        [NotMapped]
        public TimeSpan OrderTime
        {
            get { return TimeSpan.FromTicks(OrderTimeTicks); }
            set { OrderTimeTicks = value.Ticks; }
        }
        public ICollection<OrderedProduct> OrderedProducts { get; set; }
    }
}