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
            if (String.IsNullOrEmpty(ln) || String.IsNullOrEmpty(ln.TrimStart()))
                return false;

            return ln.TrimStart()[0] == '#';
        }

        private int _currentLine;

        private List<MySqlMacro> _macros = new List<MySqlMacro>();

        private MySqlMacroModel _macroModel = new MySqlMacroModel();

        public string FileName { get; private set; }

        public MySqlMacroModel MacroModel { get { return _macroModel; } }

        internal int CurrentLine { get { return _currentLine; } }

        private bool IsDefined(string macroName)
        {
            foreach (MySqlMacro macro in _macros)
                if (macro.Name.Equals(macroName))
                    return true;

            return false;
        }

        private bool IsDefined(PreprocessorDirective directive)
        {
            return IsDefined(directive.Directive);
        }

        private void InitializeDom()
        {
            _macroModel.RegisterDirective("define", typeof(MacroDirective), new EventHandler(ehProcessMacro));

            //_macroModel.RegisterDirective("undefine", typeof(MacroDirective), new EventHandler(ehProcessMacro));
            //_macroModel.RegisterDirective("if", typeof(MacroDirective), new EventHandler(ehProcessMacro));
            //_macroModel.RegisterDirective("ifnot", typeof(MacroDirective), new EventHandler(ehProcessMacro));
            //_macroModel.RegisterDirective("elif", typeof(MacroDirective), new EventHandler(ehProcessMacro));
            //_macroModel.RegisterDirective("message", typeof(MacroDirective), new EventHandler(ehProcessMacro));
            //_macroModel.RegisterDirective("warning", typeof(MacroDirective), new EventHandler(ehProcessMacro));
        }

        private void DefineMacro(string name, string content)
        {
            _macros.Add(new MySqlMacro(name, content));
        }

        private void DefineMacro(MacroDirective directive)
        {
            DefineMacro(directive.MacroName, directive.MacroContent);
        }

        public void ProcessCode()
        {
            MacroModel.CodeDocument = this;
            _currentLine = 1;
            _macros.Clear();
            StreamReader reader = new StreamReader(FileName);

            string ln = null;
            while ((ln = reader.ReadLine()) != null)
            {
                if (IsPreprocessorDirective(ln))
                    MacroModel.Process(ln);

                _currentLine++;
            }
        }

        public MySqlCodeDoc(string fileName)
        {
            this.FileName = fileName;

            InitializeDom();
        }

        private void ehProcessMacro(object sender, EventArgs args)
        {
            MacroDirective directive = sender as MacroDirective;
            if (directive != null)
                DefineMacro(directive);
        }

    }
}
