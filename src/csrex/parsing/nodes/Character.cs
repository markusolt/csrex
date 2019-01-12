using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Character : Node {
    private char _character;

    internal Character (char character) {
      _character = character;
      _compiledLength = 1;
    }

    internal override void CompileNode (Span<Instruction> buffer) {
      buffer[0] = new Instruction(Opcode.Character, parameter: (ushort) _character);
    }
  }
}
