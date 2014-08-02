> 本项目创作过程请见[如何做个好用的数据库访问类](doc/如何做个好用的数据库访问类.md)

#快速入门

### 连接串配置
> 和普通的连接串配置一样，需要提供providerName。

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="mall" connectionString="Data Source=localhost;Initial Catalog=mall;User ID=sa;Password=***" providerName="System.Data.SqlClient"/>
  </connectionStrings>
</configuration>
```

### 插入
> 利用匿名对象构造SQL参数。

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

### 更新
> 如果sql中的参数名和匿名对象中的名称一样（大小写也一样），可以不指定匿名类的成员名称。

```c#
public void UpdateStatus(string orderId, int targetStatus) {
	DatabaseFactory.CreateDatabase("mall").ExecuteNonQuery(
		@"update [Order] set Status = @targetStatus where OrderId = @orderId",
		new {orderId, targetStatus});
}
```

### 事务处理
> 约定：在ExecuteTransaction代码块中不抛出异常提交事务，否则回滚事务。利用这个特点，可以实现业务上的完整性。

```c#
public void DecreaseSellableInventory(string sku, int decreasedAmount) {
    DatabaseFactory.CreateDatabase("mall").ExecuteTransaction(db => {
        // 更新库存
        db.ExecuteNonQuery("update Inventory set Sellable -= decreasedAmount where SkuId = @sku",
            new {sku, decreasedAmount});

        // 查询更新后的库存量
        var sellableAmount = db.ExecuteScalar<int>("select Sellable from Inventory where SkuId = @sku",
            new {sku});

        // ...更新后的库存为负数，事务回滚
        if (sellableAmount < 0)
            throw new ApplicationException("库存量不允许为负数");

        // ...更新后的库存量为0，将sku下架
        if (sellableAmount == 0)
            db.ExecuteNonQuery("update Sku set Status = @targetStatus where SkuId = @sku",
                new {sku, targetStatus = "offline"});

        // 上面的三次数据库访问（更新库存、查询库存、更新SKU状态）如果出现异常，如违反数据库约束、SQL写错等，
        // 会抛出异常，整个事务回滚
    });
}
```

### 查询
> * 使用委托从DataReader中读取数据并填充业务对象。
* **注意**，`IEnumerable`接口返回的数据是延迟执行的，“延迟”的本意是“减少计算”，但是如果使用不当，很可能反而会造成“重复计算”。对这个问题不了解的同学请阅读：[老赵的这篇文章](http://www.cnblogs.com/JeffreyZhao/archive/2009/06/08/laziness-traps.html)。

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