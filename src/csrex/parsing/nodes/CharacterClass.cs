using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class CharacterClass : Node {
    private Node[] _children;

    internal CharacterClass (Node[] children) {
      if (children == null) {
        throw new ArgumentNullException("Child array may not be null.", nameof(children));
      }

      _children = children;
      _compiledLength = 1;
      for (int i = 0; i < _children.Length; i++) {
        _compiledLength += _children[i].CompiledLength;
      }
      _minLength = 1;
    }

    internal Node[] Children {
      get {
        return _children;
      }
    }

    internal override void CompileNode (Span<Instruction> buffer) {
      buffer[0] = new Instruction(Opcode.Class, parameter: (ushort) (_compiledLength - 1));
      buffer = buffer.Slice(1);
      for (int i = 0; i < _children.Length; i++) {
        buffer = _children[i].Compile(buffer);
      }
    }
  }
}
