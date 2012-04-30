#region Source information

//*****************************************************************************
//
//   Structs.cs
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
using System.Linq;
using System.Text;

namespace Halassy.Data
{
    public struct DbTypeMapping
    {
        private object _dbType;

        private Type _clrType;

        public object DbType { get { return _dbType; } }

        public Type ClrType { get { return _clrType; } }

        public bool IsMatch(Type type)
        {
            return ClrType.Equals(type);
        }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="clrType">.Net type of the parameter</param>
        /// <param name="dbType">Database type of the parameter</param>
        /// <exception cref="ArgumentException">Trown when the shipped "dbType" parameter is not an enumerated type</exception>
        public DbTypeMapping(Type clrType, object dbType)
        {
            if (!dbType.GetType().IsEnum)
                throw new ArgumentException("The dbType parameter must be an enumerated type!");

            _dbType = dbType;
            _clrType = clrType;
        }
    }
}
