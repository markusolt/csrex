using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Alternate : Node {
    private Node _child;
    private Node _alternative;

    internal Alternate (Node child, Node alternative) {
      if (child == null) {
        throw new ArgumentNullException("Child may not be null.", nameof(child));
      }
      if (alternative == null) {
        throw new ArgumentNullException("Alternative may not be null.", nameof(alternative));
      }

      _child = child;
      _alternative = alternative;
    }

    internal override int CompiledLength {
      get {
        return 2 + _child.CompiledLength + _alternative.CompiledLength;
      }
    }

    internal override Span<Instruction> Compile (Span<Instruction> buffer) {
      if (buffer.Length < CompiledLength) {
        throw new ArgumentException("Insufficient space in buffer.", nameof(buffer));
      }

      buffer[0] = new Instruction(Regex.instr_branch, parameter: (ushort) (_child.CompiledLength + 1));
      buffer = _child.Compile(buffer);
      buffer[0] = new Instruction(Regex.instr_jump, parameter: (ushort) _alternative.CompiledLength);
      return _alternative.Compile(buffer);
    }
  }
}
