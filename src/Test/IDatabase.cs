using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Test {
    public interface IDatabase {
        IEnumerable<T> ExecuteReader<T>(string sql, object parameters, Func<DbDataReader, T> action);
        int ExecuteNonQuery(string sql, object parameters);
        void ExecuteTransaction(Action<IDatabase> action);
    }
}
