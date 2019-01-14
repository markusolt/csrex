using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Repeat : Node {
    private Node _child;
    private bool _greedy;

    internal Repeat (Node child, bool greedy) {
      if (child == null) {
        throw new ArgumentNullException("Child may not be null.", nameof(child));
      }

      _child = child;
      _greedy = greedy;
      _compiledLength = 1 + _child.CompiledLength;
      _minLength = _child.MinLength;
    }

    internal Node Child {
      get {
        return _child;
      }
    }

    internal bool Greedy {
      get {
        return _greedy;
      }
    }

    internal override void CompileNode (Span<Instruction> buffer) {
      buffer = _child.Compile(buffer);

      if (_greedy) {
        buffer[0] = new Instruction(Opcode.BranchbackFast, parameter: (ushort) _child.CompiledLength);
      } else {
        buffer[0] = new Instruction(Opcode.Branchback, parameter: (ushort) _child.CompiledLength);
      }
    }
  }
}
