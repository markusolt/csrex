using System;
using System.Text;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Optional : Node {
    private Node _child;
    private bool _greedy;

    internal Optional (Node child, bool greedy) {
      if (child == null) {
        throw new ArgumentNullException("Child may not be null.", nameof(child));
      }

      _child = child;
      _greedy = greedy;
      _compiledLength = 1 + _child.CompiledLength;
      _minLength = 0;
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

    internal override void CompileNode (Span<Instruction> buffer, StringBuilder words) {
      if (_greedy) {
        buffer[0] = new Instruction(Opcode.Branch, parameter: (ushort) _child.CompiledLength);
      } else {
        buffer[0] = new Instruction(Opcode.BranchFast, parameter: (ushort) _child.CompiledLength);
      }

      _child.Compile(buffer.Slice(1), words);
    }
  }
}
