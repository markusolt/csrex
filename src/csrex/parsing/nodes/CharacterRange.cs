using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class CharacterRange : Node {
    private char _character;
    private byte _range;

    internal CharacterRange (char character, byte range) {
      _character = character;
      _range = range;
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

      buffer[0] = new Instruction(Regex.instr_range, parameter: (ushort) _character, length: _range);
      return buffer.Slice(1);
    }
  }
}
