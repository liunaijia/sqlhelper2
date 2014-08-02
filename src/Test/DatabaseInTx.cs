using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using SqlHelper2;

namespace Test {
    public class DatabaseInTx : IDatabase {
        private readonly DbCommand _Command;
        public DatabaseInTx(DbCommand command) {
            _Command = command;
        }
        private void PrepareCommand(string sql, object parameters) {
            _Command.CommandType = CommandType.Text;
            _Command.CommandText = sql;
            _Command.SetParameters(parameters);
        }
        public IEnumerable<T> ExecuteReader<T>(string sql, object parameters, Func<DbDataReader, T> action) {
            PrepareCommand(sql, parameters);
            using (var dr = _Command.ExecuteReader()) {
                while (dr.Read())
                    yield return action.Invoke(dr);
            }
        }
        public int ExecuteNonQuery(string sql, object parameters) {
            PrepareCommand(sql, parameters);
            return _Command.ExecuteNonQuery();
        }
        public void ExecuteTransaction(Action<IDatabase> action) {
            if (action != null)
                action.Invoke(this);
        }
    }
}
