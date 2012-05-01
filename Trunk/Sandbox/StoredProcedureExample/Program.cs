#region Source information

//*****************************************************************************
//
//   Program.cs
//   Created by ahalassy (2012.04.30. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Sandbox\StoredProcedureExample\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StoredProcedureExample.Services;

namespace StoredProcedureExample
{
    class Program
    {
        private static string ReadString(string prompt)
        {
            Console.Write(prompt + ": ");
            return Console.ReadLine();
        }

        static void Main(string[] args)
        {
            decimal
                stock = 0;

            string
                domain = ReadString("Domain"),
                accode = ReadString("Account"),
                part = ReadString("Part"),
                desc = ReadString("Account description"),
                oriDesc;

            try
            {
                using (SandboxProxy proxy = new SandboxProxy("Server=192.168.56.254;user=root;password=opel12;database=sandbox"))
                {
                    try
                    {
                        proxy.OpenConnection();
                        proxy.GetAcMstr(domain, accode, out oriDesc);
                        Console.WriteLine("Original description: {0}", oriDesc);
                        proxy.CreateAcMstr(domain, accode, desc);

                        proxy.GetPartStock(domain, part, out stock);
                        Console.WriteLine("Stock of {0}: {1}", part, stock);
                    }
                    finally
                    {
                        proxy.CloseConnection();
                    }
                }


                Console.WriteLine("Created ac_mstr!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(
                    "{0} thrown with message\n\"{1}\"\n{2}",
                    ex.GetType().FullName,
                    ex.Message,
                    ex.StackTrace
                    );
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
