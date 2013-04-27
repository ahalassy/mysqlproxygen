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
using MySqlDevTools.Services;
using System.Text.RegularExpressions;

namespace MySqlDevTools.Documents
{
    public class MySqlCodeDoc
    {
        #region public class ProcessStackFrame
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

        #endregion

        public static bool IsPreprocessorDirective(string ln)
        {
            if (String.IsNullOrEmpty(ln) || String.IsNullOrEmpty(ln.TrimStart()))
                return false;

            return ln.TrimStart()[0] == '#';
        }

        private bool
            _wasDirective = false;

        private int
            _currentLine;

        private string
            _routineName = null;

        private readonly DatabaseProvider
            _dbProvider;

        private List<MySqlMacro> _macros = new List<MySqlMacro>();

        private MySqlMacroModel _macroModel = new MySqlMacroModel();

        private List<ProcessStackFrame> _stack = new List<ProcessStackFrame>();

        private StreamReader CurrentReader { get; set; }

        public DatabaseProvider DbProvider { get { return _dbProvider; } }

        public bool SchemaDefinition { get; private set; }

        public string FileName { get; private set; }

        public RoutineType RoutineType { get; private set; }

        public MySqlMacroModel MacroModel { get { return _macroModel; } }

        public ProcessStackFrame TopFrame { get { return _stack[_stack.Count - 1]; } }

        public MySqlCodeDoc ParentDoc { get; private set; }

        public string RoutineName { get { return _routineName; } }

        internal int CurrentLine { get { return _currentLine; } }

        internal string WorkingDir { get { return Path.GetDirectoryName(FileName); } }

        private readonly List<string> TablesToBackup = new List<string>();

        private StringWriter CodeWriter { get; set; }

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
            if (ParentDoc != null)
                return ParentDoc.IsDefined(macroName);

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

            _macroModel.RegisterDirective("schema", typeof(PreprocessorDirective), new EventHandler(ehSchemaMarker));

            _macroModel.RegisterDirective("backup", typeof(BackupDirective), new EventHandler(ehBackupTable));

        }

        private void DefineMacro(string name, string content)
        {
            if (ParentDoc != null)
                ParentDoc.DefineMacro(name, content);
            else
                _macros.Add(new MySqlMacro(name, content));
        }

        private void DefineMacro(MacroDirective directive)
        {
            DefineMacro(directive.MacroName, directive.MacroContent);
        }

        private string ApplyMacros(string ln)
        {
            if (ParentDoc != null)
                return ParentDoc.ApplyMacros(ln);

            foreach (MySqlMacro macro in _macros)
                if (!String.IsNullOrEmpty(macro.Content))
                    ln = ln.WordReplace(macro.Name, macro.Content);

            return ln.Contains("--") ? ln.Substring(0, ln.IndexOf("--")) : ln;
        }

        private void InternalProcessCode()
        {
            try
            {
                OpenStackFrame();

                string ln = null;
                while ((ln = CurrentReader.ReadLine()) != null)
                {
                    if (IsPreprocessorDirective(ln.TrimStart()))
                    {
                        MacroModel.Process(ln.TrimStart());
                        _wasDirective = true;
                    }
                    else
                        if (!TopFrame.Ignore)
                        {
                            ln = ApplyMacros(ln);
                            if (!String.IsNullOrEmpty(ln))
                                CodeWriter.WriteLine(ln);
                        }

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

        private void ValidateCode()
        {
            if (ParentDoc != null || SchemaDefinition)
                return;

            RoutineType = RoutineType.Unknown;

            string
                procNameChars = "_abcdefghijklmnopqrstuxyvwz0123456789",
                whiteSpaces = " \t\n\r",
                code = CodeWriter.ToString();

            int defPos = code.ToUpper().IndexOf("CREATE");

            if (defPos < 0)
                throw new Exception("Source must contain \"CREATE PROCEDURE <proc_name>\" or \"CREATE FUNCTION <func_name>\" phrase!");

            int routinePos = code.ToUpper().IndexOf("FUNCTION", defPos);
            if (routinePos < 0)
                routinePos = code.ToUpper().IndexOf("PROCEDURE", defPos);
            else
                RoutineType = RoutineType.Function;

            if (routinePos < 0)
                throw new Exception("Source must contain \"CREATE PROCEDURE <proc_name>\" or \"CREATE FUNCTION <func_name>\" phrase!");
            else
                if (RoutineType == RoutineType.Unknown)
                    RoutineType = RoutineType.Procedure;

            int pos;
            for (pos = defPos + "CREATE".Length; pos < routinePos; pos++)
                if (!whiteSpaces.Contains(code[pos]))
                    throw new Exception("Source must contain \"CREATE PROCEDURE <proc_name>\" or \"CREATE FUNCTION <func_name>\" phrase!");

            _routineName = "";
            pos += RoutineType == RoutineType.Procedure ? "PROCEDURE".Length : "FUNCTION".Length;
            while (code.Length > pos && whiteSpaces.Contains(code[pos]))
                pos++;

            while (code.Length > pos && (procNameChars.Contains(code[pos]) || procNameChars.ToUpper().Contains(code[pos])))
            {
                _routineName += code[pos].ToString();
                pos++;
            }

            if (_routineName.ToUpper() != Path.GetFileNameWithoutExtension(FileName).ToUpper())
                throw new Exception(
                    String.Format(
                        "Stored routine name \"{0}\" does not match file name {1}!",
                        _routineName,
                        Path.GetFileNameWithoutExtension(FileName)
                        )
                    );

        }

        private void ResetMacros()
        {
            _macros.Clear();
            if (ParentDoc == null)
            {
                string[] tables = DbProvider.QueryTables();
                foreach (string table in tables)
                    DefineMacro(String.Format("schema.{0}", table), table);
            }
        }

        public string[] GetBackupTables()
        {
            return TablesToBackup.ToArray();
        }

        public string Process()
        {
            string result = "";

            try
            {
                MacroModel.CodeDocument = this;
                SchemaDefinition = false;
                _currentLine = 1;
                ResetMacros();
                CurrentReader = new StreamReader(FileName);
                CodeWriter = new StringWriter();

                InternalProcessCode();

            }
            catch (CodeProcessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CodeProcessException(
                    Path.GetFileName(FileName),
                    CurrentLine,
                    ex
                    );
            }

            ValidateCode();

            if (ParentDoc == null && !SchemaDefinition)
                result = String.Format(
                    RoutineType == RoutineType.Function ?
                        "DROP FUNCTION IF EXISTS {0};\n\n{1}" : "DROP PROCEDURE IF EXISTS {0};\n\n{1}",
                        RoutineName,
                        CodeWriter.ToString()
                        );
            else
                result = CodeWriter.ToString();

            // return ParentDoc == null ? String.Format("START TRANSACTION;\n{0}\nCOMMIT;", result) : result;
            return result;

        }

        public MySqlCodeDoc(string fileName, DatabaseProvider dbProvider)
        {
            _dbProvider = dbProvider;

            InitializeDom();

            this.FileName = PathExtensions.NormalizePath(fileName);
        }

        private void ehProcessMacro(object sender, EventArgs args)
        {
            MacroDirective directive = sender as MacroDirective;
            if (directive != null)
                DefineMacro(directive);
        }

        private void ehProcessMessage(object sender, EventArgs args)
        {


            if (TopFrame.Ignore)
                return;

            MessageDirective directive = sender as MessageDirective;
            if (directive is ErrorDirective)
                throw new Exception(String.Format("Explicit error: \"{0}\"", directive.Arguments));

            if (directive is WarningDirective)
            {
                if (Console.CursorLeft > 0)
                    Console.WriteLine();
                Console.WriteLine("## Warning: \"{0}\"", directive.Arguments);
                return;
            }

            if (Console.CursorLeft > 0)
                Console.WriteLine();
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
            // ConditionalDirective directive = sender as ConditionalDirective;

            CloseStackFrame();
        }

        private void ehProcessElseDefined(object sender, EventArgs args)
        {
            // ConditionalDirective directive = sender as ConditionalDirective;

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
            if (TopFrame.Ignore)
                return;

            PreprocessorDirective directive = sender as PreprocessorDirective;
            if (directive == null)
                return;

            string targetPath = PathExtensions.NormalizePath(directive.Arguments);

            MySqlCodeDoc codeDoc = new MySqlCodeDoc(Path.Combine(WorkingDir, targetPath), DbProvider);
            codeDoc.ParentDoc = this;
            CodeWriter.WriteLine(codeDoc.Process());
        }

        private void ehSchemaMarker(object sender, EventArgs args)
        {
            if (_wasDirective)
                throw new Exception("Schema defintion marker must be the first preprocessor directive!");

            SchemaDefinition = true;
        }

        private void ehBackupTable(object sender, EventArgs args)
        {
            BackupDirective directive = sender as BackupDirective;
            if (directive == null) return;

            if (!TablesToBackup.Contains(directive.TableName))
                TablesToBackup.Add(directive.TableName);
        }

    }
}
