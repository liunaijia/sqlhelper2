using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SqlHelper2;

namespace Test {
    internal class Sample {
        public void TestInsert() {
            DatabaseFactory.CreateDatabase("mall").ExecuteNonQuery(@"insert into [Order](OrderId, Status, CreatedTime, TotalPrice) values(@OrderId, @Status, @CreatedTime, @TotalPrice)", new {OrderId = Guid.NewGuid().ToString("N"), Status = "newcreated", CreatedTime = DateTime.Now, TotalPrice = 89.2});
        }

        public void UpdateStatus(string orderId, int targetStatus) {
            DatabaseFactory.CreateDatabase("mall").ExecuteNonQuery(@"update [Order] set Status = @targetStatus where OrderId = @orderId", new {orderId, targetStatus});
        }

        public void DecreaseSellableInventory(string sku, int decreasedAmount) {
            DatabaseFactory.CreateDatabase("mall").ExecuteTransaction(db => {
                // 更新库存
                db.ExecuteNonQuery("update Inventory set Sellable -= decreasedAmount where SkuId = @sku", new {sku, decreasedAmount});

                // 查询更新后的库存量
                var sellableAmount = db.ExecuteScalar<int>("select Sellable from Inventory where SkuId = @sku", new {sku});

                // ...更新后的库存为负数，事务回滚
                if (sellableAmount < 0)
                    throw new ApplicationException("库存量不允许为负数");

                // ...更新后的库存量为0，将sku下架
                if (sellableAmount == 0)
                    db.ExecuteNonQuery("update Sku set Status = @targetStatus where SkuId = @sku", new {sku, targetStatus = "offline"});

                // 上面的三次数据库访问（更新库存、查询库存、更新SKU状态）如果出现异常，如违反数据库约束、SQL写错等，
                // 会抛出异常，整个事务回滚
            });
        }

        public IEnumerable<Order> GetOrdersByStatus(string status) {
            return DatabaseFactory.CreateDatabase("mall").ExecuteDataReader(@"select * from [Order] where Status = @status", new {status}, dr => new Order {OrderId = (string) dr["OrderId"], Status = (string) dr["Status"], CreatedTime = (DateTime) dr["CreatedTime"], TotalPrice = (decimal) dr["TotalPrice"]});
        }
    }
}