using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SqlHelper2;

namespace Test {
    class BasicUsageTests {

        private IDatabase CreateDatabase() {
            return DatabaseFactory.CreateDatabase("mall");
        }

        [Test]
        public void TestInsert() {
            var rows = CreateDatabase().ExecuteNonQuery(
                @"insert into [Order](OrderId, Status, CreatedTime, TotalPrice) values(@OrderId, @Status, @CreatedTime, @TotalPrice)",
                new {
                    OrderId = Guid.NewGuid().ToString("N"),
                    Status = "newcreated",
                    CreatedTime = DateTime.Now,
                    TotalPrice = 89.2
                });

            Assert.AreEqual(1, rows);
        }

        [Test]
        public void TestUpdate() {
            // 更新一个不存在的订单状态
            var rows = CreateDatabase().ExecuteNonQuery(
                @"update [Order] set Status = @ToStatus where OrderId = @OrderId",
                new {
                    OrderId = Guid.NewGuid().ToString("N"),
                    ToStatus = "cancelled"
                });

            Assert.AreEqual(0, rows);
        }
    }
}
