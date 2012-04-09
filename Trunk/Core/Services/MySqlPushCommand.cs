#region Source information

//*****************************************************************************
//
//   MySqlPushCommand.cs
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
using System.Config;

using MySqlDevTools.Documents;

namespace MySqlDevTools.Services
{
    public class MySqlPushCommand : CommandClass
    {
        protected override bool CoreMethod()
        {
            CommandLineArg fileNameArg = CommandLineArguments.GetArgument("-s", "--source");
            string fileName = fileNameArg.IsDefined ? fileNameArg.Value : null;

            if (Verbose)
                Console.WriteLine("Parse file {0}", fileName);

            MySqlCodeDoc codeDoc = new MySqlCodeDoc(fileName);
            Console.WriteLine(
                "\nKód:\n\n{0}",
                codeDoc.Process()
                );

            return true;
        }

        protected override void Release()
        {
            // throw new NotImplementedException();
        }

        public override string CommandName
        {
            get { return "Push"; }
        }
    }
}
