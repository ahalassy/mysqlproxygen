#region Source information

//*****************************************************************************
//
//   CommandLineArgException.cs
//   Created by Adam Halassy (2012.03.25. 0:00:00)
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

namespace System.Config
{
    public class CommandLineArgException: Exception
    {
        public CommandLineArgException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public CommandLineArgException(string message)
            : base(message)
        { }
    }
}
