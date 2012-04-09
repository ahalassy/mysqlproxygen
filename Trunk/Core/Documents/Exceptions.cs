using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Documents
{
    public class ReturnProcessCodeException : Exception
    {
        public ReturnProcessCodeException() : base("Return from code processing") { }
    }
}
