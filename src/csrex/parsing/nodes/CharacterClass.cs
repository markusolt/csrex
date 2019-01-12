using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class CharacterClass : Node {
    private Node _child;

    internal CharacterClass (Node child) {
      if (child == null) {
        throw new ArgumentNullException("Child may not be null.", nameof(child));
      }

      _child = child;
    }

    internal override int CompiledLength {
      get {
        return 1 + _child.CompiledLength;
      }
    }

    internal override Span<Instruction> Compile (Span<Instruction> buffer) {
      if (buffer.Length < CompiledLength) {
        throw new ArgumentException("Insufficient space in buffer.", nameof(buffer));
      }

      buffer[0] = new Instruction(Opcode.Class, parameter: (ushort) _child.CompiledLength);
      return _child.Compile(buffer.Slice(1));
    }
  }
}
