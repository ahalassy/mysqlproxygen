using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Documents
{
    public class MacroDirective : PreprocessorDirective
    {
        public string MacroName { get; private set; }

        public string MacroContent { get; private set; }

        protected override void Parse()
        {
            base.Parse();

            int sepPos = Arguments.IndexOfAny(new char[] { ' ', '\t' });
            this.MacroName = sepPos < 0 ? Arguments.Trim() : Arguments.Substring(0, sepPos).Trim();
            this.MacroContent = sepPos < 0 ? "" : Arguments.Substring(sepPos).Trim();
        }
    }
}
