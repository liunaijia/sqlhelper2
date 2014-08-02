using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using SqlHelper2;

namespace Test {
    public class Database : IDatabase {
        private readonly DbProviderFactory _ProviderFactory;
        private readonly string _ConnectionString;
        public Database(string connectionStringName = "*") {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            var connectionString = connectionStringSettings.ConnectionString;
            _ConnectionString = connectionString;
            _ProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
        }
        private DbConnection CreateConnection() {
            var connection = _ProviderFactory.CreateConnection();
            connection.ConnectionString = _ConnectionString;
            connection.Open();
            return connection;
        }
        public IEnumerable<T> ExecuteReader<T>(string sql, object parameters, Func<DbDataReader, T> action) {
            using (var connection = CreateConnection()) {
                using (var cmd = connection.CreateCommand()) {
                    var db = new DatabaseInTx(cmd);
                    foreach (var item in db.ExecuteReader(sql, parameters, action))
                        yield return item;
                }
            }
        }
        public int ExecuteNonQuery(string sql, object parameters) {
            using (var connection = CreateConnection()) {
                using (var cmd = connection.CreateCommand()) {
                    var db = new DatabaseInTx(cmd);
                    return db.ExecuteNonQuery(sql, parameters);
                }
            }
        }
        public void ExecuteTransaction(Action<IDatabase> action) {
            using (var connection = CreateConnection()) {
                using (var transaction = connection.BeginTransaction()) {
                    try {
                        using (var cmd = connection.CreateCommand()) {
                            cmd.Transaction = transaction;
                            var db = new DatabaseInTx(cmd);
                            db.ExecuteTransaction(action);
                        }
                        transaction.Commit();
                    }
                    catch {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
