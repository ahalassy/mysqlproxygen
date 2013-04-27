#region Source information

//*****************************************************************************
//
//   BackupDirective.cs
//   Created by ahalassy (2013.04.27. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide.  This is an unpublished work.
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Core\Documents\Directives\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Documents.Directives
{
    public class BackupDirective : PreprocessorDirective
    {
        public string TableName { get; private set; }

        protected override void Parse()
        {
            base.Parse();

            if (String.IsNullOrEmpty((Arguments ?? "").Trim()))
                throw new Exception("Backup table name not defined!");

            this.TableName = Arguments.Trim();
        }
    }
}
