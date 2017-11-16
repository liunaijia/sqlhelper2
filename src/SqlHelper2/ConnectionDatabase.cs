﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Text.RegularExpressions;

namespace SqlHelper2
{
    public class ConnectionDatabase : IDatabase
    {
        private readonly string _ConnectionString;
        private readonly DbProviderFactory _ProviderFactory;

        public ConnectionDatabase(string connectionStringName)
        {
            // 此处是否应该采用多例模式，缓存 providerFactory？
            // 经测试，5秒内可创建20万个Database对象，不必进行缓存
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            var connectionString = connectionStringSettings.ConnectionString;
            _ConnectionString = connectionString;
            _ProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
        }

        public ConnectionDatabase(string connectionString, string providerName)
        {
            _ConnectionString = connectionString;
            _ProviderFactory = DbProviderFactories.GetFactory(providerName);
        }

        private DbConnection CreateConnection()
        {
            var connection = _ProviderFactory.CreateConnection();
            connection.ConnectionString = _ConnectionString;
            connection.Open();
            return connection;
        }

        private DbDataAdapter CreateDataAdapter()
        {
            return _ProviderFactory.CreateDataAdapter();
        }

        public int ExecuteNonQuery(string sql, object parameters)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    return new CommandDatabase(cmd).ExecuteNonQuery(sql, parameters);
                }
            }
        }

        public IEnumerable<T> ExecuteDataReader<T>(string sql, object parameters, Func<DbDataReader, T> action)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var db = new CommandDatabase(cmd);
                    // 这里一定要用yield，这样可以延迟执行，直接用return db.ExecuteDataReader(sql, parameters, action)在执行dr.Read()的时候，cmd对象早就释放掉了
                    foreach (var r in db.ExecuteDataReader(sql, parameters, action))
                        yield return r;
                }
            }
        }

        public void ExecuteDataReader(string sql, object parameters, Action<DbDataReader> action)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var db = new CommandDatabase(cmd);
                    db.ExecuteDataReader(sql, parameters, action);
                }
            }
        }

        public DataSet ExecuteDataSet(string sql, object parameters)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    using (var adapter = CreateDataAdapter())
                    {
                        var db = new CommandDatabase(cmd, adapter);
                        return db.ExecuteDataSet(sql,parameters);

                    }
                }
            }
        }

        public DataTable ExecuteDataTable(string sql, object parameters)
        {
            var ds = ExecuteDataSet(sql, parameters);
            return (ds.Tables.Count >= 0 ? ds.Tables[0] : null);
        }

        public void ExecuteTransaction(Action<IDatabase> action)
        {
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = transaction;

                            var db = new CommandDatabase(cmd);
                            db.ExecuteTransaction(action);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public T ExecuteScalar<T>(string sql, object parameters)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var db = new CommandDatabase(cmd);
                    return db.ExecuteScalar<T>(sql, parameters);
                }
            }
        }

        public void BulkCopy(DataTable table, int batchSize)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var db = new CommandDatabase(cmd);
                    db.BulkCopy(table, batchSize);
                }
            }
        }

        public bool HasRow(string sql, object parameters)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    var db = new CommandDatabase(cmd);
                    return db.HasRow(sql, parameters);
                }
            }
        }

        public override string ToString()
        {
            return _ConnectionString;
        }

        public DataSet ExecuteSpDataSet(string procedureName, object parameters)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    using (var adapter = CreateDataAdapter())
                    {
                        var db = new CommandDatabase(cmd, adapter);

                        return db.ExecuteSpDataSet(procedureName, parameters);

                    }
                }
            }
        }

        public int ExecuteSPNonQuery(string procedureName, object parameters = null)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    return new CommandDatabase(cmd).ExecuteSPNonQuery(procedureName, parameters);
                }
            }
        }
        public int ExecuteSPNonQuery(string procedureName, IEnumerable<object> parameters = null)
        {
            var result = 0;
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            var db = new CommandDatabase(cmd);
                            result += db.ExecuteSPNonQuery(procedureName, parameters);
                            transaction.Commit();
                            return result;
                        }
                    }
                    catch {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public int ExecuteNonQuery(string sql, IEnumerable<object> parameters = null)
        {
            var result = 0;
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = transaction;

                            var db = new CommandDatabase(cmd);
                            result = db.ExecuteNonQuery(sql, parameters);
                        }

                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public int ExecuteNonQuery(IEnumerable<string> sqllist)
        {
            var result = 0;
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = transaction;

                            var db = new CommandDatabase(cmd);
                            result = db.ExecuteNonQuery(sqllist);
                        }

                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ExecuteDataSet(string sql, object parameters, Action<DataSet> action)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    using (var adapter = CreateDataAdapter())
                    {
                        var db = new CommandDatabase(cmd, adapter);
                         db.ExecuteDataSet(sql, parameters,action);

                    }
                }
            }
        }

        public void ExecuteDataTable(string sql, object parameters, Action<DataTable> action)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    using (var adapter = CreateDataAdapter())
                    {
                        var db = new CommandDatabase(cmd, adapter);
                        db.ExecuteDataTable(sql, parameters, action);

                    }
                }
            }
        }

        public void ExecuteSpDataSet(string procedureName, object parameters, Action<DataSet> action)
        {
            using (var connection = CreateConnection())
            {
                using (var cmd = connection.CreateCommand())
                {
                    using (var adapter = CreateDataAdapter())
                    {
                        var db = new CommandDatabase(cmd, adapter);
                        db.ExecuteSpDataSet(procedureName, parameters, action);

                    }
                }
            }
        }
    }
}