using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Optional : Node {
    private Node _child;

    internal Optional (Node child) {
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
      buffer[0] = new Instruction(Opcode.Branch, parameter: (ushort) _child.CompiledLength);
      _child.Compile(buffer.Slice(1));
    }
  }
}
