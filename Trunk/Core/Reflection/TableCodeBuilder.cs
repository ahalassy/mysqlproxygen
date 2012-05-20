// #region Source information
// 
// //*****************************************************************************
// //
// //   TableCodeBuilder.cs
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

using MySqlDevTools.Documents;

namespace MySqlDevTools.Reflection
{
	public class TableCodeBuilder
	{
		#region Code snippets:
		private readonly string[] UsedNamespaces = new string[] {
			"System",
			"System.Data.Linq",
			"System.Data.Linq.Mapping",
			"DbLinq.Data.Linq",
			"DbLinq.Data.Linq.Mapping"
            };
		
		private readonly string HeaderText =
            "//*****************************************************************************\n" +
            "//" + "\n" +
            "//   {0}" + "\n" +
            "//   This is a generated code, do not edit!" + "\n" +
            "//" + "\n" +
            "//*****************************************************************************\n" +
            "//   Entity mapping class for {1}\n" +
            "//*****************************************************************************\n";
		private readonly string ClassHeader =
			"[Table(Name = \"{0}\")]" + "\n" +
			"public class {1}";
		
		#endregion
		
		private string _namespace = null;
		
		private DataTableCodeDoc _codeDoc = null;
		
		public DataTableCodeDoc CodeDoc { get { return _codeDoc; } }
		
		public string NameSpace {get {return _namespace;}}
		
		public string ClassName
		{
			get
			{
				/*
				 * Class name generation method is the following:
				 * - The first letter is upcase
				 * - Each letter following '_' is also upcase 
				 * - 'Entity' word concanetated after converted name
				 */
				
				bool upcase = true;
				string name = "";
				for (int i = 0; i < CodeDoc.TableName.Length; i++)
				{
					char chr = CodeDoc.TableName [i];
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
				
				return name + "Entity";
			}
		}
		
		private void WriteSnippet(StringWriter codeWriter, string indent, string snippet)
		{
			StringReader reader = new StringReader (snippet);
			string ln;
			while (!String.IsNullOrEmpty( (ln = reader.ReadLine())))
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
				String.Format(HeaderText, ClassName, CodeDoc.TableName)
				);
			codeWriter.WriteLine();
			
			// Using directives
			foreach (string nsName in UsedNamespaces)
                codeWriter.WriteLine("using {0};", nsName);
			codeWriter.WriteLine();
			
			// Namespace and class header:
			codeWriter.WriteLine("namespace {0}\n{{", NameSpace);
			WriteSnippet(
				codeWriter,
				"\t",
				String.Format(ClassHeader, CodeDoc.TableName, ClassName)
				);
			codeWriter.WriteLine("\t{\n");
			
			// Fields:
			foreach (FieldInformation field in CodeDoc.Fields)
				codeWriter.WriteLine(
					field.IsKey ?
						"\t\t[Column(IsDbGenerated = true, IsPrimaryKey = true)]\n\t\t{0}\n" :
						"\t\t[Column]\n\t\t{0}\n", 
					field.CSharpCode
					);
						
			// Finalize:
			codeWriter.WriteLine("\t}\n}");
			
			return codeWriter.ToString();
		}
		
		public TableCodeBuilder (string ns, DataTableCodeDoc codeDoc)
		{
			_namespace = ns;
			_codeDoc = codeDoc;
		}
		
	}
}

