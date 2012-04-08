#region Source information

//*****************************************************************************
//
//   CommandLineArguments.cs
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
    public class CommandLineArguments
    {
        #region Static members:

        private static bool
            _caseSensitive = true;

        private static CommandLineArguments
            _args = new CommandLineArguments();

        public static string Command
        {
            get
            {
                if (_args._parsedArgs.Count == 0 || !_args._parsedArgs[0].IsCommand)
                    return null;

                return _args._parsedArgs[0].Name;

            }
        }

        public static bool CaseSensitive
        {
            get { return _caseSensitive; }
            set { _caseSensitive = value; }
        }

        public static bool IsDefined(string name)
        {
            return Arguments[name].IsDefined;
        }

        public static CommandLineArguments Arguments { get { return _args; } }

        public static CommandLineArg GetArgument(string shortForm, string longForm)
        {
            CommandLineArg arg = Arguments[shortForm];
            if (arg.IsDefined) return arg;

            return Arguments[longForm];
        }

        public string[] GetEnvironmentArgs()
        {
            string[]
                envArgs = Environment.GetCommandLineArgs(),
                result = new string[envArgs.Length - 1];

            for (int i = 1; i < envArgs.Length; i++)
                result[i - 1] = envArgs[i];

            return result;
        }


        #endregion

        #region Instance members:

        private List<CommandLineArg> _parsedArgs;

        private CommandLineArg GetArg(string name)
        {
            foreach (CommandLineArg arg in _parsedArgs)
                if (arg.Equals(name)) return arg;

            return CommandLineArg.CreateNonDefined();
        }

        public CommandLineArg this[string name]
        {
            get { return GetArg(name); }
        }

        private void ParseArgs()
        {
            int i = 0;
            string[] args = GetEnvironmentArgs();
            _parsedArgs = new List<CommandLineArg>();

            while (i < args.Length)
            {
                CommandLineParmKind kind = CommandLineArg.DetermineParmKind(args[i]);
                if (kind == CommandLineParmKind.Unknown)
                {
                    if (i != 0)
                        throw new CommandLineArgException("Commands only allowed for the first item!");

                    _parsedArgs.Add(CommandLineArg.CreateCommand(args[i]));
                }
                else
                {
                    if ((i + 1) < args.Length && CommandLineArg.DetermineParmKind(args[i + 1]) == CommandLineParmKind.Unknown)
                    {
                        _parsedArgs.Add(CommandLineArg.CreateParm(args[i], args[i + 1]));
                        i++;
                    }
                    else
                        _parsedArgs.Add(CommandLineArg.CreateSwitch(args[i]));

                }

                i++;
            }

        }

        internal CommandLineArguments()
        {
            ParseArgs();
        }

        #endregion
    }
}
