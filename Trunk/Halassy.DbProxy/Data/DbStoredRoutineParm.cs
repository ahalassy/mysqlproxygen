#region Source information

//*****************************************************************************
//
//   DbStoredRoutineParm.cs
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
using System.Data;
using System.Data.Common;

namespace Halassy.Data
{
    public class DbStoredRoutineParm
    {
        public string Name { get; private set; }

        public string SqlName { get { return "@" + Name; } }

        public object Value { get; internal set; }

        public Type ValueType { get; private set; }

        public ParameterDirection Direction { get; private set; }

        public bool IsOutput
        {
            get
            {
                return Direction == ParameterDirection.InputOutput
                    || Direction == ParameterDirection.Output;
            }
        }

        private void Initialize(
            ParameterDirection direction,
            string name,
            object initVal,
            Type valueType
            )
        {
            this.Direction = direction;
            this.Name = name;
            this.Value = initVal;
        }

        public DbStoredRoutineParm(ParameterDirection direction, string name, Type valueType)
        {
            Initialize(direction, name, null, valueType);
        }

        public DbStoredRoutineParm(ParameterDirection direction, string name, object initVal)
        {
            Initialize(direction, name, initVal, initVal.GetType());
        }

        public DbStoredRoutineParm(ParameterDirection direction, string name, object initVal, Type valueType)
        {
            Initialize(direction, name, initVal, valueType);
        }
    }
}
