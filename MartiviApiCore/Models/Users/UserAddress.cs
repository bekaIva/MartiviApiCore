using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaleApiCore.Models.Users
{
    public class UserAddressCoordinate
    {
        public int UserAddressCoordinateId { get; set; }
        public int UserAddressId { get; set; }
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
    public class OrderAddressCoordinate
    {
        public int OrderAddressCoordinateId { get; set; }
        public int OrderAddressId { get; set; }
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
    public class UserAddress
    {
        public bool IsPrimary { get; set; }
        /// <summary>
        /// Gets or sets the property that holds the customer id.
        /// </summary>
        public int UserAddressId { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the property that has been bound with a label, which displays the customer name.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the address type.
        /// </summary>
        public string AddressType { get; set; }


        public UserAddressCoordinate Coordinates { get; set; }
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
        public int OrderId { get; set; }
        /// <summary>
        /// Gets or sets the property that has been bound with a label, which displays the customer name.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the property that has been bound with label, which displays the address type.
        /// </summary>
        public string AddressType { get; set; }

        public OrderAddressCoordinate Coordinates { get; set; }
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
