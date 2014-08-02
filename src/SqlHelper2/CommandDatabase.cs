using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SqlHelper2 {
    public class CommandDatabase : IDatabase {
        public readonly DbCommand Command;

        public CommandDatabase(DbCommand cmd) {
            Command = cmd;
        }

        private void PrepareCommand(string sql, object parameters) {
            Command.CommandType = CommandType.Text;
            Command.CommandText = sql;
            Command.SetParameters(parameters);
        }

        public int ExecuteNonQuery(string sql, object parameters) {
            PrepareCommand(sql, parameters);

            return Command.ExecuteNonQuery();
        }

        public IEnumerable<T> ExecuteDataReader<T>(string sql, object parameters, Func<DbDataReader, T> action) {
            PrepareCommand(sql, parameters);

            using (var dr = Command.ExecuteReader()) {
                while (dr.Read())
                    yield return action.Invoke(dr);
            }
        }

        public void ExecuteDataReader(string sql, object parameters, Action<DbDataReader> action) {
            PrepareCommand(sql, parameters);

            using (var dr = Command.ExecuteReader()) {
                while (dr.Read())
                    action.Invoke(dr);
            }
        }

        public void ExecuteTransaction(Action<IDatabase> action) {
            if (action != null)
                action.Invoke(this);
        }

        public T ExecuteScalar<T>(string sql, object parameters) {
            PrepareCommand(sql, parameters);

            return (T) Command.ExecuteScalar();
        }

        public void BulkCopy(DataTable table, int batchSize) {
            using (var bulkcopy = new SqlBulkCopy((SqlConnection) Command.Connection)) {
                if (table != null && table.Rows.Count > 0) {
                    bulkcopy.DestinationTableName = table.TableName;
                    bulkcopy.BatchSize = 100;
                    bulkcopy.WriteToServer(table);
                }
            }
        }

        public bool HasRow(string sql, object parameters) {
            Command.CommandType = CommandType.Text;
            Command.CommandText = sql;
            Command.SetParameters(parameters);

            using (var dr = Command.ExecuteReader()) {
                return dr.HasRows;
            }
        }
    }
}