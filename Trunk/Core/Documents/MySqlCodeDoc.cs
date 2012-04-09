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
using MySqlDevTools.Documents.Directives;

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

        public class ProcessStackFrame
        {
            public bool WasFullfilledBranch { get; set; }

            public bool Ignore { get; set; }

            public ProcessStackFrame()
            {
                this.WasFullfilledBranch = false;
                this.Ignore = false;
            }

        }

        private int _currentLine;

        private List<MySqlMacro> _macros = new List<MySqlMacro>();

        private MySqlMacroModel _macroModel = new MySqlMacroModel();

        private List<ProcessStackFrame> _stack = new List<ProcessStackFrame>();

        private StreamReader CurrentReader { get; set; }

        public string FileName { get; private set; }

        public MySqlMacroModel MacroModel { get { return _macroModel; } }

        public ProcessStackFrame TopFrame { get { return _stack[_stack.Count - 1]; } }

        internal int CurrentLine { get { return _currentLine; } }

        internal string WorkingDir { get { return Path.GetDirectoryName(FileName); } }

        private void OpenStackFrame()
        {
            _stack.Add(new ProcessStackFrame());
        }

        private void CloseStackFrame()
        {
            _stack.Remove(TopFrame);
        }

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

            _macroModel.RegisterDirective("message", typeof(MessageDirective), new EventHandler(ehProcessMessage));
            _macroModel.RegisterDirective("warning", typeof(WarningDirective), new EventHandler(ehProcessMessage));
            _macroModel.RegisterDirective("error", typeof(ErrorDirective), new EventHandler(ehProcessMessage));

            _macroModel.RegisterDirective("ifdef", typeof(IfDefinedDirective), new EventHandler(ehProcessIfDefined));
            _macroModel.RegisterDirective("ifndef", typeof(IfNotDefineDirective), new EventHandler(ehProcessIfNotDefined));
            _macroModel.RegisterDirective("else", typeof(ElseDirective), new EventHandler(ehProcessElseDefined));
            _macroModel.RegisterDirective("elif", typeof(ElseIfDefinedDirective), new EventHandler(ehProcessElseIfDefined));
            _macroModel.RegisterDirective("elifndef", typeof(ElseIfNotDefinedDirective), new EventHandler(ehProcessElseIfNotDefined));
            _macroModel.RegisterDirective("endif", typeof(EndIfDirective), new EventHandler(ehProcessEndIfDefined));

            _macroModel.RegisterDirective("include", typeof(EndIfDirective), new EventHandler(ehProcessInclude));

            // TODO: #include
            // TODO: #function 
            // TODO: #procedure
        }

        private void DefineMacro(string name, string content)
        {
            _macros.Add(new MySqlMacro(name, content));
        }

        private void DefineMacro(MacroDirective directive)
        {
            DefineMacro(directive.MacroName, directive.MacroContent);
        }

        private string ApplyMacros(string ln)
        {
            foreach (MySqlMacro macro in _macros)
                if (!String.IsNullOrEmpty(macro.Content))
                    ln = ln.Replace(macro.Name, macro.Content);

            return ln;
        }

        private void InternalProcessCode()
        {
            try
            {
                OpenStackFrame();

                string ln = null;
                while ((ln = CurrentReader.ReadLine()) != null)
                {
                    ln = ApplyMacros(ln);

                    if (IsPreprocessorDirective(ln))
                        MacroModel.Process(ln);

                    _currentLine++;
                }
            }
            catch (ReturnProcessCodeException)
            {
                return;
            }
            finally
            {
                CloseStackFrame();
            }
        }

        public void Process()
        {
            MacroModel.CodeDocument = this;
            _currentLine = 1;
            _macros.Clear();
            CurrentReader = new StreamReader(FileName);

            InternalProcessCode();
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

        private void ehProcessMessage(object sender, EventArgs args)
        {
            if (TopFrame.Ignore) return;

            MessageDirective directive = sender as MessageDirective;
            if (directive is ErrorDirective)
                throw new Exception(String.Format("Explicit error: \"{0}\"", directive.Arguments));

            if (directive is WarningDirective)
            {
                Console.WriteLine("## Warning: \"{0}\"", directive.Arguments);
                return;
            }

            Console.WriteLine("## Message: \"{0}\"", directive.Arguments);


        }

        private void ehProcessIfDefined(object sender, EventArgs args)
        {
            ConditionalDirective directive = sender as ConditionalDirective;

            OpenStackFrame();

            bool
               defined = IsDefined(directive.Arguments),
               fullfilled = defined;

            TopFrame.Ignore = !fullfilled;
            TopFrame.WasFullfilledBranch = fullfilled;
        }

        private void ehProcessIfNotDefined(object sender, EventArgs args)
        {
            ConditionalDirective directive = sender as ConditionalDirective;

            OpenStackFrame();

            bool
               defined = IsDefined(directive.Arguments),
               fullfilled = !defined;

            TopFrame.Ignore = !fullfilled;
            TopFrame.WasFullfilledBranch = fullfilled;
        }

        private void ehProcessEndIfDefined(object sender, EventArgs args)
        {
            ConditionalDirective directive = sender as ConditionalDirective;

            CloseStackFrame();
        }

        private void ehProcessElseDefined(object sender, EventArgs args)
        {
            ConditionalDirective directive = sender as ConditionalDirective;

            TopFrame.Ignore = TopFrame.WasFullfilledBranch;
        }

        private void ehProcessElseIfDefined(object sender, EventArgs args)
        {
            ConditionalDirective directive = sender as ConditionalDirective;

            if (TopFrame.WasFullfilledBranch)
            {
                TopFrame.Ignore = true;
                return;
            }

            bool
               defined = IsDefined(directive.Arguments),
               fullfilled = defined;

            TopFrame.Ignore = !fullfilled;
            TopFrame.WasFullfilledBranch = fullfilled;
        }

        private void ehProcessElseIfNotDefined(object sender, EventArgs args)
        {
            ConditionalDirective directive = sender as ConditionalDirective;

            if (TopFrame.WasFullfilledBranch)
            {
                TopFrame.Ignore = true;
                return;
            }

            bool
               defined = IsDefined(directive.Arguments),
               fullfilled = !defined;

            TopFrame.Ignore = !fullfilled;
            TopFrame.WasFullfilledBranch = fullfilled;
        }

        private void ehProcessInclude(object sender, EventArgs args)
        {
            if (TopFrame.Ignore) return;

            PreprocessorDirective directive = sender as PreprocessorDirective;
            if (directive == null) return;

            MySqlCodeDoc codeDoc = new MySqlCodeDoc(Path.Combine(WorkingDir, directive.Arguments));
            codeDoc.Process();
        }

    }
}
