#region Source information

//*****************************************************************************
//
//   MySqlDebugCommand.cs
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
using System.IO;
using System.Linq;
using System.Text;
using System.Config;

using MySql.Data.MySqlClient;

using MySqlDevTools.Documents;
using MySqlDevTools.Config;


namespace MySqlDevTools.Services
{
    public class MySqlDebugCommand : CommandClass
    {
        private string GetDebugParameters()
        {
            throw new NotImplementedException();
        }

        protected override bool CoreMethod()
        {
            CommandClass.ExecuteCommand(typeof(MySqlPushCommand));

            CommandLineArg parmArgs = CommandLineArguments.Arguments["--debug-parms"];



            return true;
        }

        protected override void Release()
        {
            // throw new NotImplementedException();
        }

        public override string CommandName
        {
            get { return "Debug"; }
        }
    }
}
