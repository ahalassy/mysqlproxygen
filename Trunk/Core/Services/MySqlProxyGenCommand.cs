#region Source information

//*****************************************************************************
//
//   MySqlProxyGenCommand.cs
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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using MySqlDevTools.Documents;
using MySqlDevTools.Reflection;
using System.CodeDom.Compiler;

namespace MySqlDevTools.Services
{
    public class MySqlProxyGenCommand : CommandClass
    {
        private static string GetSeparatorText(string sepstr, int len)
        {
            string result = "";
            for (int i = 0; i < len; i++) result += sepstr;
            return result;
        }

        private static string SeparatorText { get { return GetSeparatorText("-", 80); } }

        private string
            _dbName = null;

        private MySqlConnection
            _connection = null;

        public string Database { get { return _dbName; } }

        public MySqlConnection Connection { get { return _connection; } }

        private void TruncOutput()
        {
            TruncOutput(true);
        }

        private void TruncOutput(bool success)
        {
            Console.WriteLine(
                success ? "ok" : "failed"
                );
        }

        private void WriteMsg(string msg)
        {
            Console.Write("{0} ...   ", msg);
        }

        private void ExecuteInto(DataTable table, MySqlCommand command)
        {
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                table.Load(reader);
                reader.Close();
            }
        }

        private DataTable QueryStoredRoutines()
        {
            DataTable
                result = new DataTable();

            MySqlCommand
                queryProcs = new MySqlCommand("show procedure status where Db = DATABASE();", Connection),
                queryFuncs = new MySqlCommand("show function status where Db = DATABASE();", Connection);

            ExecuteInto(result, queryProcs);
            ExecuteInto(result, queryFuncs);

            return result;
        }

        private string GetDatabaseName()
        {
            using (MySqlCommand queryDbName = new MySqlCommand("SELECT DATABASE();", Connection))
            {
                return queryDbName.ExecuteScalar() as string;
            }

        }

        private RoutineType ParseRoutineType(string type)
        {
            if (type.ToUpper() == "PROCEDURE")
                return RoutineType.Procedure;

            if (type.ToUpper() == "FUNCTION")
                return RoutineType.Function;

            return RoutineType.Unknown;
        }

        private StreamWriter OpenLogWriter(string path, string asmName)
        {
            StreamWriter result = new StreamWriter(
                Path.Combine(path, asmName + ".Build.log"),
                true,
                Encoding.UTF8
                );

            result.WriteLine(SeparatorText + "\n");
            result.WriteLine("Build date: {0}, {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            result.Write("BUILD {0} ", asmName);

            return result;
        }

        public StoredRoutineParser QueryRoutineCode(string type, string name)
        {
            MySqlCommand
                queryCode = new MySqlCommand(
                    String.Format("SHOW CREATE {0} {1}.{2};", type, Database, name),
                    Connection
                    );

            using (DataTable createInfo = new DataTable())
            {
                ExecuteInto(createInfo, queryCode);
                if (createInfo.Rows.Count != 1)
                    throw new InvalidProgramException(String.Format("Cannot fetch code of {0} {1}.{2}!", type, name, Database));

                return new StoredRoutineParser(
                    ParseRoutineType(type),
                    name,
                    createInfo.Rows[0][2].ToString()
                    );

            }

        }

        public void BuildPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        protected override bool CoreMethod()
        {
            StreamWriter buildLogWriter = null;


            try
            {
                WriteMsg("Prepare");

                CommandLineArg
                    cStringArg = CommandLineArguments.Arguments["--connection-string"],
                    namespaceArg = CommandLineArguments.Arguments["--namespace"],
                    saveSourceArg = CommandLineArguments.Arguments["--save-source"],
                    assemblyNameArg = CommandLineArguments.Arguments["--assembly"],
                    pathArg = CommandLineArguments.Arguments["--output-path"];

                string
                    path = pathArg.IsDefined ? pathArg.Value : Directory.GetCurrentDirectory();

                if (!assemblyNameArg.IsDefined) throw new Exception("You must specify assembly name!");
                if (!cStringArg.IsDefined) throw new Exception("You must specify connection string!");
                if (!namespaceArg.IsDefined) throw new Exception("You must specify a namespace!");

                BuildPath(path);
                buildLogWriter = OpenLogWriter(path, assemblyNameArg.Value);

                WriteMsg("Connect to database");
                _connection = new MySqlConnection(cStringArg.Value);
                Connection.Open();
                TruncOutput();

                WriteMsg("Database name");
                _dbName = GetDatabaseName();
                Console.WriteLine(Database);

                WriteMsg("Query and parse stored routines");
                DataTable routines = QueryStoredRoutines();
                List<StoredRoutineParser> parsers = new List<StoredRoutineParser>();
                foreach (DataRow row in routines.Rows)
                    parsers.Add(QueryRoutineCode(row["Type"].ToString(), row["Name"].ToString()));
                TruncOutput();

                WriteMsg("Build assembly");
                ProxyCodeBuilder codeBuilder = new ProxyCodeBuilder(assemblyNameArg.Value, namespaceArg.Value, parsers.ToArray());
                if (saveSourceArg.IsDefined)
                    codeBuilder.SaveSource(Path.Combine(path, assemblyNameArg.Value + ".cs"));

                ProxyAssemblyBuilder assemblyBuilder = new ProxyAssemblyBuilder(path, assemblyNameArg.Value, codeBuilder.CreateCode());
                assemblyBuilder.BuildToFile();
                TruncOutput();

                buildLogWriter.WriteLine("SUCCESS");
                return true;
            }
            catch (AssemblyCompilerException ex)
            {
                buildLogWriter.WriteLine("FAILED.\n");
                foreach (CompilerError error in ex.CompilerResults.Errors)
                    buildLogWriter.WriteLine(error.ToString());
                Console.WriteLine("failed.");

                throw;
            }
            catch
            {
                if (buildLogWriter != null)
                    buildLogWriter.WriteLine("FAILED.");
                Console.WriteLine("failed.");
                throw;
            }
            finally
            {
                if (buildLogWriter != null)
                    buildLogWriter.Close();

                if (Connection != null
                    && (Connection.State == global::System.Data.ConnectionState.Closed
                        || Connection.State == global::System.Data.ConnectionState.Broken
                    )
                    )
                    Connection.Close();

            }
        }

        protected override void Release()
        {
            // throw new NotImplementedException();
        }

        public override string CommandName
        {
            get { return "Proxy Generation"; }
        }
    }
}
