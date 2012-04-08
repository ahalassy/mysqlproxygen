#region Source information

//*****************************************************************************
//
//   CommandClass.cs
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
using System.Reflection;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Services
{
    public abstract class CommandClass
    {
        private static bool _verbose = false;

        public static bool Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }

        public static void ExecuteCommand(Type commandType)
        {
            ExecuteCommand(commandType, null, null);
        }

        public static void ExecuteCommand(Type commandType, Type[] constructorParmTypes, object[] constructorParms)
        {
            if (!commandType.IsSubclassOf(typeof(CommandClass)))
                throw new InvalidCastException(
                    String.Format("{0} is not a subclass of {1}!", commandType.FullName, typeof(CommandClass).FullName)
                    );


            ConstructorInfo constructor = constructorParmTypes == null ?
                commandType.GetConstructor(new Type[] { }) :
                commandType.GetConstructor(constructorParmTypes);

            CommandClass command = (CommandClass)(constructor.Invoke(constructorParms == null ? new object[] { } : constructorParms));
            command.Execute();

        }

        public bool Success { get; protected set; }

        public abstract string CommandName { get; }

        public void Execute()
        {
            try
            {
                Success = false;
                if (Verbose)
                    Console.WriteLine("Command \"{0}\" started ...", CommandName);
                Success = CoreMethod();
            }
            catch
            {
                Success = false;
                throw;
            }
            finally
            {
                if (Verbose)
                    Console.WriteLine("Command \"{0}\" {1}.", CommandName, Success ? "succeeded" : "failed");
                Release();
            }
        }

        protected abstract bool CoreMethod();

        protected abstract void Release();
    }
}
