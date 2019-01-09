using System;
using System.Collections.Generic;
using CsRex.Parsing;

namespace CsRex.Parsing {
  internal abstract class Node {
    internal abstract ushort[] ToBytecode (int leftpadding = 0, int rightpadding = 0);

    internal static Node NewCharacterNode (char c) {
      return new CharacterNode(c);
    }

    internal static Node NewClassNode (Node content) {
      if (content == null) {
        throw new ArgumentNullException(nameof(content));
      }

      if (content is FailNode) {
        return content;
      }
      if (content is CharacterNode) {
        return content;
      }
      if (content is RangeNode) {
        return content;
      }
      if (content is GroupNode) {
        return new ClassNode(content);
      }

      throw new ArgumentException(nameof(content));
    }

    internal static Node NewRangeNode (char c1, char c2) {
      if (c1 > c2) {
        char tmp = c1;
        c1 = c2;
        c2 = tmp;
      }
      if (c1 == c2) {
        return new CharacterNode(c1);
      }
      return new RangeNode(c1, c2);
    }

    internal static Node NewFailNode () {
      return new FailNode();
    }

    internal static Node NewGroupNode (List<Node> content) {
      if (content == null) {
        throw new ArgumentNullException();
      }

      for (int i = 0; i < content.Count; i++) {
        if (content[i] is GroupNode) {
          content.InsertRange(i + 1, (content[i] as GroupNode)!.Content);
          content.RemoveAt(i);
          i--;
          continue;
        }
        if (content[i] is FailNode) {
          return content[i];
        }
      }

      if (content.Count == 1) {
        return content[0];
      }

      if (content.Count > 1) {
        return new GroupNode(content.ToArray());
      }

      throw new ArgumentException(nameof(content));
    }

    internal static Node NewOrNode (Node n1, Node n2) {
      if (n1 == null) {
        throw new ArgumentNullException(nameof(n1));
      }
      if (n2 == null) {
        throw new ArgumentNullException(nameof(n2));
      }

      if (n1 is FailNode && !(n2 is FailNode)) {
        return n2;
      }
      if (n2 is FailNode && !(n1 is FailNode)) {
        return n1;
      }

      return new OrNode(n1, n2);
    }

    internal static Node NewOptionalNode (Node n, bool greedy = true) {
      if (n == null) {
        throw new ArgumentNullException(nameof(n));
      }

      if (n is OptionalNode) {
        return new OptionalNode(((OptionalNode) n).Content, ((OptionalNode) n).IsGreedy || greedy);
      }

      return new OptionalNode(n, greedy);
    }

    internal static Node NewRepeatNode (Node n, bool greedy = true) {
      if (n == null) {
        throw new ArgumentNullException(nameof(n));
      }

      if (n is OptionalNode) {
        return new OptionalNode(new RepeatNode(((OptionalNode) n).Content, greedy), greedy);
      }
      if (n is RepeatNode) {
        return new RepeatNode(((RepeatNode) n).Content, ((RepeatNode) n).IsGreedy || greedy);
      }

      return new RepeatNode(n, greedy);
    }
  }

  internal class CharacterNode : Node {
    private char _c;

    internal CharacterNode (char c) {
      _c = c;
    }

    internal override ushort[] ToBytecode (int leftpadding, int rightpadding) {
      ushort[] bytecode;

      bytecode = new ushort[leftpadding + rightpadding + 2];
      bytecode[leftpadding] = Regex.instr_char;
      bytecode[leftpadding + 1] = _c;
      return bytecode;
    }
  }

  internal class ClassNode : Node {
    private Node _content;

    internal ClassNode (Node content) {
      if (content == null) {
        throw new ArgumentNullException(nameof(content));
      }

      _content = content;
    }

    internal override ushort[] ToBytecode (int leftpadding, int rightpadding) {
      ushort[] bytecode;

      bytecode = _content.ToBytecode(leftpadding + 2, rightpadding);
      bytecode[leftpadding] = Regex.instr_class;
      bytecode[leftpadding + 1] = (ushort) (bytecode.Length - leftpadding - rightpadding - 2);
      return bytecode;
    }
  }

  internal class RangeNode : Node {
    private char _c1;
    private char _c2;

    internal RangeNode (char c1, char c2) {
      _c1 = c1;
      _c2 = c2;
    }

    internal override ushort[] ToBytecode (int leftpadding, int rightpadding) {
      ushort[] bytecode;

      bytecode = new ushort[leftpadding + rightpadding + 3];
      bytecode[leftpadding] = Regex.instr_range;
      bytecode[leftpadding + 1] = _c1;
      bytecode[leftpadding + 2] = _c2;
      return bytecode;
    }
  }

  internal class FailNode : Node {
    internal override ushort[] ToBytecode (int leftpadding, int rightpadding) {
      ushort[] bytecode;

      bytecode = new ushort[leftpadding + rightpadding + 1];
      bytecode[leftpadding] = Regex.instr_fail;
      return bytecode;
    }
  }

  internal class GroupNode : Node {
    private Node[] _content;

    internal GroupNode (Node[] content) {
      if (content == null) {
        throw new ArgumentNullException(nameof(content));
      }

      _content = content;
    }

    internal Node[] Content {
      get {
        return _content;
      }
    }

    internal override ushort[] ToBytecode (int leftpadding, int rightpadding) {
      ushort[] bytecode;
      ushort[] add;

      bytecode = new ushort[leftpadding + rightpadding];
      for (int i = 0; i < _content.Length; i++) {
        add = _content[i].ToBytecode();
        Array.Resize(ref bytecode, bytecode.Length + add.Length);
        Array.Copy(add, 0, bytecode, bytecode.Length - add.Length - rightpadding, add.Length);
      }
      return bytecode;
    }
  }

  internal class OrNode : Node {
    private Node _n1;
    private Node _n2;

    internal OrNode (Node n1, Node n2) {
      if (n1 == null) {
        throw new ArgumentNullException(nameof(n1));
      }
      if (n2 == null) {
        throw new ArgumentNullException(nameof(n2));
      }

      _n1 = n1;
      _n2 = n2;
    }

    internal override ushort[] ToBytecode (int leftpadding, int rightpadding) {
      ushort[] left;
      ushort[] right;

      left = _n1.ToBytecode(leftpadding + 2, 2);
      right = _n2.ToBytecode(0, rightpadding);

      left[leftpadding] = Regex.instr_branch;
      left[leftpadding + 1] = (ushort) (left.Length - leftpadding - 4);
      left[left.Length - 2] = Regex.instr_jump;
      left[left.Length - 1] = (ushort) (right.Length - rightpadding);

      Array.Resize(ref left, left.Length + right.Length);
      Array.Copy(right, left.Length - right.Length, left, 0, right.Length);
      return left;
    }
  }

  internal class OptionalNode : Node {
    private Node _content;
    private bool _isGreedy;

    internal OptionalNode (Node content, bool isGreedy = true) {
      if (content == null) {
        throw new ArgumentNullException(nameof(content));
      }

      _content = content;
      _isGreedy = isGreedy;
    }

    internal Node Content {
      get {
        return _content;
      }
    }

    internal bool IsGreedy {
      get {
        return _isGreedy;
      }
    }

    internal override ushort[] ToBytecode (int leftpadding = 0, int rightpadding = 0) {
      ushort[] bytecode;

      bytecode = _content.ToBytecode(leftpadding + 2, rightpadding);
      bytecode[leftpadding] = Regex.instr_branch;
      bytecode[leftpadding + 1] = (ushort) (bytecode.Length - leftpadding - rightpadding - 2);
      return bytecode;
    }
  }

  internal class RepeatNode : Node {
    private Node _content;
    private bool _isGreedy;

    internal RepeatNode (Node content, bool isGreedy = true) {
      if (content == null) {
        throw new ArgumentNullException(nameof(content));
      }

      _content = content;
      _isGreedy = isGreedy;
    }

    internal Node Content {
      get {
        return _content;
      }
    }

    internal bool IsGreedy {
      get {
        return _isGreedy;
      }
    }

    internal override ushort[] ToBytecode (int leftpadding = 0, int rightpadding = 0) {
      ushort[] bytecode;

      bytecode = _content.ToBytecode(leftpadding, rightpadding + 2);
      bytecode[bytecode.Length - rightpadding - 2] = Regex.instr_branchback;
      bytecode[bytecode.Length - rightpadding - 1] = (ushort) (bytecode.Length - leftpadding - rightpadding - 2);
      return bytecode;
    }
  }
}
