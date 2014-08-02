using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlHelper2;

namespace Test {
    class OrderRepo {
        private readonly IDatabase _Database;

        public OrderRepo(IDatabase database) {
            if (database == null)
                throw new ArgumentNullException("database");

            _Database = database;
        }

        public void Save(Order order) {
            var sql = @"insert into [Order](OrderId, Status, CreatedTime, TotalPrice) values(@OrderId, @Status, @CreatedTime, @TotalPrice)";
            _Database.ExecuteNonQuery(sql, new {order.OrderId, order.Status, order.CreatedTime, order.TotalPrice});
        }

        public int UpdateStatus(string orderId, string fromStatus, string toStatus) {
            var sql = @"update [Order] set Status = @toStatus where OrderId = @orderId and Status = @fromStatus";
            return _Database.ExecuteNonQuery(sql, new {orderId, fromStatus, toStatus});
        }

        public Order GetWithLinesByOrderId(string orderId) {
            var sql = @"select * from [Order] where OrderId = @orderId";
            return _Database.ExecuteDataReader(sql, new {orderId}, dr => {
                var order = Order.GetByDataReader(dr);

                var orderLineRepo = new OrderLineRepo(_Database);
                order.Lines = orderLineRepo.GetByOrderId(order.OrderId);
                
                return order;
            }).FirstOrDefault();
        }

        public IEnumerable<Order> GetByStatus(string status) {
            var sql = @"select * from [Order] where Status = @status order by CreatedTime";
            return _Database.ExecuteDataReader<Order>(sql, new {status}, Order.GetByDataReader);
        }
    }
}
