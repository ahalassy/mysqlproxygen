#region Source information

//*****************************************************************************
//
//   Enumerations.cs
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
	#region public enum ProxySessionManagementMethod
	/// <summary>
	/// Proxy connection management method
	/// </summary>
	public enum ProxySessionManagementMethod
	{
		/// <summary>Session will opened and closed on demand</summary>
		OnDemand,
		
		/// <summary>Session will opened when object constructed, and kept opened till dispose</summary>
		Continous,
		
		/// <summary>Session state will be managed manually</summary>
		Manual
	}
	#endregion
	
    #region public enum CommandExecution
    /// <summary>SQL Command execute mode</summary>
    public enum CommandExecution
    {
        /// <summary>Execute and return with DataTable</summary>
        Query,

        /// <summary>Execute and returns a scalar value</summary>
        Scalar,

        /// <summary>Execute, but no return value</summary>
        Command

    }

    #endregion
}
