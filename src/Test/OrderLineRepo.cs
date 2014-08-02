using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlHelper2;

namespace Test {
    class OrderLineRepo {
        private readonly IDatabase _Database;

        public OrderLineRepo(IDatabase database) {
            if (database == null)
                throw new ArgumentNullException("database");

            _Database = database;
        }

        public IEnumerable<OrderDetail> GetByOrderId(string orderId) {
            var sql = @"select * from OrderDetail where OrderId = @orderId";
            return _Database.ExecuteDataReader<OrderDetail>(sql, new { orderId }, OrderDetail.GetByDataReader);
        }

        public void Save(string orderId, OrderDetail orderDetail) {
            var sql = @"insert into OrderDetail(OrderId, Product, Amount) values(@orderId, @Product, @Amount)";
            _Database.ExecuteNonQuery(sql, new {orderId, orderDetail.Product, orderDetail.Amount});
        }
    }
}
