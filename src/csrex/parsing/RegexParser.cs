using System;
using System.Collections.Generic;
using CsRex.Parsing;

namespace CsRex.Parsing {
  internal static class RegexParser {
    internal static Node Parse (string pattern) {
      Reader reader;
      Node tree;

      reader = new Reader(pattern);
      tree = _parseExpression(reader);

      if (!reader.EndOfFile) {
        throw new ParsingException("Unexpected \")\" in pattern.");
      }

      return tree;
    }

    private static Node _parseExpression (Reader reader) {
      List<Node> list;
      Node n;

      list = new List<Node>();
      while (!reader.EndOfFile) {
        switch (reader.Peek()) {
          case '(': {
            reader.Read();
            n = _parseExpression(reader);
            if (reader.Read() != ')') {
              throw new ParsingException("Missing \")\" in pattern.");
            }
            list.Add(_parseModifier(n, reader));
            break;
          }
          case ')': {
            return Node.Concatenate(list);
          }
          case '[': {
            reader.Read();
            n = Node.CharacterClass(_parseClass(reader));
            if (reader.Read() != ']') {
              throw new ParsingException("Missing \"]\" in pattern.");
            }
            list.Add(_parseModifier(n, reader));
            break;
          }
          case '|': {
            reader.Read();
            return Node.Alternate(Node.Concatenate(list), _parseExpression(reader));
          }
          case '\\': {
            reader.Read();
            if (reader.EndOfFile) {
              throw new ParsingException("Unexpected end of pattern.");
            }
            switch (reader.Peek()) {
              default: {
                list.Add(_parseModifier(Node.Character(reader.Read()), reader));
                break;
              }
            }
            break;
          }
          default: {
            list.Add(_parseModifier(Node.Character(reader.Read()), reader));
            break;
          }
        }
      }

      return Node.Concatenate(list);
    }

    private static Node _parseClass (Reader reader) {
      List<Node> list;

      list = new List<Node>();
      if (reader.Peek() == '-') {
        reader.Read();
        list.Add(Node.Character('-'));
      }
      while (!reader.EndOfFile) {
        switch (reader.Peek()) {
          case ']': {
            return Node.Concatenate(list);
          }
          case '\\': {
            reader.Read();
            if (reader.EndOfFile) {
              throw new ParsingException("Unexpected end of pattern.");
            }
            switch (reader.Peek()) {
              default: {
                list.Add(Node.Character(reader.Read()));
                break;
              }
            }
            break;
          }
          default: {
            char a = reader.Read();
            if (reader.Peek() == '-') {
              reader.Read();
              if (reader.Peek() == ']') {
                list.Add(Node.Character(a));
                list.Add(Node.Character('-'));
                break;
              }
              if (reader.EndOfFile) {
                throw new ParsingException("Unexpected end of pattern.");
              }
              list.Add(Node.CharacterRange(a, reader.Read()));
              break;
            }
            list.Add(Node.Character(a));
            break;
          }
        }
      }

      return Node.Concatenate(list);
    }

    private static Node _parseModifier (Node tree, Reader reader) {
      if (reader.EndOfFile) {
        return tree;
      }

      switch (reader.Peek()) {
        case '?': {
          reader.Read();
          return Node.Optional(tree);
        }
        case '*': {
          reader.Read();
          return Node.Optional(Node.Repeat(tree));
        }
        case '+': {
          reader.Read();
          return Node.Repeat(tree);
        }
        default: {
          return tree;
        }
      }
    }
  }
}
