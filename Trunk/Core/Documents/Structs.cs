#region Source information

//*****************************************************************************
//
//   Structs.cs
//   Created by Adam Halassy (2012.03.26. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide.  This is an unpublished work.
//
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Documents
{
    public struct MySqlMacro
    {
        private string
            _name;

        public string Name { get { return _name; } }

        public MySqlMacro(string name)
        {
            _name = name;
        }
    }
}
