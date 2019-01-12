using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Concatenate : Node {
    private Node[] _children;

    internal Concatenate (Node[] children) {
      if (children == null) {
        throw new ArgumentNullException("Child array may not be null.", nameof(children));
      }

      _children = children;
      _compiledLength = 0;
      for (int i = 0; i < _children.Length; i++) {
        _compiledLength += _children[i].CompiledLength;
      }
    }

    internal Node[] Children {
      get {
        return _children;
      }
    }

    internal override void CompileNode (Span<Instruction> buffer) {
      for (int i = 0; i < _children.Length; i++) {
        buffer = _children[i].Compile(buffer);
      }
    }
  }
}
