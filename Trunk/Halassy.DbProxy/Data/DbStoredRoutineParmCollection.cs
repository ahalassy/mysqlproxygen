#region Source information

//*****************************************************************************
//
//   DbStoredRoutineParms.cs
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
using System.Collections;
using System.Data.Common;
using System.Data;

namespace Halassy.Data
{
    public class DbStoredRoutineParmCollection
    {
        private DbProxyClass _proxy = null;

        private List<DbStoredRoutineParm> _parms = new List<DbStoredRoutineParm>();

        public DbProxyClass Proxy { get { return _proxy; } }

        public int Count { get { return _parms.Count; } }

        public DbStoredRoutineParm this[int index]
        {
            get { return _parms[index]; }
        }

        public DbStoredRoutineParm this[string name]
        {
            get
            {
                foreach (DbStoredRoutineParm parm in _parms)
                    if (parm.Name == name)
                        return parm;

                return null;
            }
        }

        public DataTable Result { get; internal set; }

        public void Add(ParameterDirection direction, string name, Type valueType)
        {
            _parms.Add(new DbStoredRoutineParm(direction, name, valueType));
        }

        public void Add(ParameterDirection direction, string name, object initVal)
        {
            _parms.Add(new DbStoredRoutineParm(direction, name, initVal));
        }

        public void Add(ParameterDirection direction, string name, object initVal, Type valueType)
        {
            _parms.Add(new DbStoredRoutineParm(direction, name, initVal, valueType));
        }

        /// <summary>
        /// Fills the selected command with the stored parameters
        /// </summary>
        /// <param name="command">Command</param>
        public void FillParameters(DbCommand command)
        {
            command.Parameters.Clear();
            foreach (DbStoredRoutineParm parm in _parms)
            {

                Proxy.MgmtObjectFactory.AddParameter(
                    command,
                    parm.SqlName,
                    parm.ValueType,
                    parm.Direction,
                    parm.Value
                    );
            }
        }

        /// <summary>
        /// Reads the parameter values from the selected command
        /// </summary>
        /// <param name="command">Command</param>
        public void FetchParameters(DbCommand command)
        {
            foreach (DbStoredRoutineParm parm in _parms)
            {
                if (!parm.IsOutput) continue;

                parm.Value = command.Parameters[parm.SqlName].Value;
            }
        }

        public DbStoredRoutineParmCollection(DbProxyClass proxy)
        {
            _proxy = proxy;
        }

    }
}
