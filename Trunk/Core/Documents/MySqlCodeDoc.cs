#region Source information

//*****************************************************************************
//
//   MySqlCodeDoc.cs
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
using System.IO;

namespace MySqlDevTools.Documents
{
    public class MySqlCodeDoc
    {
        public static bool IsPreprocessorDirective(string ln)
        {
            return ln.TrimStart()[0] == '#';
        }

        private bool
            _ignore = false;

        private List<MySqlMacro> _macros = new List<MySqlMacro>();

        public string FileName { get; private set; }

        private bool IsDefined(string macroName)
        {
            foreach (MySqlMacro macro in _macros)
                if (macro.Name.Equals(macroName))
                    return true;

            return false;
        }

        private void DefineMacro(string name)
        {
            _macros.Add(new MySqlMacro(name));
        }

        private void ProcessDirective(PreprocessorDirective directive)
        {
            switch (directive.Directive)
            {
                case "define":
                    break;

                case "if": break;
                case "else": break;
                case "elif": break;
                case "endif": break;

                case "include": break;
            }
        }

        public void ProcessCode()
        {
            _macros.Clear();
            StreamReader reader = new StreamReader(FileName);

            string ln = null;
            while ((ln = reader.ReadLine()) != null)
            {
                if (IsPreprocessorDirective(ln))
                    ProcessDirective(new PreprocessorDirective(ln));
            }
        }

        public MySqlCodeDoc(string fileName)
        {
            this.FileName = fileName;
        }

    }
}
