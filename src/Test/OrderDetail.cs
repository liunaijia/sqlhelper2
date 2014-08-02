using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Test {
    class OrderDetail {
        public string Product { get; set; }

        public int Amount { get; set; }

        public static OrderDetail GetByDataReader(DbDataReader dr) {
            return new OrderDetail {
                Product = (string) dr["Product"],
                Amount = (int) dr["Amount"]
            };
        }
    }
}
