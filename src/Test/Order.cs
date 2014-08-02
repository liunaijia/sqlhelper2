using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Test {
    class Order {
        public string OrderId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public decimal TotalPrice { get; set; }

        public IEnumerable<OrderDetail> Lines { get; set; }

        public static Order GetByDataReader(DbDataReader dr) {
            return new Order {
                OrderId = (string)dr["OrderId"],
                Status = (string)dr["Status"],
                CreatedTime = (DateTime)dr["CreatedTime"],
                TotalPrice = (decimal)dr["TotalPrice"],
            };
        }
    }
}
