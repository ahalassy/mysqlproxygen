#region Source information

//*****************************************************************************
//
//   Program.cs
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

using MySqlDevTools.Config;
using MySqlDevTools.Services;

namespace MySqlDevTools
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (RuntimeConfig.Command)
            {
                case ToolCommand.Push:
                    CommandClass.ExecuteCommand(typeof(MySqlPushCommand));
                    break;

                case ToolCommand.Debug:
                    CommandClass.ExecuteCommand(typeof(MySqlDebugCommand));
                    break;

                case ToolCommand.ProxyGen:
                    CommandClass.ExecuteCommand(typeof(MySqlProxyGenCommand));
                    break;

                default:
                    throw new CommandLineArgException(String.Format("Invalid command: \"{0}\"", CommandLineArguments.Command));
                    break;
            }

            Console.Write("Press ENTER to finish ...");
            Console.ReadLine();
        }
    }
}
