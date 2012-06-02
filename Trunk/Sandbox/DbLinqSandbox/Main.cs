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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using MySql.Data.MySqlClient;

namespace DbLinqSandbox
{
    public static class MainClass
    {
        public const string ConnectionString = "Host=vr-leviathan;uid=proxygen;pwd=proxygen;database=cs_schema";

        public static void Main(string[] args)
        {
            MySqlConnection connection = null;

            try
            {
                Console.Write("Connecting ... ");
                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                Console.WriteLine("ok.");

                Console.WriteLine("Query data ... ");
                CsSchemaDatabase database = new CsSchemaDatabase(connection);
                IEnumerable<table_usr_mstr> users = from usr_mstr in database.usr_mstr
                                                    where usr_mstr.usr_mstr_recid != 0
                                                    select usr_mstr;
                Console.WriteLine("ok.");

                Console.WriteLine("Users:");
                foreach (table_usr_mstr usr in users)
                    Console.WriteLine("{0} {1} <{2}>", usr.usr_first_name, usr.usr_last_name, usr.usr_mail);
                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed.");
                Console.WriteLine(
                    "{0} thrown: \"{1}\"\n{2}",
                    ex.GetType().FullName,
                    ex.Message,
                    ex.StackTrace
                    );
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }

            Console.ReadLine();
        }
    }
}
