using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLink.SymbolStore
{
    public class SequencePoint
    {
        public int Offset { get; private set; }
        public SymDocument Document { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }

        public SequencePoint(int offset, SymDocument document, int line, int column, int endLine, int endColumn)
        {
            Offset = offset;
            Document = document;
            Line = line;
            Column = column;
            EndLine = endLine;
            EndColumn = endColumn;
        }
    }
}
