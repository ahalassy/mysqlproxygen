using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Documents.Directives
{
    public class MessageDirective : PreprocessorDirective
    { }

    public class WarningDirective : MessageDirective
    { }

    public class ErrorDirective : WarningDirective
    { }
}
