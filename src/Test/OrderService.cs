using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlHelper2;

namespace Test {
    class OrderService {
        private string NewOrderId() {
            return Guid.NewGuid().ToString("N");
        }

        public void CreateOrder(Order order) {
            if (order == null)
                throw new ArgumentNullException("order");
            if (!order.Lines.Any())
                throw new ArgumentException("order.Lines");

            order.OrderId = NewOrderId();
            order.Status = "newcreated";
            order.CreatedTime = DateTime.Now;

            DatabaseFactory.CreateDatabase("mall").ExecuteTransaction(db => {
                var orderRepo = new OrderRepo(db);
                orderRepo.Save(order);

                var orderLineRepo = new OrderLineRepo(db);
                foreach (var orderLine in order.Lines)
                    orderLineRepo.Save(order.OrderId, orderLine);
            });
        }
    }
}
