#region  information

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
            for (int i = 0; i < len; i++)
                result += sepstr;
            return result;
        }

        private static string SeparatorText { get { return GetSeparatorText("-", 80); } }

        private string
            _dbName = null;

        private MySqlConnection
            _connection = null;
        
        private CommandLineArg
            _cStringArg,
            _namespaceArg,
            _saveArg,
            _assemblyNameArg,
            _pathArg;

        public CommandLineArg ConnectionStringArg { get { return _cStringArg; } }
        
        public CommandLineArg NamespaceArg { get { return _namespaceArg; } }
        
        public CommandLineArg SaveSourceArg { get { return _saveArg; } }
        
        public CommandLineArg AssemblyNameArg { get { return _assemblyNameArg; } }
        
        public CommandLineArg PathArg { get { return _pathArg; } }
        
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
                result = new DataTable ();

            MySqlCommand
                queryProcs = new MySqlCommand ("show procedure status where Db = DATABASE();", Connection),
                queryFuncs = new MySqlCommand ("show function status where Db = DATABASE();", Connection);

            ExecuteInto(result, queryProcs);
            ExecuteInto(result, queryFuncs);

            return result;
        }
     
        private DataTable QueryTables()
        {
            DataTable result = new DataTable ();
            MySqlCommand queryTables = new MySqlCommand ("show tables;", Connection);
         
            ExecuteInto(result, queryTables);
         
            return result;
        }
     
        private DataTable QueryFields(string tableName)
        {
            DataTable result = new DataTable ();
            MySqlCommand queryFields = new MySqlCommand (
             String.Format("show fields from {0};", tableName),
             Connection
             );
         
            ExecuteInto(result, queryFields);
         
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
            StreamWriter result = new StreamWriter (
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
                queryCode = new MySqlCommand (
                    String.Format("SHOW CREATE {0} {1}.{2};", type, Database, name),
                    Connection
                    );

            using (DataTable createInfo = new DataTable())
            {
                ExecuteInto(createInfo, queryCode);
                if (createInfo.Rows.Count != 1)
                    throw new InvalidProgramException (String.Format("Cannot fetch code of {0} {1}.{2}!", type, name, Database));

                return new StoredRoutineParser (
                    ParseRoutineType(type),
                    name,
                    createInfo.Rows [0] [2].ToString()
                    );

            }

        }

        public void BuildPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private void FetchArguments()
        {
            _cStringArg = CommandLineArguments.Arguments ["--connection-string"];
            _namespaceArg = CommandLineArguments.Arguments ["--namespace"];
            _saveArg = CommandLineArguments.Arguments ["--save-source"];
            _assemblyNameArg = CommandLineArguments.Arguments ["--assembly"];
            _pathArg = CommandLineArguments.Arguments ["--output-path"];
   
            if (!AssemblyNameArg.IsDefined)
                throw new Exception ("You must specify assembly name!");
            if (!ConnectionStringArg.IsDefined)
                throw new Exception ("You must specify connection string!");
            if (!NamespaceArg.IsDefined)
                throw new Exception ("You must specify a namespace!");

        }
        
        private DataBaseCodeBuilder FetchSchemaCodes(CommandLineArg namespaceArg, CommandLineArg assemblyNameArg)
        {
            DataTable tables = QueryTables();
            List<TableCodeBuilder> tableCodeBuilders = new List<TableCodeBuilder> ();
            foreach (DataRow row in tables.Rows)
            {
                string tableName = row [0] as string;
                DataTable fields = QueryFields(tableName);
                DataTableCodeDoc codeDoc = new DataTableCodeDoc (tableName, fields);
                tableCodeBuilders.Add(new TableCodeBuilder (namespaceArg.Value, codeDoc));
            }
            DataBaseCodeBuilder dbCodeBuilder = new DataBaseCodeBuilder (namespaceArg.Value, Database, assemblyNameArg.Value, tableCodeBuilders.ToArray());
            return dbCodeBuilder;
        }

        private List<StoredRoutineParser> FetchRoutineWrappers()
        {
            DataTable routines = QueryStoredRoutines();
            List<StoredRoutineParser> parsers = new List<StoredRoutineParser> ();
            foreach (DataRow row in routines.Rows)
                parsers.Add(QueryRoutineCode(row ["Type"].ToString(), row ["Name"].ToString()));
            return parsers;
        }

        private List<string> BuildCode(DataBaseCodeBuilder dbCodeBuilder, ProxyCodeBuilder proxyCodeBuilder)
        {
            List<string> Code = new List<string> ();
            Code.Add(proxyCodeBuilder.CreateCode());
            Code.Add(dbCodeBuilder.CreateCode());
            foreach (TableCodeBuilder codeBuilder in dbCodeBuilder.TableCodeBuilders)
                Code.Add(codeBuilder.CreateCode());
            return Code;
        }

        private void SaveSourceCodes(
            string path, 
            DataBaseCodeBuilder dbCodeBuilder, 
            ProxyCodeBuilder proxyCodeBuilder
            )
        {
            string sourcePath = Path.Combine(path, "source");
            BuildPath(sourcePath);
                         
            dbCodeBuilder.SaveSource(Path.Combine(sourcePath, dbCodeBuilder.ClassName + ".cs"));
            foreach (TableCodeBuilder codeBuilder in dbCodeBuilder.TableCodeBuilders)
                codeBuilder.SaveSource(Path.Combine(sourcePath, codeBuilder.ClassName + ".cs"));
            proxyCodeBuilder.SaveSource(Path.Combine(sourcePath, AssemblyNameArg.Value + ".cs"));
        }

        protected override bool CoreMethod()
        {
            StreamWriter buildLogWriter = null;


            try
            {
                WriteMsg("Prepare");
                FetchArguments();
                
                string
                    path = Path.Combine(PathArg.IsDefined ? PathArg.Value : Directory.GetCurrentDirectory(), "");

                BuildPath(path);
                buildLogWriter = OpenLogWriter(path, AssemblyNameArg.Value);

                WriteMsg("Connect to database");
                _connection = new MySqlConnection (ConnectionStringArg.Value);
                Connection.Open();
                TruncOutput();

                WriteMsg("Database name");
                _dbName = GetDatabaseName();
                Console.WriteLine(Database);
             
                WriteMsg("Query database schema");
                DataBaseCodeBuilder dbCodeBuilder = FetchSchemaCodes(NamespaceArg, AssemblyNameArg);
                TruncOutput();

                WriteMsg("Query and parse stored routines");
                List<StoredRoutineParser> parsers = FetchRoutineWrappers();
                TruncOutput();

                WriteMsg("Build assembly");
                ProxyCodeBuilder proxyCodeBuilder = new ProxyCodeBuilder (AssemblyNameArg.Value, NamespaceArg.Value, parsers.ToArray());
                List<string> Code = BuildCode(dbCodeBuilder, proxyCodeBuilder);

                if (SaveSourceArg.IsDefined)
                    SaveSourceCodes(path, dbCodeBuilder, proxyCodeBuilder);
         
                ProxyAssemblyBuilder assemblyBuilder = new ProxyAssemblyBuilder (
                    path, 
                    AssemblyNameArg.Value, 
                    Code.ToArray()
                    );
                
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
