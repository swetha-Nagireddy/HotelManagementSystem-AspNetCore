﻿using System.Data;

namespace HotelMangSys.Services
{
    /// <summary>
    /// interface for Dapper wrapper to handle database operations
    /// </summary>
    public interface IDapperWrapper
    {
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? param = null, IDbTransaction? transaction = null);
        Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object? param = null, IDbTransaction? transaction = null);
        Task<int> ExecuteAsync(IDbConnection connection, string sql, object? param = null, IDbTransaction? transaction = null);
    }
}