# Quick Start Guide

### Configurate Connection String
> As usual, setting the `connectionString` and `providerName`.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="mall" connectionString="Data Source=localhost;Initial Catalog=mall;User ID=sa;Password=***" providerName="System.Data.SqlClient"/>
  </connectionStrings>
</configuration>
```

### Insert
> Using anonymous object to build SQL parameters.

```c#
public void Save() {
	DatabaseFactory.CreateDatabase("mall").ExecuteNonQuery(
		@"insert into [Order](OrderId, Status, CreatedTime, TotalPrice) values(@OrderId, @Status, @CreatedTime, @TotalPrice)",
		new {
			OrderId = Guid.NewGuid().ToString("N"),
			Status = "newcreated",
			CreatedTime = DateTime.Now,
			TotalPrice = 89.2
		});
}
```

### Update
> Field name could be ignored if parameter name is same with anonymous object property name.

```c#
public void UpdateStatus(string orderId, int targetStatus) {
	DatabaseFactory.CreateDatabase("mall").ExecuteNonQuery(
		@"update [Order] set Status = @targetStatus where OrderId = @orderId",
		new {orderId, targetStatus});
}
```

### Transaction
> Committing transaction if code in `ExecuteTransaction` block  doesn't throw an exception, otherwise rollbacking transaction.

```c#
public void DecreaseSellableInventory(string sku, int decreasedAmount) {
    DatabaseFactory.CreateDatabase("mall").ExecuteTransaction(db => {
        // Update sellable amount
        db.ExecuteNonQuery("update Inventory set Sellable -= decreasedAmount where SkuId = @sku",
            new {sku, decreasedAmount});

        // Get amount after updating
        var sellableAmount = db.ExecuteScalar<int>("select Sellable from Inventory where SkuId = @sku",
            new {sku});

        // ...Throw an exception to rollback transaction if amount is negative
        if (sellableAmount < 0)
            throw new ApplicationException("sellable amount can't be negative.");

        // ...Set SKU state to offline if it's amount is zero
        if (sellableAmount == 0)
            db.ExecuteNonQuery("update Sku set Status = @targetStatus where SkuId = @sku",
                new {sku, targetStatus = "offline"});

        // If any exception was thrown in above code block, such as database is disconnected, sql statement is wrong, database constraints are violated, rollback the whole transaction, otherwise commit it.
    });
}
```

### Query
> * Using delegate to read data from `DataReader` and fill data into business object.
* **NOTICE**: the return value of `IEnumerable` type is deferred execution, which means it doesn't execute any code when it's executed, until actual filtering/ordering/projecting is asked. So it will cause repeated execution if not using properly.

```c#
public IEnumerable<Order> GetOrdersByStatus(string status) {
    return DatabaseFactory.CreateDatabase("mall").ExecuteDataReader(
        @"select * from [Order] where Status = @status",
        new { status },
        dr => new Order {
            OrderId = (string)dr["OrderId"],
            Status = (string)dr["Status"],
            CreatedTime = (DateTime)dr["CreatedTime"],
            TotalPrice = (decimal)dr["TotalPrice"]
        });
}
```

Now go have some fun exploring SqlHelper2! You can view [how to develop an easy-to-use database access class](https://github.com/liunaijia/sqlhelper2/wiki) for the development background of this project.