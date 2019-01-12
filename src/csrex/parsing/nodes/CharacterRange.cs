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
      _compiledLength = 1;
    }

    internal override void CompileNode (Span<Instruction> buffer) {
      buffer[0] = new Instruction(Opcode.Range, parameter: (ushort) _character, length: _range);
    }
  }
}
