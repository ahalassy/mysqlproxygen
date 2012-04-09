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
            _content,
            _name;

        public string Name { get { return _name; } }

        public string Content { get { return _content; } }

        public override string ToString()
        {
            return String.Format("{0} = \"{1}\"", Name, Content);
        }

        public MySqlMacro(string name, string content)
        {
            _name = name;
            _content = content;
        }
    }
}
