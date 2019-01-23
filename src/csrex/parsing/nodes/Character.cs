using System;
using System.Text;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Character : Node {
    private char _character;

    internal Character (char character) {
      _character = character;
      _compiledLength = 1;
      _minLength = 1;
    }

    internal char Value {
      get {
        return _character;
      }
    }

    internal override void CompileNode (Span<Instruction> buffer, StringBuilder words) {
      buffer[0] = new Instruction(Opcode.Character, parameter: (ushort) _character);
    }
  }
}
