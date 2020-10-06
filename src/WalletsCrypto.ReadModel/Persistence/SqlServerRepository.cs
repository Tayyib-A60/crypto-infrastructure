using Dapper;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WalletsCrypto.ReadModel.Common;

namespace WalletsCrypto.ReadModel.Persistence
{
    public class SqlServerRepository<T> : IRepository<T>
        where T : IReadEntity
    {

        private readonly IDbConnection _connection;
        public SqlServerRepository(SqlConnection connection)
        {
            _connection = connection;
        }

        private string GetTableName(string typeName) => typeName switch
        {
            "User" => "Users",
            "Address" => "Addresses",
            "Transaction" => "Transactions",
            "UnspentTransaction" => "UnspentTransactions",
            _ => throw new NotImplementedException()
        };
        private string _tableName => GetTableName(typeof(T).Name);
        
        public Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetByQueryString(string query)
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            var result = await _connection.QueryAsync<T>(query);
            return result.FirstOrDefault();

        }

        public async Task<IEnumerable<T>> GetAllByQueryString(string query)
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            var result = await _connection.QueryAsync<T>(query);
            return result;

        }


        public async Task<T> GetByIdAsync(string id)
        {
            string sQuery = $"SELECT * FROM {_tableName} WHERE ID = @ID";
            if(_connection.State == ConnectionState.Closed)
                _connection.Open();
            var result = await _connection.QueryAsync<T>(sQuery, new { ID = id });
            return result.FirstOrDefault();
        }

        public async Task InsertAsync(T entity)
        {
            try
            {   
                string insertQuery = GenerateInsertQuery();
                if(_connection.State == ConnectionState.Closed)
                    _connection.Open();
                await _connection.ExecuteAsync(insertQuery, entity);
            }
            catch (SqlException ex)
            {
                throw new RepositoryException($"Error inserting entity {entity.Id}", ex);
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                string updateQuery = GenerateUpdateQuery();
                if(_connection.State == ConnectionState.Closed)
                    _connection.Open();
                await _connection.ExecuteAsync(updateQuery, entity);

            }
            catch(SqlException ex)
            {
                throw new RepositoryException($"Error inserting entity {entity.Id}", ex);
            }
        }

        private string GenerateUpdateQuery()
        {
            var updateQuery = new StringBuilder($"UPDATE {_tableName} SET ");
            var properties = GenerateListOfProperties(GetProperties);
            properties.ForEach(property =>
            {
                if (!property.Equals("Id"))
                {
                    updateQuery.Append($"{property}=@{property},");
                }
            });
            updateQuery.Remove(updateQuery.Length - 1, 1); //remove last comma
            updateQuery.Append(" WHERE Id=@Id");
            return updateQuery.ToString();
        }

        private string GenerateInsertQuery()
        {
            var insertQuery = new StringBuilder($"INSERT INTO {_tableName} ");
            
            insertQuery.Append("(");
            var properties = GenerateListOfProperties(GetProperties);
            properties.ForEach(prop => { insertQuery.Append($"[{prop}],"); });
            insertQuery
                .Remove(insertQuery.Length - 1, 1)
                .Append(") VALUES (");
            properties.ForEach(prop => { insertQuery.Append($"@{prop},"); });
            insertQuery
                .Remove(insertQuery.Length - 1, 1)
                .Append(")");
            return insertQuery.ToString();
        }

        private IEnumerable<PropertyInfo> GetProperties => typeof(T).GetProperties();
        private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties)
        {
            return (from prop in listOfProperties let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
                where attributes.Length <= 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore" select prop.Name).ToList();
        }

    }
}