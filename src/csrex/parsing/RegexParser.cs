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
            reader.Expect(')');
            list.Add(_parseModifier(n, reader));
            break;
          }
          case '[': {
            reader.Read();
            n = _parseCharacterClass(reader);
            reader.Expect(']');
            list.Add(_parseModifier(n, reader));
            break;
          }
          case '|': {
            reader.Read();
            return Node.Alternate(Node.Concatenate(list), _parseExpression(reader));
          }
          case '?':
          case '*':
          case '+': {
            throw reader.Unexpected();
          }
          case ')': {
            return Node.Concatenate(list);
          }
          case ']': {
            throw reader.Unexpected();
          }
          case '\\': {
            reader.Read();

            switch (reader.Peek()) {
              case '(':
              case '[':
              case '|':
              case '?':
              case '*':
              case '+':
              case ')':
              case ']':
              case '\\':
              case '^':
              case '$': {
                list.Add(_parseModifier(Node.Character(reader.Read()), reader));
                break;
              }
              default: {
                throw reader.Unexpected();
              }
            }
            break;
          }
          case '^': {
            throw new NotSupportedException(); // TODO
          }
          case '$': {
            throw new NotSupportedException(); // TODO
          }
          default: {
            list.Add(_parseModifier(Node.Character(reader.Read()), reader));
            break;
          }
        }
      }

      return Node.Concatenate(list);
    }

    private static Node _parseCharacterClass (Reader reader) {
      List<Node> list;

      list = new List<Node>();

      if (reader.Peek() == '^') {
        throw new NotSupportedException(); // TODO
      }

      if (reader.Peek() == '-') {
        list.Add(Node.Character(reader.Read()));
      }

      while (!reader.EndOfFile) {
        switch (reader.Peek()) {
          case '(':
          case '[':
          case '|':
          case '?':
          case '*':
          case '+':
          case ')': {
            throw reader.Unexpected();
          }
          case ']': {
            return Node.CharacterClass(list);
          }
          case '\\': {
            reader.Read();

            switch (reader.Peek()) {
              case '(':
              case '[':
              case '|':
              case '?':
              case '*':
              case '+':
              case ')':
              case ']':
              case '\\':
              case '^':
              case '$': {
                list.Add(Node.Character(reader.Read()));
                break;
              }
              default: {
                throw reader.Unexpected();
              }
            }
            break;
          }
          case '^':
          case '$': {
            throw reader.Unexpected();
          }
          case '-': {
            if (reader.Peek(offset: 1) != ']') {
              throw reader.Unexpected();
            }
            list.Add(Node.Character(reader.Read()));
            break;
          }
          default: {
            char c;

            c = reader.Read();
            if (reader.Peek() == '-' && reader.Peek(offset: 1) != ']') {
              reader.Read();
              list.Add(Node.CharacterRange(c, reader.Read()));
              break;
            }

            list.Add(Node.Character(c));
            break;
          }
        }
      }

      return Node.CharacterClass(list);
    }

    private static Node _parseModifier (Node n, Reader reader) {
      if (reader.EndOfFile) {
        return n;
      }

      switch (reader.Peek()) {
        case '?': {
          reader.Read();
          if (reader.Peek() == '?') {
            reader.Read();
            return Node.Optional(n, greedy: false);
          }
          return Node.Optional(n, greedy: true);
        }
        case '*': {
          reader.Read();
          if (reader.Peek() == '?') {
            reader.Read();
            return Node.Optional(Node.Repeat(n, greedy: false), greedy: false);
          }
          return Node.Optional(Node.Repeat(n, greedy: true), greedy: true);
        }
        case '+': {
          reader.Read();
          if (reader.Peek() == '?') {
            reader.Read();
            return Node.Repeat(n, greedy: false);
          }
          return Node.Repeat(n, greedy: true);
        }
        default: {
          return n;
        }
      }
    }
  }
}
