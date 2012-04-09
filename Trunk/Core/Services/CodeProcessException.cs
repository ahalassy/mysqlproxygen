using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Services
{
    public class CodeProcessException : Exception
    {
        public string FileName { get; private set; }

        public int CurrentLine { get; private set; }

        public override string Message
        {
            get
            {
                return String.Format(
                    "{0} - in {1} at line {2}.",
                    base.Message,
                    FileName,
                    CurrentLine
                    );
            }
        }

        public override string StackTrace
        {
            get { return InnerException.StackTrace; }
        }

        public CodeProcessException(
            string fileName,
            int line,
            Exception innerException
            )
            : base(innerException.Message, innerException)
        {
            this.FileName = fileName;
            this.CurrentLine = line;
        }

    }
}
