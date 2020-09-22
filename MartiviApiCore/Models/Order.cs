using MaleApi.Models;
using MaleApiCore.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MaleApi.Models
{
    public enum OrderStatus
    {
        Accepted,
        Canceled,
        Completed
    }
    public enum PaymentStatus
    {
        NotPaid,
        PENDING,
        PROCESS,
        ON_HOLD,
        COMPLETED,
        CANCELED,
        NOT_FINISHED,
        SAVED,
        PREPARED,
        CLEARED,
        DENIED,
        EXPIRED,
        FAILED,
        REFUNDED,
        DECLINED,
        RETURNED
    }


    public class Order
    {
        [Column(TypeName = "decimal(18,4)")]
        public decimal DeliveryFee { get; set; }

        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus Payment { get; set; }
        public string UserUId { get; set; }
        public User User { get; set; }
        public long OrderTimeTicks { get; set; }
        public string Hash { get; set; }
        public string TransactionID { get; set; }
        public bool IsSeen { get; set; }
        [NotMapped]
        public TimeSpan OrderTime
        {
            get { return TimeSpan.FromTicks(OrderTimeTicks); }
            set { OrderTimeTicks = value.Ticks; }
        }
        public ICollection<OrderedProduct> OrderedProducts { get; set; }
        public OrderAddress OrderAddress { get; set; }
    }

    
    public class FlutterOrder:Order
    {
        public string documentId { get; set; }
        public string uid { get; set; }
    }
}