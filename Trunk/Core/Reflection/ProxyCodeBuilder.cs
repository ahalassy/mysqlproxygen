#region Source information

//*****************************************************************************
//
//   ProxyCodeBuilder.cs
//   Created by ahalassy (2012.05.01. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Core\Reflection\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MySqlDevTools.Documents;

namespace MySqlDevTools.Reflection
{
    public class ProxyCodeBuilder
    {
        private string[] UsedNamespaces = new string[] {
            "System",
            "System.Data",
            "Halassy.Data",
			"MySql.Data.MySqlClient"
            };

        private string HeaderText =
            "//*****************************************************************************\n" +
            "//" + "\n" +
            "//   {0}" + "\n" +
            "//   This is a generated code, do not edit!" + "\n" +
            "//" + "\n" +
            "//*****************************************************************************\n";

        private string ConstructorCodeA =
            "public {0}(string connectionString)" + "\n" +
            "   : base(connectionString, typeof(MySqlManagementObjectFactory))" + "\n" +
            "{{ }}";
		
		private string ConstructorCodeB =
            "public {0}(MySqlConnection connection)" + "\n" +
            "   : base(connection, typeof(MySqlManagementObjectFactory))" + "\n" +
            "{{ }}";

        private string
            _namespace = null,
            _proxyName = null;

        private StoredRoutineParser[] _parsers = null;

        public string ProxyName { get { return _proxyName; } }

        public string Namespace { get { return _namespace; } }

        public void WriteCodeSnippet(string indent, string code, StringWriter writer)
        {
            StringReader snippetReader = new StringReader(code);
            string line;
            while ((line = snippetReader.ReadLine()) != null)
                writer.WriteLine(indent + line);
        }

        public void SaveSource(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                writer.WriteLine(CreateCode());
            }

        }

        public StoredRoutineParser[] GetParsers()
        {
            List<StoredRoutineParser> result = new List<StoredRoutineParser>();
            foreach (StoredRoutineParser parser in _parsers)
                result.Add(parser);

            return result.ToArray();

        }

        public string CreateCode()
        {
            StringWriter codeWriter = new StringWriter();

            codeWriter.WriteLine("#region Source Information");
            WriteCodeSnippet("", String.Format(HeaderText, ProxyName), codeWriter);
            codeWriter.WriteLine("#endregion\n");

            foreach (string nsName in UsedNamespaces)
                codeWriter.WriteLine("using {0};", nsName);

            codeWriter.WriteLine("\nnamespace {0}\n{{\n", Namespace);
            codeWriter.WriteLine("\tpublic class {0} : DbProxyClass\n\t{{\n", ProxyName);

            StoredRoutineParser[] parsers = GetParsers();
            foreach (StoredRoutineParser parser in parsers)
            {
                WriteCodeSnippet("\t\t", parser.CreateRoutineWrappers(), codeWriter);
                codeWriter.WriteLine();
            }

            WriteCodeSnippet("\t\t", String.Format(ConstructorCodeA, ProxyName), codeWriter);
			codeWriter.WriteLine ();
			
			WriteCodeSnippet("\t\t", String.Format(ConstructorCodeB, ProxyName), codeWriter);
            codeWriter.WriteLine();
			
            codeWriter.WriteLine("\t}\n}");
            codeWriter.WriteLine();
            codeWriter.WriteLine("// EOF");

            return codeWriter.ToString();

        }

        public ProxyCodeBuilder(
            string proxyName,
            string nameSpace,
            StoredRoutineParser[] routineParsers
            )
        {
            _namespace = nameSpace;
            _proxyName = proxyName;
            _parsers = routineParsers;
        }
    }
}
