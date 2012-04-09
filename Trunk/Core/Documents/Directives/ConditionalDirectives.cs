using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlDevTools.Documents.Directives
{
    public class ConditionalDirective : PreprocessorDirective { }

    public class IfDefinedDirective : ConditionalDirective { }

    public class IfNotDefineDirective : ConditionalDirective { }

    public class ElseDirective : ConditionalDirective { }

    public class ElseIfDefinedDirective : ConditionalDirective { }

    public class ElseIfNotDefinedDirective : ConditionalDirective { }

    public class EndIfDirective : ConditionalDirective { }
}
