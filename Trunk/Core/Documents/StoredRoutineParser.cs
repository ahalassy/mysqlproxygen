#region Source information

//*****************************************************************************
//
//   StoredRoutineParser.cs
//   Created by ahalassy (2012.05.01. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Core\Documents\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MySqlDevTools.Documents
{
    public class StoredRoutineParser
    {
        private static string ExtractParameter(ref string parms)
        {
            string result = "";
            if (!parms.Contains(','))
            {
                result = parms;
                parms = "";
                return result;
            }

            /* Meg kell nézni, hogy van-e a vessző előtt nyitó zárójel. Ha van, akkor a
             *      paraméter vége a záró zárójelet követő első vessző, vagy ha ott már
             *      nincs vessző, akkor a sztring végééig
             */

            int
                commaPos = parms.IndexOf(','),
                bracketPos = parms.IndexOf('(');

            if (bracketPos < commaPos)
            {
                bracketPos = parms.IndexOf(')', bracketPos);
                commaPos = parms.IndexOf(',', bracketPos);

                if (commaPos < 0)
                {
                    result = parms.Substring(0, parms.IndexOf('('));
                    parms = "";
                    return result;
                }
                else
                {
                    result = parms.Substring(0, parms.IndexOf('('));
                    parms = parms.Substring(commaPos + 1);
                }
            }
            else
            {
                result = parms.Substring(0, commaPos);
                parms = parms.Substring(commaPos + 1);
            }

            return result;

        }

        private string
            _sqlCode,
            _name;

        private RoutineType
            _type = RoutineType.Unknown;

        private List<RoutineParameter> _parms = new List<RoutineParameter>();

        public string RoutineName { get { return _name; } }

        public string SqlCode { get { return _sqlCode; } }

        public RoutineType RoutineType { get { return _type; } }

        public List<RoutineParameter> Parameters { get { return _parms; } }

        private string GetParameterList()
        {
            if (!SqlCode.Contains('(') || !SqlCode.Contains(')'))
                throw new SqlRoutineSyntaxException(RoutineName, "Parameter list not found");

            int
                start = SqlCode.IndexOf('('),
                depth = 1,
                end;

            for (end = start + 1; depth > 0 && end < SqlCode.Length; end++)
            {
                switch (SqlCode[end])
                {
                    case '(': depth++; break;
                    case ')': depth--; break;
                }
            }

            start++;
            end--;

            return RemoveNewLines(SqlCode.Substring(start, end - start));
        }

        private string RemoveNewLines(string text)
        {
            text = text.Replace("\r\n", "");
            return text.Replace("\n", "");
        }

        private void Parse()
        {
            Parameters.Clear();
            string parms = GetParameterList();

            while (!String.IsNullOrEmpty(parms))
            {
                string parmCode = ExtractParameter(ref parms);
                RoutineParameter parm = new RoutineParameter(parmCode);
                Parameters.Add(parm);
            }

        }

        public string CreateRoutineWrappers()
        {
            StringWriter codeWriter = new StringWriter();

            codeWriter.Write("public void {0}(", RoutineName);
            for (int i = 0; i < Parameters.Count; i++)
                codeWriter.Write(
                    i < (Parameters.Count - 1) ? "{0}, " : "{0}",
                    Parameters[i].CSharpCode
                    );
            codeWriter.WriteLine(")\n{");

            // Beírjuk a paraméterek felépítését
            codeWriter.WriteLine("\tDbStoredRoutineParmCollection parms = new DbStoredRoutineParmCollection(this);");
            foreach (RoutineParameter parm in Parameters)
                codeWriter.WriteLine(
                    parm.ParameterDirection == global::System.Data.ParameterDirection.Output ?
                        "\tparms.Add(ParameterDirection.{0}, \"{1}\", typeof({2}));" :
                        "\tparms.Add(ParameterDirection.{0}, \"{1}\", {1});",
                    parm.ParameterDirection,
                    parm.Name,
                    parm.ClrTypeName
                    );

            // Meghívjuk az eljárást
            codeWriter.WriteLine("\n\tthis.RunProcedure(\"{0}\", parms);\n", RoutineName);

            // Visszafejtjük a kimenő paramétereket
            foreach (RoutineParameter parm in Parameters)
            {
                if (!parm.IsOutGoing) continue;

                DefaultTypeValueHolder defValHolder = parm.GetClrNullValue();
                codeWriter.WriteLine(
                    defValHolder.IsBlank ?
                        "\t{0} = ({1})parms[\"{0}\"].Value;" :
                        "\t{0} = ({1})((parms[\"{0}\"].Value as {1}?) ?? {2});",
                    parm.Name,
                    parm.ClrTypeName,
                    parm.GetClrNullValue().CSharpCode
                    );

            }

            codeWriter.WriteLine("}");
            return codeWriter.ToString();
        }

        public StoredRoutineParser(RoutineType type, string name, string sqlCode)
        {
            _type = type;
            _name = name;
            _sqlCode = sqlCode;

            Parse();

        }

    }
}
