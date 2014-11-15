using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLink.SymbolStore
{
    public class SequencePoint
    {
        public int Offset { get; }
        public SymDocument Document { get; }
        public int Line { get; }
        public int Column { get; }
        public int EndLine { get; }
        public int EndColumn { get; }

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
