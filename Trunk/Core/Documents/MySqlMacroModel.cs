using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace MySqlDevTools.Documents
{
    public sealed class MySqlMacroModel
    {
        #region private struct DirectiveAssignment
        private struct DirectiveAssignment
        {
            public static DirectiveAssignment Blank
            {
                get
                {
                    DirectiveAssignment result = new DirectiveAssignment();
                    result._blank = true;
                    return result;
                }
            }

            private Type
                _directiveType;

            private EventHandler
                _handler;

            private bool
                _blank;

            private string
                _directive;

            public EventHandler Handler { get { return IsBlank ? null : _handler; } }

            public bool IsBlank { get { return _blank; } }

            public string Directive { get { return IsBlank ? null : _directive; } }

            public Type DirectiveType { get { return IsBlank ? null : _directiveType; } }

            public override string ToString()
            {
                return String.Format("{0} handled by {1}", Directive, DirectiveType.FullName);
            }

            public DirectiveAssignment(string directive, Type directiveType, EventHandler handler)
            {
                _blank = false;
                _directiveType = directiveType;
                _directive = directive;
                _handler = handler;
            }
        }

        #endregion

        private List<DirectiveAssignment> _assignments = null;

        private List<DirectiveAssignment> Assignments
        {
            get
            {
                if (_assignments == null)
                    _assignments = new List<DirectiveAssignment>();

                return _assignments;
            }
        }

        public int CurrentLine { get { return CodeDocument == null ? -1 : CodeDocument.CurrentLine; } }

        internal MySqlCodeDoc CodeDocument { get; set; }

        public void RegisterDirective(string directive, Type directiveType, EventHandler handler)
        {
            Assignments.Add(new DirectiveAssignment(directive, directiveType, handler));
        }

        public PreprocessorDirective GetDirective(string ln)
        {
            PreprocessorDirective directive = new PreprocessorDirective();
            directive.Initialize(ln);

            foreach (DirectiveAssignment assignment in Assignments)
                if (assignment.Directive == directive.Directive)
                {
                    PreprocessorDirective result = assignment.DirectiveType.CreateInstance() as PreprocessorDirective;
                    if (result != null)
                    {
                        result.Initialize(ln);
                        result.ProcessDirective += assignment.Handler;
                        return result;
                    }
                }

            return null;
        }

        public void Process(string ln)
        {
            PreprocessorDirective directive = GetDirective(ln);
            if (directive != null)
                directive.InvokeProcess();
            else
                throw new InvalidDirectiveException(String.Format("Unknown directive in line {0}", CurrentLine));

        }
    }
}
