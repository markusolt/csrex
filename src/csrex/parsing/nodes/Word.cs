using System;
using System.Text;
using CsRex;
using CsRex.Parsing;
using CsRex.Parsing.Nodes;

namespace CsRex.Parsing.Nodes {
  internal class Word : Node {
    private string _word;

    internal Word (string word) {
      if (word.Length > 255) {
        throw new ArgumentException("Length of word must be at most 255.", nameof(word));
      }

      _word = word;
      _compiledLength = 1;
      _minLength = word.Length;
    }

    internal string Value {
      get {
        return _word;
      }
    }

    internal override void CompileNode (Span<Instruction> buffer, StringBuilder words) {
      buffer[0] = new Instruction(Opcode.Word, parameter: (ushort) words.Length, length: (byte) _word.Length);
      words.Append(_word);
    }
  }
}
