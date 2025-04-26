using System.Data;
using Dapper;

namespace HotelMangSys.Services
{
    public class DapperWrapper : IDapperWrapper
    {
        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? param = null, IDbTransaction? transaction = null)
        {
            return await connection.QueryAsync<T>(sql, param, transaction);
        }

        public async Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object? param = null, IDbTransaction? transaction = null)
        {
            return await connection.QuerySingleAsync<T>(sql, param, transaction);
        }

        public async Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null, IDbTransaction? transaction = null)
        {
            return await connection.ExecuteAsync(sql, param, transaction);
        }
    }
}