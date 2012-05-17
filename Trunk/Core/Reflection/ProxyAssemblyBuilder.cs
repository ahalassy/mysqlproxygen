#region Source information

//*****************************************************************************
//
//   ProxyAssemblyBuilder.cs
//   Created by ahalassy (2012.05.05. 0:00:00)
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Microsoft.CSharp;


namespace MySqlDevTools.Reflection
{
    public class ProxyAssemblyBuilder
    {
        public const int BufferSize = 4096;

        public static string ProgramPath
        {
            get
            {
                string asmPath = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().GetName().CodeBase
                    );
				
				switch (Environment.OSVersion.Platform) 
				{
					case PlatformID.MacOSX:
					case PlatformID.Unix:
						return String.IsNullOrEmpty(asmPath) ? "" : asmPath.Replace("file:", "");
				
					default:
						return String.IsNullOrEmpty(asmPath) ? "" : asmPath.Replace("file:\\", "");
				}
            }
        }

        public static string CustomAssemblyPath
        {
            get
            {
                return Path.Combine(ProgramPath, "Deploy");
            }
        }

        private string[] SystemReferences = new string[] {
            "System.dll",
            "System.Data.dll",
            "System.Xml.dll"
            };

        private string[] CustomReferences = new string[] {
            "Halassy.DbProxy.dll"
            };

        private string
            _name,
            _code,
            _path;

        public string AssemblyName { get { return _name; } }

        public string AssemblyCode { get { return _code; } }

        public string OutputPath { get { return _path; } }

        private Exception CreateCompilerException(CompilerResults results)
        {
            return new AssemblyCompilerException(
                "Failed to compile proxy assembly! (See build log for details)",
                results
                );
        }

        private void CopyFile(string src, string target)
        {
            FileStream
                srcFile = null,
                targetFile = null;

            try
            {
                byte[] buffer = new byte[BufferSize];
                int bytesRead = 0;

                srcFile = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.None);
                targetFile = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None);

                do
                {
                    bytesRead = srcFile.Read(buffer, 0, BufferSize);
                    targetFile.Write(buffer, 0, bytesRead);
                }
                while (bytesRead == BufferSize);

            }
            finally
            {
                try { if (srcFile != null) srcFile.Close(); }
                catch { }
                try { if (targetFile != null) targetFile.Close(); }
                catch { }
            }
        }

        private void CopyCustomAssemblies()
        {
            foreach (string assembly in CustomReferences)
                CopyFile(
                    Path.Combine(CustomAssemblyPath, assembly),
                    Path.Combine(OutputPath, assembly)
                    );
        }

        public void BuildToFile()
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compilerParms = new CompilerParameters();

            compilerParms.OutputAssembly = Path.Combine(OutputPath, AssemblyName + ".dll");
            compilerParms.GenerateExecutable = false;

            foreach (string refasm in SystemReferences)
                compilerParms.ReferencedAssemblies.Add(refasm);

            foreach (string refasm in CustomReferences)
                compilerParms.ReferencedAssemblies.Add(Path.Combine(CustomAssemblyPath, refasm));

            CompilerResults result = codeProvider.CompileAssemblyFromSource(compilerParms, AssemblyCode);
            if (result.Errors.Count > 0)
                throw CreateCompilerException(result);

            CopyCustomAssemblies();
        }

        public ProxyAssemblyBuilder(string path, string name, string code)
        {
            _path = path ?? "";
            _name = name;
            _code = code;
        }
    }
}
