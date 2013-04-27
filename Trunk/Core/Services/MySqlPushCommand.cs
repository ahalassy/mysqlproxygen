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
using System.Data;


namespace MySqlDevTools.Services
{
    public class MySqlPushCommand : CommandClass
    {
        private readonly List<TableBackupData> BackupData = new List<TableBackupData>();

        protected override bool CoreMethod()
        {
            DatabaseProvider dbProvider = null;

            try
            {
                CommandLineArg
                    cStringArg = CommandLineArguments.Arguments["--connection-string"],
                    fileNameArg = CommandLineArguments.GetArgument("-s", "--source");


                string
                    pattern = fileNameArg.IsDefined ? fileNameArg.Value : null,
                    path = Path.GetDirectoryName(pattern);

                if (String.IsNullOrEmpty(path))
                    path = ".";

                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException(String.Format("Path \"{0}\" not found", path));


                foreach (string fileName in Directory.GetFiles(path, Path.GetFileName(pattern)))
                {
                    Console.Write("Pushing {0} ...   ", fileName);
                    dbProvider = new DatabaseProvider(cStringArg.Value);

                    MySqlCodeDoc codeDoc = new MySqlCodeDoc(fileName, dbProvider);
                    string code = codeDoc.Process();

                    BackupTables(dbProvider.Connection, codeDoc.GetBackupTables());

                    // Pushing code:
                    MySqlCommand command = dbProvider.GetCommand(code);
                    command.ExecuteNonQuery();

                    RestoreTables(dbProvider.Connection);

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
                if (dbProvider != null)
                    dbProvider.Dispose();
            }
        }

        private void ClearBackup()
        {
            foreach (TableBackupData backup in BackupData)
                backup.Clear();

            BackupData.Clear();
        }

        private void RestoreTables(MySqlConnection connection)
        {
            try
            {
                foreach (TableBackupData backup in BackupData)
                    backup.RestoreContent(connection);

            }
            finally
            {
                ClearBackup();
            }
        }

        private void BackupTables(MySqlConnection connection, string[] tables)
        {
            ClearBackup();

            foreach (string table in tables)
            {
                TableBackupData backup = new TableBackupData(table);
                if (!backup.IsTableExists(connection)) continue;

                backup.BackupContent(connection);
                BackupData.Add(backup);
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
