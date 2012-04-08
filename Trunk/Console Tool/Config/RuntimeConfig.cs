#region Source information

//*****************************************************************************
//
//   RuntimeConfig.cs
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
using System.Config;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Config
{
    public static class RuntimeConfig
    {
        public const string ConnectionStringShortParm = "-c";

        public const string ConnectionStringLongParm = "--connection-string";

        public const string DebugCommand = "debug";

        public const string PushCommand = "push";

        public const string ProxyGenCommand = "proxygen";

        public static ToolCommand Command
        {
            get
            {
                switch (CommandLineArguments.Command)
                {
                    case DebugCommand: return ToolCommand.Debug;
                    case PushCommand: return ToolCommand.Push;
                    case ProxyGenCommand: return ToolCommand.ProxyGen;
                    default: return ToolCommand.Invalid;
                }
            }
        }

        public static CommandLineArg ConnectionStringArg
        {
            get
            {
                return CommandLineArguments.Arguments[ConnectionStringShortParm].IsDefined ?
                    CommandLineArguments.Arguments[ConnectionStringShortParm] :
                    CommandLineArguments.Arguments[ConnectionStringLongParm];
            }
        }

    }
}
