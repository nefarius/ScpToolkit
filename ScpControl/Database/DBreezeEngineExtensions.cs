using System.Collections.Generic;
using System.Linq;
using DBreeze;
using DBreeze.DataTypes;

namespace ScpControl.Database
{
    /// <summary>
    ///     Extension methods for embedded object database.
    /// </summary>
    public static class DBreezeEngineExtensions
    {
        public static void PurgeDbTable(this DBreezeEngine engine, string table)
        {
            using (var tran = engine.GetTransaction())
            {
                tran.RemoveAllKeys(table, false);
                tran.Commit();
            }
        }

        /// <summary>
        ///     Returns all objects from a given table.
        /// </summary>
        /// <typeparam name="T">The object type to retrieve.</typeparam>
        /// <param name="engine">The database engine to query.</param>
        /// <param name="table">The table name to query.</param>
        /// <returns>A dictionary containing results as <see cref="KeyValuePair{TKey,TValue}"/>.</returns>
        public static IDictionary<string, T> GetAllDbEntities<T>(this DBreezeEngine engine, string table)
        {
            using (var tran = engine.GetTransaction())
            {
                return tran.SelectForward<string, DbCustomSerializer<T>>(table).ToDictionary(d => d.Key, d => d.Value.Get);
            }
        }

        public static T GetDbEntity<T>(this DBreezeEngine engine, string table, string key)
        {
            using (var tran = engine.GetTransaction())
            {
                return tran.Select<string, DbCustomSerializer<T>>(table, key).Value.Get;
            }
        }

        public static void PutDbEntity<T>(this DBreezeEngine engine, string table, string key, T entity)
        {
            using (var tran = engine.GetTransaction())
            {
                tran.Insert(table, key, new DbCustomSerializer<T>(entity));
                tran.Commit();
            }
        }

        public static bool DoesDbEntityExist<T>(this DBreezeEngine engine, string table, string key)
        {
            using (var tran = engine.GetTransaction())
            {
                return tran.Select<string, T>(table, key).Exists;
            }
        }

        public static bool DeleteDbEntity(this DBreezeEngine engine, string table, string key)
        {
            bool wasDeleted;

            using (var tran = engine.GetTransaction())
            {
                tran.RemoveKey(table, key, out wasDeleted);
                tran.Commit();
            }

            return wasDeleted;
        }
    }
}
