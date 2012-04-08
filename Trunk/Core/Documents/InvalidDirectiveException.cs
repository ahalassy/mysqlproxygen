#region Source information

//*****************************************************************************
//
//   InvalidDirectiveException.cs
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
    public class InvalidDirectiveException : Exception
    {
        public string Directive { get; private set; }

        public InvalidDirectiveException(string msg)
            : base(msg) { }

        public InvalidDirectiveException(string directive, string msg)
            : base(msg)
        {
            this.Directive = directive;
        }

        public InvalidDirectiveException(string directive, string msg, Exception innerException)
            : base(msg, innerException)
        {
            this.Directive = directive;
        }
    }
}
