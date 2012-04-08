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
using System.IO;
using System.Linq;
using System.Text;

using MySqlDevTools.Config;
using MySqlDevTools.Services;
using MySqlDevTools.Documents;

namespace MySqlDevTools
{
    class Program
    {

        private static void Initialize()
        {
            CommandClass.Verbose =
                PreprocessorDirective.Verbose = RuntimeConfig.IsVerbose;

        }

        static void Main(string[] args)
        {
            if (RuntimeConfig.IsVerbose)
                System.Console.WriteLine(Directory.GetCurrentDirectory());
            Initialize();
            try
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    RuntimeConfig.IsVerbose ?
                        "** {1}\nException: {0}\n{2}\n" :
                        "** {1}",
                    ex.GetType().FullName,
                    ex.Message,
                    ex.StackTrace
                    );
            }

            Console.Write("Press ENTER to finish ...");
            Console.ReadLine();
        }
    }
}
