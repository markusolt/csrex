using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Character : Node {
    private char _character;

    internal Character (char character) {
      _character = character;
    }

    internal override int CompiledLength {
      get {
        return 1;
      }
    }

    internal override Span<Instruction> Compile (Span<Instruction> buffer) {
      if (buffer.Length < 1) {
        throw new ArgumentException("Insufficient space in buffer.", nameof(buffer));
      }

      buffer[0] = new Instruction(Regex.instr_character, parameter: (ushort) _character);
      return buffer.Slice(1);
    }
  }
}
