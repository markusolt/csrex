using System;
using System.Collections.Generic;
using System.IO;
using CsRex;

namespace CsRex {
  internal static class RegexParser {
    internal static ushort[] Parse (string pattern) {
      StringReader reader;
      Node tree;
      List<ushort> bytecode;

      reader = new StringReader(pattern);
      tree = _parseExpression(reader);

      if (reader.Peek() > -1) {
        throw new ParsingException("Unexpected \")\" in pattern.");
      }

      bytecode = tree.GetBytecode();
      bytecode.Add(Regex.instr_success);
      return bytecode.ToArray();
    }

    private static Node _parseExpression (TextReader reader) {
      Node tree;

      tree = new GroupNode();
      while (reader.Peek() > -1) {
        switch ((char) reader.Peek()) {
          case '(': {
            reader.Read();
            tree.Add(_parseExpression(reader));
            if (reader.Read() != (int) ')') {
              throw new ParsingException("Missing \")\" in pattern.");
            }
            tree = _parseModifier(tree, reader);
            break;
          }
          case ')': {
            return tree;
          }
          case '|': {
            reader.Read();
            tree = new OrNode(tree, _parseExpression(reader));
            break;
          }
          case '\\': {
            reader.Read();
            switch (reader.Peek()) {
              default: {
                tree.Add(_parseModifier(new CharacterNode((char) reader.Read()), reader));
                break;
              }
            }
            break;
          }
          default: {
            tree.Add(_parseModifier(new CharacterNode((char) reader.Read()), reader));
            break;
          }
        }
      }

      return tree;
    }

    private static Node _parseModifier (Node tree, TextReader reader) {
      if (reader.Peek() == -1) {
        return tree;
      }

      switch ((char) reader.Peek()) {
        case '?': {
          reader.Read();
          return new OptionalNode(tree);
        }
        case '*': {
          reader.Read();
          return new OptionalNode(new RepeatNode(tree));
        }
        case '+': {
          reader.Read();
          return new RepeatNode(tree);
        }
        default: {
          return tree;
        }
      }
    }
  }
}
