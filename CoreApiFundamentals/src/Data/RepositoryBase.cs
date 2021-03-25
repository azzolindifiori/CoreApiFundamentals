using Microsoft.Extensions.DependencyInjection;
using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System.Linq;
using System.Data.Common;
using CoreCodeCamp.Data.IOC;
using CoreCodeCamp.Data.DatabaseRepo;

namespace CoreCodeCamp.Data
{
    [AutoRegister(ServiceLifetime.Scoped, typeof(IRepositoryBase))]
    public class RepositoryBase : IRepositoryBase
    {
        private readonly IDatabaseConnectionFactory database;
        private readonly Compiler compiler;

        public RepositoryBase(IDatabaseConnectionFactory database, Compiler compiler)
        {
            this.database = database;
            this.compiler = compiler;
        }

        public async Task<int> CreateAndReturnIdAsync(Query query)
        {
            using (var connection = await database.CreateConnectionAsync())
            {
                var queryBuilder = compiler.Compile(query);
                return await connection.QueryFirstAsync<int>(queryBuilder.Sql, queryBuilder.NamedBindings);
            }
        }

        public async Task<bool> ExecuteAsync(Query query) => await ExecAsync(query);

        public async Task<bool> BulkOperationAsync(List<Query> queries)
        {
            using (var connection = await database.CreateConnectionAsync())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var query in queries)
                        {
                            await ExecAsync(query, transaction);
                        }
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw e;
                    }
                }
            }
            return true;
        }

        public async Task<bool> DeleteAsync(Query query) => await ExecAsync(query);

        public async Task<T> GetAsync<T>(Query query, Type[] types, Func<object[], T> map, string splitOn) => (await GetListAsync(query, types, map, splitOn)).FirstOrDefault();

        public async Task<T> GetAsync<T>(Query query) => (await GetListAsync<T>(query)).FirstOrDefault();

        public async Task<IEnumerable<T>> GetListAsync<T>(Query query, Type[] types, Func<object[], T> map, string splitOn)
        {
            using (var connection = await database.CreateConnectionAsync())
            {
                var queryBuilder = compiler.Compile(query);
                return await connection.QueryAsync(queryBuilder.Sql, types, map, queryBuilder.NamedBindings, splitOn: splitOn);
            }
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(Query query)
        {
            using (var connection = await database.CreateConnectionAsync())
            {
                var queryBuilder = compiler.Compile(query);
                return await connection.QueryAsync<T>(queryBuilder.Sql, queryBuilder.NamedBindings);
            }
        }

        private async Task<bool> ExecAsync(Query query)
        {
            using (var connection = await database.CreateConnectionAsync())
            {
                var queryBuilder = compiler.Compile(query);
                return await connection.ExecuteAsync(queryBuilder.Sql, queryBuilder.NamedBindings) > 0;
            }
        }

        private async Task<bool> ExecAsync(Query query, DbTransaction transaction)
        {
            var queryBuilder = compiler.Compile(query);
            return await transaction.Connection.ExecuteAsync(queryBuilder.Sql, queryBuilder.NamedBindings, transaction) > 0;
        }
    }


    public interface IRepositoryBase
    {
        Task<bool> ExecuteAsync(Query query);
        Task<bool> BulkOperationAsync(List<Query> queries);
        Task<int> CreateAndReturnIdAsync(Query query);

        Task<bool> DeleteAsync(Query query);

        Task<T> GetAsync<T>(Query query, Type[] types, Func<object[], T> map, string splitOn);

        Task<IEnumerable<T>> GetListAsync<T>(Query query, Type[] types, Func<object[], T> map, string splitOn);

        Task<T> GetAsync<T>(Query query);

        Task<IEnumerable<T>> GetListAsync<T>(Query query);
    }


}
