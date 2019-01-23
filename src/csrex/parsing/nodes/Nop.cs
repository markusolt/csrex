using System;
using System.Text;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Nop : Node {
    internal Nop () {
      _compiledLength = 0;
      _minLength = 0;
    }

    internal override void CompileNode (Span<Instruction> buffer, StringBuilder words) {}
  }
}
