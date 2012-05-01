#region Source information

//*****************************************************************************
//
//   MySqlManagementObjectFactory.cs
//   Created by ahalassy (2012.04.15. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Halassy.DbProxy\Data\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

namespace Halassy.Data
{
    public class MySqlManagementObjectFactory : DbManagementObjectFactoryClass
    {
        private const MySqlDbType SQLTYPE_STRING = MySqlDbType.VarChar;

        protected override DbTypeMapping[] CreateMapping()
        {
            return new DbTypeMapping[] {
                new DbTypeMapping(typeof(DateTime), MySqlDbType.DateTime),
                new DbTypeMapping(typeof(byte[]), MySqlDbType.Blob),
                new DbTypeMapping(typeof(System.Int16), MySqlDbType.Int16),
                new DbTypeMapping(typeof(System.Int32), MySqlDbType.Int32),
                new DbTypeMapping(typeof(System.Int64), MySqlDbType.Int64),
                new DbTypeMapping(typeof(byte), MySqlDbType.Byte),
                new DbTypeMapping(typeof(float), MySqlDbType.Float),
                new DbTypeMapping(typeof(double), MySqlDbType.Double),
                new DbTypeMapping(typeof(System.UInt16), MySqlDbType.UInt16),
                new DbTypeMapping(typeof(System.UInt32), MySqlDbType.UInt32),
                new DbTypeMapping(typeof(System.UInt64), MySqlDbType.UInt64),
                new DbTypeMapping(typeof(decimal), MySqlDbType.Decimal),
                new DbTypeMapping(typeof(string), SQLTYPE_STRING)
                };
        }

        public override DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public override DbCommand CreateCommand(
            string sqlCommand,
            DbConnection connection
            )
        {
            return new MySqlCommand(
                 sqlCommand,
                 connection as MySqlConnection
                 );
        }

        public override void AddParameter(
            DbCommand command,
            string name,
            Type type,
            ParameterDirection direction,
            object value
            )
        {
            MySqlCommand cmd = command as MySqlCommand;

            switch (direction)
            {
                case ParameterDirection.Input:
                case ParameterDirection.InputOutput:
                    cmd.Parameters.AddWithValue(name, value);
                    break;


                case ParameterDirection.Output:
                    MySqlDbType dbType = (MySqlDbType)GetDbTypeOf(type);
                    cmd.Parameters.Add(name, dbType);
                    break;

                default:
                    throw new InvalidCastException(String.Format("Stored procedure parameter cannot be {0}!", direction));

            }

            cmd.Parameters[name].Direction = direction;
        }

        public MySqlManagementObjectFactory(DbProxyClass proxy)
            : base(proxy) { }
    }
}
