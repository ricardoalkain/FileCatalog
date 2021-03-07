using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using FileCatalog.Respositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileCatalog.Respositories
{
    /*
    * NOTE: Base repository. For this sample app totally unnecessary but it's a good practice
    * to have a base repo with simple CRUD methods.
    */

    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private DbConnection _connection;

        protected IDbConnection Connection
        {
            get
            {
                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    return _connection;
                }

                _connection = SqlClientFactory.Instance.CreateConnection();
                _connection.ConnectionString = ConnectionString;
                _connection.Open();

                if (_connection.State != ConnectionState.Open)
                {

                    var ex = new InvalidOperationException("Could not open a connection with database");
                    Logger.LogError(ex, ex.Message);
                    throw ex;

                }

                return _connection;
            }
        }

        protected readonly ILogger Logger;
        protected readonly string ConnectionString;
        protected readonly string TableName;

        protected BaseRepository(IConfiguration config, ILoggerFactory loggerFactory)
        {
            var dataType = typeof(T);
            Logger = loggerFactory.CreateLogger(GetType());

            ConnectionString = config.GetConnectionString("SqlServer");

            TableName = dataType.GetCustomAttribute<TableAttribute>()?.Name ??
                throw new InvalidOperationException($"Can't create repository for {dataType.FullName}: Missing {nameof(TableAttribute)}.");

            Logger.LogDebug("Repository created.");
        }

        public virtual async Task<long> Insert(T entity)
        {
            return await Connection.InsertAsync(entity);
        }

        public virtual async Task<bool> Remove(T entity)
        {
            return await Connection.DeleteAsync(entity);
        }

        public async Task<T> GetById(long id)
        {
            return await Connection.GetAsync<T>(id);
        }

        public async virtual Task<IEnumerable<T>> GetAll()
        {
            return await Connection.GetAllAsync<T>();
        }

        public async Task<bool> Update(T entity)
        {
            return await Connection.UpdateAsync(entity);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}

