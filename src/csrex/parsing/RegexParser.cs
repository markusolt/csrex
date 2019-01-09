using System;
using System.Collections.Generic;
using CsRex.Parsing;

namespace CsRex.Parsing {
  internal static class RegexParser {
    internal static ushort[] Parse (string pattern) {
      Reader reader;
      Node tree;
      ushort[] bytecode;

      reader = new Reader(pattern);
      tree = _parseExpression(reader);

      if (!reader.EndOfFile) {
        throw new ParsingException("Unexpected \")\" in pattern.");
      }

      bytecode = tree.ToBytecode(0, 1);
      bytecode[bytecode.Length - 1] = Regex.instr_success;
      return bytecode;
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
            return Node.NewGroupNode(list);
          }
          case '[': {
            reader.Read();
            n = new ClassNode(_parseClass(reader));
            if (reader.Read() != ']') {
              throw new ParsingException("Missing \"]\" in pattern.");
            }
            list.Add(_parseModifier(n, reader));
            break;
          }
          case '|': {
            reader.Read();
            return Node.NewOrNode(Node.NewGroupNode(list), _parseExpression(reader));
          }
          case '\\': {
            reader.Read();
            if (reader.EndOfFile) {
              throw new ParsingException("Unexpected end of pattern.");
            }
            switch (reader.Peek()) {
              default: {
                list.Add(_parseModifier(Node.NewCharacterNode(reader.Read()), reader));
                break;
              }
            }
            break;
          }
          default: {
            list.Add(_parseModifier(Node.NewCharacterNode(reader.Read()), reader));
            break;
          }
        }
      }

      return Node.NewGroupNode(list);
    }

    private static Node _parseClass (Reader reader) {
      List<Node> list;

      list = new List<Node>();
      if (reader.Peek() == '-') {
        reader.Read();
        list.Add(Node.NewCharacterNode('-'));
      }
      while (!reader.EndOfFile) {
        switch (reader.Peek()) {
          case ']': {
            return Node.NewGroupNode(list);
          }
          case '\\': {
            reader.Read();
            if (reader.EndOfFile) {
              throw new ParsingException("Unexpected end of pattern.");
            }
            switch (reader.Peek()) {
              default: {
                list.Add(Node.NewCharacterNode(reader.Read()));
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
                list.Add(Node.NewCharacterNode(a));
                list.Add(Node.NewCharacterNode('-'));
                break;
              }
              if (reader.EndOfFile) {
                throw new ParsingException("Unexpected end of pattern.");
              }
              list.Add(Node.NewRangeNode(a, reader.Read()));
              break;
            }
            list.Add(Node.NewCharacterNode(a));
            break;
          }
        }
      }

      return Node.NewGroupNode(list);
    }

    private static Node _parseModifier (Node tree, Reader reader) {
      if (reader.EndOfFile) {
        return tree;
      }

      switch (reader.Peek()) {
        case '?': {
          reader.Read();
          if (reader.Peek() == '?') {
            reader.Read();
            return Node.NewOptionalNode(tree, false);
          }
          return Node.NewOptionalNode(tree, true);
        }
        case '*': {
          reader.Read();
          if (reader.Peek() == '?') {
            reader.Read();
            return Node.NewOptionalNode(Node.NewRepeatNode(tree, false), false);
          }
          return Node.NewOptionalNode(Node.NewRepeatNode(tree, true), true);
        }
        case '+': {
          reader.Read();
          if (reader.Peek() == '?') {
            reader.Read();
            return Node.NewRepeatNode(tree, false);
          }
          return Node.NewRepeatNode(tree, true);
        }
        default: {
          return tree;
        }
      }
    }
  }
}
