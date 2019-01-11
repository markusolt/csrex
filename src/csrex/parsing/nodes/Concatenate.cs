using System;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Concatenate : Node {
    private Node[] _children;
    private int _length;

    internal Concatenate (Node[] children) {
      if (children == null) {
        throw new ArgumentNullException("Child array may not be null.", nameof(children));
      }

      _children = children;
      _length = 0;
      for (int i = 0; i < _children.Length; i++) {
        _length += _children[i].CompiledLength;
      }
    }

    internal Node[] Children {
      get {
        return _children;
      }
    }

    internal override int CompiledLength {
      get {
        return _length;
      }
    }

    internal override Span<Instruction> Compile (Span<Instruction> buffer) {
      if (buffer.Length < CompiledLength) {
        throw new ArgumentException("Insufficient space in buffer.", nameof(buffer));
      }

      for (int i = 0; i < _children.Length; i++) {
        buffer = _children[i].Compile(buffer);
      }
      return buffer;
    }
  }
}
