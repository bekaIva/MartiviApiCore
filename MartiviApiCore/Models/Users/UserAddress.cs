using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MartiviApiCore.Models.Users
{
    public class UserAddress
    {
        public bool IsPrimary { get; set; }
        /// <summary>
        /// Gets or sets the property that holds the customer id.
        /// </summary>
        public int UserAddressId { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with a label, which displays the customer name.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the address type.
        /// </summary>
        public string AddressType { get; set; }


        public string Coordinates { get; set; }
        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the customer address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the customer mobile number.
        /// </summary>
        public string MobileNumber { get; set; }
    }
    public class OrderAddress
    {
        public bool IsPrimary { get; set; }
        /// <summary>
        /// Gets or sets the property that holds the customer id.
        /// </summary>
        public int OrderAddressId { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with a label, which displays the customer name.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the address type.
        /// </summary>
        public string AddressType { get; set; }

        public string Coordinates { get; set; }
        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the customer address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the customer mobile number.
        /// </summary>
        public string MobileNumber { get; set; }
    }
}
