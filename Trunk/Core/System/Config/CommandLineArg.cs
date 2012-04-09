#region Source information

//*****************************************************************************
//
//   CommandLineArg.cs
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
using System.Linq;
using System.Text;

namespace System.Config
{
    public class CommandLineArg
    {
        internal static CommandLineArg ParseString(string str)
        {
            int
                sepPos = str.IndexOf(' ');

            string
                name = sepPos < 0 ? str.Trim() : str.Substring(0, sepPos).Trim(),
                value = sepPos < 0 ? null : str.Substring(sepPos).Trim();

            return value == null ?
                CreateSwitch(name) : CreateParm(name, value);
        }

        internal static CommandLineArg CreateParm(string name, string value)
        {
            return new CommandLineArg(name, value);
        }

        internal static CommandLineArg CreateSwitch(string name)
        {
            return new CommandLineArg(name, null);
        }

        internal static CommandLineArg CreateCommand(string name)
        {
            CommandLineArg arg = new CommandLineArg(name, null);
            arg.IsCommand = true;
            return arg;
        }

        internal static CommandLineArg CreateNonDefined()
        {
            return new CommandLineArg(null, null);
        }

        private string
            _value = null;

        public static CommandLineParmKind DetermineParmKind(string name)
        {
            if (name.IndexOf("--") == 0) return CommandLineParmKind.LongForm;
            if (name.IndexOf('-') == 0) return CommandLineParmKind.ShortForm;

            return CommandLineParmKind.Unknown;
        }

        public string Value
        {
            get { return _value.Replace("\"", ""); }
            private set { _value = value; }
        }

        public string Name { get; private set; }

        public bool IsDefined { get { return !String.IsNullOrEmpty(Name); } }

        public bool IsSwitch { get { return !HasValue; } }

        public bool IsCommand { get; private set; }

        public bool HasValue { get { return !String.IsNullOrEmpty(Value); } }

        public CommandLineParmKind Kind
        {
            get
            {
                if (IsCommand || !IsDefined)
                    return CommandLineParmKind.Unknown;
                else
                    return DetermineParmKind(Name);

            }
        }

        public int GetAsInt() { return Int32.Parse(Value); }

        public bool GetAsBool() { return Boolean.Parse(Value); }

        public double GetAsFloat() { return Double.Parse(Value); }

        public DateTime GetAsDateTime() { return DateTime.Parse(Value); }

        public override bool Equals(object obj)
        {
            CommandLineArg otherArg = obj as CommandLineArg;
            if (otherArg == null)
                return false;

            if (this.IsDefined && otherArg.IsDefined)
                return CommandLineArguments.CaseSensitive ?
                    Name.Equals(otherArg.Name) :
                    Name.ToUpper().Equals(otherArg.Name.ToUpper());
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(string argName)
        {
            if (!IsDefined || String.IsNullOrEmpty(argName)) return false;

            return CommandLineArguments.CaseSensitive ?
                Name.Equals(argName) :
                Name.ToUpper().Equals(argName.ToUpper());
        }

        private CommandLineArg(string name, string value)
        {
            this.Name = name;
            this.Value = value;
            this.IsCommand = false;
        }
    }
}
