#region Source information

//*****************************************************************************
//
//   DbManagementObjectFactoryClass.cs
//   Created by ahalassy (2012.04.30. 0:00:00)
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

namespace Halassy.Data
{
    public abstract class DbManagementObjectFactoryClass
    {
        private DbTypeMapping[] _mappings;

        public DbTypeMapping[] TypeMapping
        {
            get
            {
                List<DbTypeMapping> mapping = new List<DbTypeMapping>();
                mapping.AddRange(_mappings);
                return mapping.ToArray();
            }
        }

        public DbProxyClass OwnerProxy { get; private set; }

        protected abstract DbTypeMapping[] CreateMapping();

        protected virtual void Initialize()
        {
            _mappings = CreateMapping();
        }

        public abstract DbConnection CreateConnection(string connectionString);

        public abstract DbCommand CreateCommand(
            string sqlCommand,
            DbConnection connection
            );

        public abstract DbParameter CreateParameter(
            string name,
            Type type,
            ParameterDirection direction,
            object value
            );

        /// <summary>
        /// Determines the Db parameter type of the given .Net type
        /// </summary>
        /// <param name="type">.Net type</param>
        /// <returns>The Db parameter type</returns>
        /// <exception cref="InvalidCastException">Thrown when no mapping for the given CLR type</exception>
        public object GetDbTypeOf(Type type)
        {
            foreach (DbTypeMapping mapping in TypeMapping)
                if (mapping.IsMatch(type)) return mapping.DbType;

            throw new InvalidCastException(
                String.Format(
                    "There is no mapping for {0} CLR type!",
                    type.FullName
                    )
                );
        }

        public DbManagementObjectFactoryClass(DbProxyClass proxyClass)
        {
            this.OwnerProxy = proxyClass;

            Initialize();
        }
    }
}
