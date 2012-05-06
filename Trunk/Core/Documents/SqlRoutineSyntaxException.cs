#region Source information

//*****************************************************************************
//
//   SqlRoutineSyntaxException.cs
//   Created by ahalassy (2012.05.01. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Core\Documents\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Documents
{
    public class SqlRoutineSyntaxException: Exception
    {
        public string RoutineName { get; private set; }

        public SqlRoutineSyntaxException(string routineName, string message)
            : base(String.Format("{0} in {1}!", message, routineName)) 
        {
            this.RoutineName = routineName;
        }

    }
}
