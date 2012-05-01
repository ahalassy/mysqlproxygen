#region Source information

//*****************************************************************************
//
//   SandboxProxy.cs
//   Created by ahalassy (2012.04.30. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Sandbox\StoredProcedureExample\Services\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Halassy.Data;

namespace StoredProcedureExample.Services
{
    public class SandboxProxy : DbProxyClass
    {

        /// <summary>
        /// Sample procedure call, without return value
        /// </summary>
        /// <param name="domain">Domain</param>
        /// <param name="code">Account code</param>
        /// <param name="dsc">Account description</param>
        public void CreateAcMstr(string domain, string code, string dsc)
        {
            DbStoredRoutineParmCollection parms = new DbStoredRoutineParmCollection(this);
            parms.Add(ParameterDirection.Input, "domain", domain);
            parms.Add(ParameterDirection.Input, "code", code);
            parms.Add(ParameterDirection.Input, "dsc", dsc);

            this.RunProcedure("CreateAcMstr", parms);
        }

        public void GetAcMstr(string domain, string code, out string dsc)
        {
            DbStoredRoutineParmCollection parms = new DbStoredRoutineParmCollection(this);
            parms.Add(ParameterDirection.Input, "domain", domain);
            parms.Add(ParameterDirection.Input, "code", code);
            parms.Add(ParameterDirection.Output, "dsc", typeof(string));

            this.RunProcedure("GetAcMstr", parms);

            dsc = parms["dsc"].Value as string;
        }

        public void GetPartStock(string domain, string part, out decimal stock)
        {
            DbStoredRoutineParmCollection parms = new DbStoredRoutineParmCollection(this);
            parms.Add(ParameterDirection.Input, "domain", domain);
            parms.Add(ParameterDirection.Input, "part", part);
            parms.Add(ParameterDirection.Output, "qty_stock", typeof(decimal));

            this.RunProcedure("GetPartStock", parms);

            stock = (decimal)((parms["qty_stock"].Value as decimal?) ?? 0);
        }

        public SandboxProxy(string connectionString)
            : base(connectionString, typeof(MySqlManagementObjectFactory))
        { }

    }
}
