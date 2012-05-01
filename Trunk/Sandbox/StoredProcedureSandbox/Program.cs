#region Source information

//*****************************************************************************
//
//   Program.cs
//   Created by ahalassy (2012.05.01. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Sandbox\StoredProcedureSandbox\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using MySql.Data.MySqlClient;

namespace StoredProcedureSandbox
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            MySqlConnection connection = new MySqlConnection("server=192.168.56.254;user=proxygen;password=proxygen;database=sandbox");
            try
            {
                connection.Open();

                MySqlCommand command = new MySqlCommand("GetAcMstr", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@domain", "dom");
                command.Parameters.AddWithValue("@code", "10");
                command.Parameters.Add("@dsc", MySqlDbType.VarChar);

                command.Parameters["@domain"].Direction = ParameterDirection.Input;
                command.Parameters["@code"].Direction = ParameterDirection.Input;
                command.Parameters["@dsc"].Direction = ParameterDirection.Output;

                command.ExecuteNonQuery();

                foreach (MySqlParameter parm in command.Parameters)
                {
                    if (parm.Direction == ParameterDirection.InputOutput || parm.Direction == ParameterDirection.Output)
                        System.Console.WriteLine("{0} = {1};", parm.ParameterName, parm.Value);
                }

            }
            finally
            {
                connection.Close();
            }

            Console.ReadLine();
        }
    }
}
