using System;
using System.Collections.Generic;
using CsRex;
using CsRex.Parsing.Nodes;
using CsRex.Parsing;

namespace CsRex.Parsing {
  internal abstract class Node {
    protected int _compiledLength;

    internal int CompiledLength {
      get {
        return _compiledLength;
      }
    }

    internal Instruction[] Compile () {
      Instruction[] bytecode;

      bytecode = new Instruction[CompiledLength];
      Compile(bytecode);
      return bytecode;
    }

    internal Span<Instruction> Compile (Span<Instruction> buffer) {
      if (buffer.Length < CompiledLength) {
        throw new ArgumentException("Insufficient space in buffer.", nameof(buffer));
      }

      CompileNode(buffer.Slice(0, CompiledLength));
      return buffer.Slice(CompiledLength);
    }

    internal abstract void CompileNode (Span<Instruction> buffer);

    internal static Node Concatenate (List<Node> children) {
      if (children == null) {
        throw new ArgumentNullException("Child list may not be null.", nameof(children));
      }

      for (int i = 0; i < children.Count; i++) {
        if (children[i] is Concatenate) {
          children.InsertRange(i + 1, ((Concatenate) children[i]).Children);
          children.RemoveAt(i);
          i--;
          continue;
        }

        if (children[i] is Nop) {
          children.RemoveAt(i);
          i--;
          continue;
        }
      }

      if (children.Count == 0) {
        return new Nop();
      }

      if (children.Count == 1) {
        return children[0];
      }

      return new Concatenate(children.ToArray());
    }

    internal static Node CharacterClass (Node child) {
      if (child is Concatenate) {
        return new CharacterClass(child);
      }

      return child;
    }

    internal static Node Character (char character) {
      return new Character(character);
    }

    internal static Node CharacterRange (char c1, char c2) {
      Node[] ranges;

      if (c2 < c1) {
        char tmp;

        tmp = c2;
        c2 = c1;
        c1 = tmp;
      }

      if (c2 - c1 < 256) {
        Console.WriteLine(" (range): {0}-{1}", (int) c1, (int) c2);
        return new CharacterRange(c1, (byte) (c2 - c1));
      }

      ranges = new Node[(c2 - c1) / 256];
      for (int i = 0; i < ranges.Length - 1; i++) {
        Console.WriteLine(" (range): {0}-{1}", (int) c1, (int) (c1 + 255));
        ranges[i] = new CharacterRange(c1, (byte) 255);
        c1 += (char) 256;
      }
      Console.WriteLine(" (range): {0}-{1}", (int) c1, (int) c2);
      ranges[ranges.Length - 1] = new CharacterRange(c1, (byte) (c2 - c1));
      return new Concatenate(ranges);
    }

    internal static Node Optional (Node child) {
      if (child is Nop) {
        return child;
      }

      if (child is Optional) {
        return child;
      }

      return new Optional(child);
    }

    internal static Node Repeat (Node child) {
      if (child is Nop) {
        return child;
      }

      if (child is Repeat) {
        return child;
      }

      if (child is Optional) {
        return Optional(Repeat(((Optional) child).Child));
      }

      return new Repeat(child);
    }

    internal static Node Alternate (Node child, Node alternative) {
      if (child is Nop) {
        return alternative;
      }
      if (alternative is Nop) {
        return child;
      }

      return new Alternate(child, alternative);
    }
  }
}
