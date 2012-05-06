#region Source information

//*****************************************************************************
//
//   AssemblyCompilerException.cs
//   Created by ahalassy (2012.05.06. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Core\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;

namespace MySqlDevTools
{
    public class AssemblyCompilerException : Exception
    {
        public CompilerResults CompilerResults { get; private set; }

        public AssemblyCompilerException(string msg, CompilerResults compilerResults)
            : base(msg)
        {
            this.CompilerResults = compilerResults;
        }
    }
}
