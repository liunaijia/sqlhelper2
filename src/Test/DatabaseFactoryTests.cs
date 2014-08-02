using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SqlHelper2;

namespace Test {
    public class DatabaseFactoryTests {
        [Test]
        public void TestGetDatabase() {
            var db = DatabaseFactory.CreateDatabase("mall");

            Assert.NotNull(db);
        }
    }
}