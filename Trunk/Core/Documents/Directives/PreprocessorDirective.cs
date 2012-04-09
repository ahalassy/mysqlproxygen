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
using System.Reflection;

namespace MySqlDevTools.Documents
{
    public class PreprocessorDirective
    {
        private static bool _verbose = false;

        public static bool Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }

        private string
            _directive = null,
            _args = null;

        public event EventHandler ProcessDirective;

        public string DirectiveLine { get; private set; }

        public string Arguments { get { return _args; } }

        public string Directive { get { return _directive; } }

        protected virtual void Parse()
        {
            int
                sepPos = DirectiveLine.IndexOfAny(new char[] { ' ', '\t' });

            _directive = sepPos < 0 ? DirectiveLine.Substring(1) : DirectiveLine.Substring(1, sepPos - 1);
            _args = sepPos < 0 ? "" : DirectiveLine.Substring(sepPos).Trim();
        }

        internal void InvokeProcess()
        {
            OnProcessDirective(EventArgs.Empty);
        }

        internal void Initialize(string ln)
        {
            this.DirectiveLine = ln;


            Parse();
        }

        private void OnProcessDirective(EventArgs args)
        {
            if (ProcessDirective != null)
                ProcessDirective(this, args);
        }
    }
}
