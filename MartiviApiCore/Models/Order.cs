using MartiviApi.Models;
using MartiviApiCore.Models.Users;
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
    public enum PaymentStatus
    {
        Paid,
        NotPaid
    }
    public class Order
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus Payment { get; set; }
        public User User { get; set; }
        public long OrderTimeTicks { get; set; }

        [NotMapped]
        public TimeSpan OrderTime
        {
            get { return TimeSpan.FromTicks(OrderTimeTicks); }
            set { OrderTimeTicks = value.Ticks; }
        }
        public ICollection<OrderedProduct> OrderedProducts { get; set; }
        public OrderAddress OrderAddress { get; set; }
    }
    public class CanceledOrder
    {
        public int CanceledOrderId { get; set; }
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus Payment { get; set; }
        public User User { get; set; }
        public long OrderTimeTicks { get; set; }
        public OrderAddress OrderAddress { get; set; }

        [NotMapped]
        public TimeSpan OrderTime
        {
            get { return TimeSpan.FromTicks(OrderTimeTicks); }
            set { OrderTimeTicks = value.Ticks; }
        }
        public ICollection<OrderedProduct> OrderedProducts { get; set; }

    }
    public class CompletedOrder
    {
        public int CompletedOrderId { get; set; }
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus Payment { get; set; }
        public User User { get; set; }
        public long OrderTimeTicks { get; set; }
        public OrderAddress OrderAddress { get; set; }
        [NotMapped]
        public TimeSpan OrderTime
        {
            get { return TimeSpan.FromTicks(OrderTimeTicks); }
            set { OrderTimeTicks = value.Ticks; }
        }
        public ICollection<OrderedProduct> OrderedProducts { get; set; }

    }
}