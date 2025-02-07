using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ApiGenerator.Utility
{
    public class Database
    {
        private SqlConnection OpenDatabaseConnection()
        {
            var connectionString = $"Data Source=(local);Initial Catalog={Settings.DatabaseName};Integrated Security=True;";
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public EntityList GetEntities(string databaseObject = null)
        {
            using (var connection = OpenDatabaseConnection())
            {
                var where = databaseObject == null ? "" : $"WHERE StorageStructure = '{databaseObject}'";
                var select = $"SELECT * FROM metadata.TEntity {where} ORDER BY ComponentName, ComponentFeature, EntityName";

                System.Diagnostics.Debug.WriteLine(select);

                var table = new DataTable();
                using (var da = new SqlDataAdapter(select, connection))
                {
                    da.Fill(table);
                }

                var list = new EntityList();
                foreach (DataRow row in table.Rows)
                {
                    var entity = new Entity
                    {
                        StorageStructure = (string)row["StorageStructure"],
                        StorageSchema = (string)row["StorageSchema"],
                        StorageTable = (string)row["StorageTable"],
                        StorageKey = (string)row["StorageKey"],

                        FutureStorageTable = (string)row["FutureStorageTable"],

                        ComponentType = (string)row["ComponentType"],
                        ComponentName = (string)row["ComponentName"],
                        ComponentFeature = (string)row["ComponentFeature"],

                        EntityName = (string)row["EntityName"],

                        CollectionSlug = (string)row["CollectionSlug"],
                        CollectionKey = (string)row["CollectionKey"]
                    };

                    list.Add(entity);
                }
                return list;
            }
        }

        public DataTable GetTable(string schemaName, string tableName)
        {
            using (var connection = OpenDatabaseConnection())
            {
                var table = new DataTable(tableName);
                using (var da = new SqlDataAdapter($"SELECT * FROM [{schemaName}].[{tableName}] WHERE 1=0", connection))
                {
                    da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    da.Fill(table);
                }

                return table;
            }
        }

        public object GetScalar(string select)
        {
            using (var connection = OpenDatabaseConnection())
            {
                var command = new SqlCommand(select, connection);
                return command.ExecuteScalar();
            }
        }

        public string GetNativeType(string table, string column)
        {
            return (string)GetScalar($"select DATA_TYPE from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{table}' and COLUMN_NAME = '{column}'");
        }

        public int? GetColumnPrecision(string table, string column)
        {
            return (int?)GetScalar($"select cast(NUMERIC_PRECISION as int) from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{table}' and COLUMN_NAME = '{column}'");
        }

        public int? GetColumnScale(string table, string column)
        {
            return (int?)GetScalar($"select NUMERIC_SCALE from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{table}' and COLUMN_NAME = '{column}'");
        }

        public class EntityColumnComparer : IComparer<DataColumn>
        {
            public int Compare(DataColumn a, DataColumn b)
            {
                var aType = TypeNameOrAlias(a.DataType);
                var bType = TypeNameOrAlias(b.DataType);

                if (aType == bType)
                    return a.ColumnName.CompareTo(b.ColumnName);

                var atype = DataTypeIndex(a.DataType.Name);
                var btype = DataTypeIndex(b.DataType.Name);

                if (atype < btype)
                    return -1;

                return 1;
            }

            private int DataTypeIndex(string name)
            {
                if (name == "Guid") return 0;
                if (name == "String") return 1;
                if (name == "Boolean") return 2;
                if (name == "Int32") return 3;
                if (name == "Decimal") return 4;
                if (name.StartsWith("DateTimeOffset")) return 5;
                if (name.StartsWith("DateTime")) return 6;
                return 7;
            }
        }

        public class ConfigurationColumnComparer : IComparer<DataColumn>
        {
            public int Compare(DataColumn a, DataColumn b)
            {
                return a.ColumnName.CompareTo(b.ColumnName);
            }
        }

        // This is the set of types from the C# keyword list.
        public static Dictionary<Type, string> _aliases = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(byte[]), "byte[]" },
            { typeof(char), "char" },
            { typeof(DateTime), "DateTime" },
            { typeof(DateTimeOffset), "DateTimeOffset" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(Guid), "Guid" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
        };

        public static string TypeNameOrAlias(Type type)
        {
            if (!_aliases.ContainsKey(type))
                throw new Exception("Unexpected Type: " + type.FullName);
            return _aliases[type];
        }
    }
}
