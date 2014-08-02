using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test {
    class OrderServiceTests {
        [Test]
        public void TestCreateOrder() {
            var order = new Order();
            order.TotalPrice = (decimal)98.5;
            order.Lines = new[] {
                new OrderDetail{Product = "P1", Amount = 2},
                new OrderDetail{Product = "P2", Amount = 3}
            };

            var orderService = new OrderService();
            orderService.CreateOrder(order);
        }
    }
}
