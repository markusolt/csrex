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
      _compiledLength = 2 + _child.CompiledLength + _alternative.CompiledLength;
      _minLength = _child.MinLength < _alternative.MinLength ? _child.MinLength : _alternative.MinLength;
    }

    internal override void CompileNode (Span<Instruction> buffer) {
      buffer[0] = new Instruction(Opcode.Branch, parameter: (ushort) (_child.CompiledLength + 1));
      buffer = _child.Compile(buffer.Slice(1));
      buffer[0] = new Instruction(Opcode.Jump, parameter: (ushort) _alternative.CompiledLength);
      _alternative.Compile(buffer.Slice(1));
    }
  }
}
