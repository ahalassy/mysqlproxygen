using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        public static String Format(this string str, object arg0)
        {
            return String.Format(str, arg0);
        }

        public static String Format(this string str, object arg0, string arg1)
        {
            return String.Format(str, arg0, arg1);
        }

        public static String Format(this string str, object arg0, string arg1, string arg2)
        {
            return String.Format(str, arg0, arg1, arg2);
        }

        public static String Format(this string str, object[] args)
        {
            return String.Format(str, args);
        }

        public static string WordReplace(this string str, string oldstr, string newstr)
        {
            return Regex.Replace(str, String.Format("\\b{0}\\b", oldstr), newstr);
        }

    }
}
