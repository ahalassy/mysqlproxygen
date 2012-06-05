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
using System.IO;
using System.Linq;
using System.Text;
using System.Config;

using MySql.Data.MySqlClient;

using MySqlDevTools.Documents;
using MySqlDevTools.Config;


namespace MySqlDevTools.Services
{
    public class MySqlPushCommand : CommandClass
    {
        protected override bool CoreMethod()
        {
            MySqlConnection connection = null;

            try
            {
                CommandLineArg
                    cStringArg = CommandLineArguments.Arguments["--connection-string"],
                    fileNameArg = CommandLineArguments.GetArgument("-s", "--source");


                string pattern = fileNameArg.IsDefined ? fileNameArg.Value : null;

                foreach (string fileName in Directory.GetFiles(Path.GetDirectoryName(pattern), Path.GetFileName(pattern)))
                {
                    Console.Write("Pushing {0} ...   ", fileName);

                    MySqlCodeDoc codeDoc = new MySqlCodeDoc(fileName);
                    string code = codeDoc.Process();

                    // Pushing code:
                    connection = new MySqlConnection(cStringArg.Value);
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(code, connection);
                    command.ExecuteNonQuery();

                    Console.WriteLine("ok.");

                }

                return true;
            }
            catch
            {
                Console.WriteLine("failed.");
                throw;
            }
            finally
            {
                if (connection != null
                    && (connection.State == global::System.Data.ConnectionState.Closed
                        || connection.State == global::System.Data.ConnectionState.Broken
                    )
                    )
                    connection.Close();

            }
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
