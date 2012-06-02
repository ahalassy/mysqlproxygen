#region Source information
 
//*****************************************************************************
//
//   Main.cs
//   Created by Adam Halassy <adam.halassy@gmail.com> (2012.05.01. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//    Copyright (c) 2012 Adam Halassy
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************

#endregion
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;

using MySqlDevTools.Documents;
using MySqlDevTools;
using MySqlDevTools.Reflection;


namespace SchemaSandbox
{
	static class MainClass
	{
		private static MySqlConnection _connection = new MySqlConnection ("Host=vr-leviathan;uid=proxygen;pwd=proxygen;database=cs_schema");
		private static DataTable
			_tables = new DataTable (),
			_fields = null;
		
		public static MySqlConnection Connection { get { return _connection; } }
		
		public static DataTable Tables { get { return _tables; } }
		
		public static DataTable Fields { get { return _fields; } }
		
		#region Output control:
		private static void WriteStatus(string status)
		{
			Console.Write("{0} ... ", status);
		}
		
		private static void WriteOk()
		{
			Console.WriteLine("ok.");
		}
		
		private static void WriteFail()
		{
			Console.WriteLine("failed.");
		}
		
		#endregion
		
		public static void QueryDataTables()
		{
			MySqlCommand command = new MySqlCommand ("show tables;", Connection);
			using (MySqlDataReader reader = command.ExecuteReader())
			{
				_tables.Load(reader);
				reader.Close();
			}
		}
		
		public static void QueryFieldInformation(string tableName)
		{
			_fields = null;
			MySqlCommand command = new MySqlCommand (
					String.Format("show fields from {0};", tableName),
					Connection
					);
				
			using (MySqlDataReader reader = command.ExecuteReader())
			{
				_fields = new DataTable ();
				_fields.Load(reader);
			}
							
		}
		
		public static void Main(string[] args)
		{
			try
			{
				WriteStatus("Connecting");
				Connection.Open();
				WriteOk();
				
				WriteStatus("Query schema");
				QueryDataTables();
				WriteOk();
				
				if (!Directory.Exists("SchemaCode"))
					Directory.CreateDirectory("SchemaCode");
				
				List<TableCodeBuilder> tableCodeBuilders = new List<TableCodeBuilder> ();
				foreach (DataRow tableRow in Tables.Rows)
				{
					string tableName = tableRow [0] as string;
					if (String.IsNullOrEmpty(tableName))
						continue;
					WriteStatus(String.Format("Write code for table {0}", tableName));
					QueryFieldInformation(tableName);
					DataTableCodeDoc codeDoc = new DataTableCodeDoc (tableName, Fields);
					TableCodeBuilder codeBuilder = new TableCodeBuilder ("CloudStock.Data", codeDoc);
					tableCodeBuilders.Add(codeBuilder);
					codeBuilder.SaveSource("SchemaCode" + Path.DirectorySeparatorChar.ToString() + codeBuilder.ClassName + ".cs");
					WriteOk();
				}
				
				DataBaseCodeBuilder dbCodeBuilder = new DataBaseCodeBuilder ("CloudStock.Data", "cs_schema", "CloudStockSchemaProxy", tableCodeBuilders.ToArray());
				dbCodeBuilder.SaveSource("SchemaCode" + Path.DirectorySeparatorChar.ToString() + dbCodeBuilder.ClassName + ".cs");
				
			}
			catch (Exception ex)
			{
				WriteFail();
				Console.WriteLine(
					"{0} thrown: \"{1}\"\n{2}", 
					ex.GetType().FullName, 
					ex.Message,
					ex.StackTrace
					);
			}
			finally
			{
				
				Connection.Close();
			}
			Console.WriteLine("Done!");
		}
	}
}
