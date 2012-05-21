// #region Source information
// 
// //*****************************************************************************
// //
// //   DataBaseCodeBuilder.cs
// //   Created by Adam Halassy <adam.halassy@gmail.com> (2012.05.01. 0:00:00)
// //
// // ---------------------------------------------------------------------------
// //
// //    Copyright (c) 2012 Adam Halassy
// //   All rights reserved worldwide. Document licensed by the terms of GPLv3
// //
// //*****************************************************************************
// 
// #endregion
// 
using System;
using System.IO;
using System.Text;

using MySqlDevTools.Reflection;

namespace MySqlDevTools
{
	public class DataBaseCodeBuilder
	{
		#region Code snippets:
		private readonly string[] UsedNamespaces = new string[] {
			"System",
			"System.Data.Linq",
			"System.Data.Linq.Mapping",
			"MySql.Data.MySqlClient",
			"DbLinq.Data.Linq",
			"DbLinq.Data.Linq.Mapping",
			"DataContext = DbLinq.MySql.MySqlDataContext"
            };
		private readonly string HeaderText =
            "//*****************************************************************************\n" +
            "//" + "\n" +
            "//   {0}" + "\n" +
            "//   This is a generated code, do not edit!" + "\n" +
            "//" + "\n" +
            "//*****************************************************************************\n" +
            "//   Database class for {1}\n" +
            "//*****************************************************************************\n";
		private readonly string ClassHeader =
			"[Database(Name = \"{0}\")]" + "\n" +
			"public class {1} : DataContext";
		private readonly string ConstructorCode =
            "private {0}(MySqlConnection connection)" + "\n" +
            "   : base(connection)" + "\n" +
            "{{" + "\n" +
			"\tthis.RoutineWrapper = new {1}(connection);" + "\n" +
			"}}";
		private readonly string DataTablePropertyCode = 
			"public DbLinq.Data.Linq.Table<{0}> {1} {{ get {{ return base.GetTable<{0}>(); }} }}";
		
		private readonly string SingletonCode =
			"#region Static members"+ "\n" +
			"private static {0} _database = null;" + "\n" +
			""+ "\n" +
			"public static {0} Database {{ get {{ return _database; }} }}" + "\n" +
			""+ "\n" +
			"public static void Open(MySqlConnection connection)" + "\n" +
			"{{" + "\n" +
			"\t_database = new {0}(connection);" + "\n" +
			"}}" + "\n" +
			"#endregion";
		
		#endregion
		
		private string
			_dbName = null,
			_proxyClassName = null,
			_namespace = null;
		
		private TableCodeBuilder[]
			_tableBuilders = null;
		
		public string DataBaseName { get { return _dbName; } }
		
		public string Namespace { get { return _namespace; } }
		
		public string ProxyClassName { get { return _proxyClassName; } }
		
		public string ClassName
		{
			get
			{
				/*
				 * Class name generation method is the following:
				 * - The first letter is upcase
				 * - Each letter following '_' is also upcase 
				 * - 'Database' word concanetated after converted name
				 */
				
				bool upcase = true;
				string name = "";
				for (int i = 0; i < DataBaseName.Length; i++)
				{
					char chr = DataBaseName [i];
					if (upcase)
					{
						chr = char.ToUpper(chr);
						upcase = false;
					}
					
					if (chr == '_')
						upcase = true;
					else
						name += chr.ToString();
				}
				
				return name + "Database";
			}
		}

		public TableCodeBuilder[] TableCodeBuilders { get { return _tableBuilders; } }
		
		private void WriteSnippet(StringWriter codeWriter, string indent, string snippet)
		{
			StringReader reader = new StringReader (snippet);
			string ln;
			while (( (ln = reader.ReadLine())) != null)
				codeWriter.WriteLine(indent + ln);
		}
		
		public void SaveSource(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                writer.WriteLine(CreateCode());
            }

        }
		
		public string CreateCode()
		{
			StringWriter codeWriter = new StringWriter ();
			
			// Code header:
			WriteSnippet(
				codeWriter,
				"",
				String.Format(HeaderText, ClassName, DataBaseName)
				);
			codeWriter.WriteLine();
			
			// Using directives
			foreach (string nsName in UsedNamespaces)
				codeWriter.WriteLine("using {0};", nsName);
			codeWriter.WriteLine();
			
			// Namespace and class header:
			codeWriter.WriteLine("namespace {0}\n{{", Namespace);
			WriteSnippet(
				codeWriter,
				"\t",
				String.Format(ClassHeader, DataBaseName, ClassName)
				);
			codeWriter.WriteLine("\t{");
			
			// Singleton code:
			WriteSnippet(
				codeWriter,
				"\t\t",
				String.Format(SingletonCode, ClassName)
				);
			codeWriter.WriteLine();
			
			// ProxyClass:
			codeWriter.WriteLine("\t\tpublic {0} RoutineWrapper {{ get; private set; }}\n", ProxyClassName);
			
			// Tables:
			codeWriter.WriteLine("\t\t#region Data tables");
			foreach (TableCodeBuilder tableCodeBuilder in TableCodeBuilders)
				WriteSnippet(
					codeWriter,
					"\t\t",
					String.Format(DataTablePropertyCode, tableCodeBuilder.ClassName, tableCodeBuilder.CodeDoc.TableName)
					);
			codeWriter.WriteLine("\t\t#endregion\n");
			
			// Constructor:
			WriteSnippet(
				codeWriter,
				"\t\t",
				String.Format(ConstructorCode, ClassName, ProxyClassName)
				);
			codeWriter.WriteLine();
			
			// Finalize:
			codeWriter.WriteLine("\t}\n}");
			
			return codeWriter.ToString();
		}
		
		public DataBaseCodeBuilder (
			string ns, 
			string databaseName, 
			string proxyClassName, 
			TableCodeBuilder[] builders
			)
		{
			_namespace = ns;
			_dbName = databaseName;
			_tableBuilders = builders;
			_proxyClassName = proxyClassName;
		}
	}
}

