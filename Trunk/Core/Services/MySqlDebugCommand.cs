#region Source information

//*****************************************************************************
//
//   MySqlDebugCommand.cs
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

namespace MySqlDevTools.Services
{
    public class MySqlDebugCommand : CommandClass
    {
        protected override bool CoreMethod()
        {
            return true;
        }

        protected override void Release()
        {
            // throw new NotImplementedException();
        }

        public override string CommandName
        {
            get { return "Debug"; }
        }
    }
}
