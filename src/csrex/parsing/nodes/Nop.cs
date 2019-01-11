using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Nop : Node {
    internal Nop () {}

    internal override int CompiledLength {
      get {
        return 0;
      }
    }

    internal override Span<Instruction> Compile (Span<Instruction> buffer) {
      return buffer;
    }
  }
}
