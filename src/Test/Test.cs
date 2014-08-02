using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using NUnit.Framework;
using SqlHelper2;
using Test;

namespace Test {
    public class Book {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class BookRepo {
        //[Test]
        //public IDictionary<Book, int> GetAllBookSales() {
        //    var sales = new Dictionary<Book, int>();
        //    new Database("product").ExecuteReader("select * from Book", null, dr => {
        //        while (dr.Read()) {
        //            var book = new Book {Id = (int) dr["Id"], Name = (string) dr["Name"]};
        //            var amount = GetBookSales(book.Id);
        //            sales.Add(book, amount);
        //        }
        //    });

        //    foreach (var kvp in sales)
        //        Console.WriteLine(kvp.Key.Name + ":" + kvp.Value);

        //    return sales;
        //}

        //[Test]
        //public IList<Book> GetAllBooks() {
        //    var books = new List<Book>();
        //    new Database("product").ExecuteReader("select * from Book", null, dr => {
        //        var book = new Book {Id = (int) dr["Id"], Name = (string) dr["Name"]};
        //        books.Add(book);
        //    });

        //    foreach (var book in books)
        //        Console.WriteLine(book.Name);

        //    return books;
        //}

        [Test]
        public void GetAllBooks1() {
            var books = new Database("product").ExecuteReader("select * from Book", null, dr => {
                var book = new Book {Id = (int) dr["Id"], Name = (string) dr["Name"]};
                return book;
            });

            foreach (var book in books)
                Console.WriteLine(book.Name);

        }

        [Test]
        public void Update() {
            new Database("product").ExecuteNonQuery("update Book set UpdateTime = @Now", new { DateTime.Now });
        }

        [Test]
        public void ProcessMessages() {
            var messages = FindTodoMessages();

            int errors = 0;
            foreach (var message in messages) {
                try {
                    DispatchMessage(message);
                }
                catch {
                    if (++errors >= 3) 
                        throw new AppDomainUnloadedException("too many errors, abort.");
                }
            }
        }

        [Test]
        public void CreateOrder() {
            var db = new Database("order");
            db.ExecuteTransaction((tx) => {
                var orderId = tx.ExecuteReader(@"insert into [Order](Status, TotalPrice) values(@Status, @TotalPrice); select SCOPE_IDENTITY()",
                    new { @Status = "new", TotalPrice = 89.3 }, dr => Convert.ToInt32(dr[0]))
                    .FirstOrDefault();
                //throw new ApplicationException();
                tx.ExecuteNonQuery("insert into OrderDetail(OrderId, BookId, Amount) values(@OrderId, @BookId, @Amount)",
                    new {orderId, BookId = 1, Amount = 2});
            });
        }

        [Test]
        public IEnumerable<Message> FindTodoMessages() {
            return new Database("product").ExecuteReader("select * from Message where Status = @todo", new { todo = "todo" }, Message.GetByDataReader);
        }

        private void DispatchMessage(Message message) {
            throw new NotImplementedException();
        }

        //private int GetBookSales(int bookId) {
        //var sum = 0;
        //new Database("order").ExecuteReader(string.Format("select sum(Amount) from OrderDetail where BookId = @BookId"),
        //    new {BookId = bookId},
        //    dr => {
        //        if (dr.Read() && dr[0] != DBNull.Value)
        //            sum = (int) dr[0];
        //    });
        //    return sum;
        //}
    }

    public class Message {
        public int Id { get; set; }

        public string Status { get; set; }

        public static Message GetByDataReader(DbDataReader dr) {
            return new Message {
                Id = (int)dr["Id"],
                Status = (string)dr["Status"]
            };
        }
    }
}