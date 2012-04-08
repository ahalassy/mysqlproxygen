#region Source information

//*****************************************************************************
//
//   PreprocessorDirective.cs
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
    public class PreprocessorDirective
    {
        private string _args;

        public string DirectiveLine { get; private set; }

        public string Arguments { get {return _args;}}

        public string Directive {get; private set;}

        private void Parse()
        {
            

        }

        public PreprocessorDirective(string ln)
        {
            this.DirectiveLine = ln;

            Parse();

        }
    }
}
