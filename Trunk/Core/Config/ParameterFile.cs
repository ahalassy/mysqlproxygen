using System;
using System.Collections.Generic;
using System.Config;
using System.Linq;
using System.Text;
using System.IO;

namespace MySqlDevTools.Config
{
    public static class ParameterFile
    {
        public static void ApplyParameters(string fileName)
        {
			fileName = PathExtensions.NormalizePath(fileName);
			
            StreamReader reader = new StreamReader(fileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Replace("\t", " ");
                line = line.TrimStart();

                if (String.IsNullOrEmpty(line))
                    continue;

                switch (line[0])
                {
                    case '#':
                        continue;

                    default:
                        CommandLineArguments.DefineArg(CommandLineArg.ParseString(line));
                        break;
                }
            }

        }
    }
}
