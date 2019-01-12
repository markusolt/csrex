using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Repeat : Node {
    private Node _child;

    internal Repeat (Node child) {
      if (child == null) {
        throw new ArgumentNullException("Child may not be null.", nameof(child));
      }

      _child = child;
      _compiledLength = 1 + _child.CompiledLength;
    }

    internal Node Child {
      get {
        return _child;
      }
    }

    internal override void CompileNode (Span<Instruction> buffer) {
      buffer = _child.Compile(buffer);
      buffer[0] = new Instruction(Opcode.Branchback, parameter: (ushort) _child.CompiledLength);
    }
  }
}
