using com.etsoo.Core.Database;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using System;
using System.Collections.Generic;
using System.Data;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Local data parameter parser
    /// 本地数据参数解析器
    /// </summary>
    public class DataParameterParser : IDataParameterParser
    {
        // Add data parameter record definition
        // Static cache for improving performance
        private static Dictionary<Tuple<Type, bool?, bool>, Tuple<string, SqlMetaData[]>> AddDataParameterRecords = new Dictionary<Tuple<Type, bool?, bool>, Tuple<string, SqlMetaData[]>>()
        {
            {
                new Tuple<Type, bool?, bool>(typeof(byte), false, false),
                new Tuple<string, SqlMetaData[]>("et_byte_ids",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.TinyInt)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(short), false, false),
                new Tuple<string, SqlMetaData[]>("et_short_ids",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.SmallInt)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(short), true, false),
                new Tuple<string, SqlMetaData[]>("et_short_ids1",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.SmallInt),
                        new SqlMetaData("row_index", SqlDbType.SmallInt, true, false, SortOrder.Unspecified, -1)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(int), false, false),
                new Tuple<string, SqlMetaData[]>("et_int_ids",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.Int)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(int), true, false),
                new Tuple<string, SqlMetaData[]>("et_int_ids1",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.Int),
                        new SqlMetaData("row_index", SqlDbType.Int, true, false, SortOrder.Unspecified, -1)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(int), null, false),
                new Tuple<string, SqlMetaData[]>("et_int_data",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.Int),
                        new SqlMetaData("row_index", SqlDbType.Int, true, false, SortOrder.Unspecified, -1)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(long), false, false),
                new Tuple<string, SqlMetaData[]>("et_long_ids",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.BigInt)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(long), true, false),
                new Tuple<string, SqlMetaData[]>("et_long_ids1",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.BigInt),
                        new SqlMetaData("row_index", SqlDbType.Int, true, false, SortOrder.Unspecified, -1)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(string), false, false),
                new Tuple<string, SqlMetaData[]>("et_string_ids",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.VarChar, 30)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(string), true, false),
                new Tuple<string, SqlMetaData[]>("et_string_ids1",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.VarChar, 30),
                        new SqlMetaData("row_index", SqlDbType.Int, true, false, SortOrder.Unspecified, -1)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(string), false, true),
                new Tuple<string, SqlMetaData[]>("et_long_string_ids",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.VarChar, 50)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(string), true, true),
                new Tuple<string, SqlMetaData[]>("et_long_string_ids1",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.VarChar, 50),
                        new SqlMetaData("row_index", SqlDbType.Int, true, false, SortOrder.Unspecified, -1)
                    }
                )
            },
            {
                new Tuple<Type, bool?, bool>(typeof(string), null, false),
                new Tuple<string, SqlMetaData[]>("et_data",
                    new SqlMetaData[] {
                        new SqlMetaData("id", SqlDbType.NVarChar, 256),
                        new SqlMetaData("row_index", SqlDbType.Int, true, false, SortOrder.Unspecified, -1)
                    }
                )
            }
        };

        /// <summary>
        /// Parse records
        /// When writing common language runtime (CLR) applications, you should re-use existing SqlDataRecord objects instead of creating new ones every time.
        /// Creating many new SqlDataRecord objects could severely deplete memory and adversely affect performance.
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.server.sqldatarecord?view=dotnet-plat-ext-3.1
        /// </summary>
        /// <typeparam name="T">Item data type</typeparam>
        /// <param name="items">Items</param>
        /// <param name="metaData">Meta data</param>
        /// <returns>Records</returns>
        private static IEnumerable<SqlDataRecord> AddDataParameterParser<T>(IEnumerable<T> items, SqlMetaData[] metaData)
        {
            // Record
            var record = new SqlDataRecord(metaData);

            foreach (var item in items)
            {
                // Write data
                record.SetValue(0, item);

                // Return
                yield return record;
            }
        }

        /// <summary>
        /// Add data parameter
        /// 添加数据列表参数
        /// </summary>
        /// <typeparam name="T">Type generic</typeparam>
        /// <param name="items">Data list</param>
        /// <param name="parameters">Operation data</param>
        /// <param name="name">Parameter name</param>
        /// <param name="hasRowIndex">Has row index</param>
        /// <param name="flag">Flag value</param>
        public void AddDataParameter<T>(IEnumerable<T> items, Dictionary<string, dynamic> parameters, string name, bool? hasRowIndex, bool flag = false) where T : IComparable
        {
            Tuple<string, SqlMetaData[]> dr;
            if (AddDataParameterRecords.TryGetValue(new Tuple<Type, bool?, bool>(typeof(T), hasRowIndex, flag), out dr))
            {
                parameters[name] = new SqlParameter(name, SqlDbType.Structured) { Value = AddDataParameterParser(items, dr.Item2), TypeName = dr.Item1 };
            }
            else
            {
                throw new InvalidOperationException(nameof(AddDataParameter));
            }
        }
    }
}