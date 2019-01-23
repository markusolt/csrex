using System;
using System.Collections.Generic;
using System.Text;
using CsRex;
using CsRex.Parsing.Nodes;
using CsRex.Parsing;

namespace CsRex.Parsing {
  internal abstract class Node {
    protected int _compiledLength;
    protected int _minLength;

    internal int CompiledLength {
      get {
        return _compiledLength;
      }
    }

    internal int MinLength {
      get {
        return _minLength;
      }
    }

    internal (Instruction[] program, char[] words) Compile () {
      Instruction[] program;
      StringBuilder wordBuffer;
      char[] words;

      program = new Instruction[CompiledLength];
      wordBuffer = new StringBuilder();

      Compile(program.AsSpan(), wordBuffer);
      words = new char[wordBuffer.Length];
      wordBuffer.CopyTo(0, words, 0, wordBuffer.Length);

      return (program, words);
    }

    internal Span<Instruction> Compile (Span<Instruction> buffer, StringBuilder words) {
      if (buffer.Length < CompiledLength) {
        throw new ArgumentException("Insufficient space in buffer.", nameof(buffer));
      }

      CompileNode(buffer.Slice(0, CompiledLength), words);
      return buffer.Slice(CompiledLength);
    }

    internal abstract void CompileNode (Span<Instruction> buffer, StringBuilder words);

    internal static Node Concatenate (List<Node> children) {
      StringBuilder buffer;

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

      buffer = new StringBuilder();
      for (int i = 0; i < children.Count; i++) {
        if (children[i] is Character) {
          buffer.Append((children[i] as Character)!.Value);
          children.RemoveAt(i);
          i--;
          continue;
        }

        if (children[i] is Word) {
          buffer.Append((children[i] as Word)!.Value);
          children.RemoveAt(i);
          i--;
          continue;
        }

        if (buffer.Length > 0) {
          children.Insert(i, Word(buffer.ToString()));
          buffer.Clear();
          i++;
          continue;
        }
      }
      if (buffer.Length > 0) {
        children.Add(Word(buffer.ToString()));
      }

      if (children.Count == 0) {
        return new Nop();
      }

      if (children.Count == 1) {
        return children[0];
      }

      return new Concatenate(children.ToArray());
    }

    internal static Node CharacterClass (List<Node> children) {
      if (children == null) {
        throw new ArgumentNullException("Child list may not be null.", nameof(children));
      }

      for (int i = 0; i < children.Count; i++) {
        if (children[i] is CharacterClass) {
          children.InsertRange(i + 1, ((CharacterClass) children[i]).Children);
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

      return new CharacterClass(children.ToArray());
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

      if (c1 == c2) {
        return new Character(c1);
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

    internal static Node Word (string word) {
      if (word.Length == 0) {
        return new Nop();
      }

      if (word.Length == 1) {
        return Character(word[0]);
      }

      return new Word(word);
    }

    internal static Node Optional (Node child, bool greedy) {
      if (child is Nop) {
        return child;
      }

      if (child is Optional) {
        if (((Optional) child).Greedy) {
          return child;
        }
        return new Optional(((Optional) child).Child, greedy);
      }

      return new Optional(child, greedy);
    }

    internal static Node Repeat (Node child, bool greedy) {
      if (child is Nop) {
        return child;
      }

      if (child is Repeat) {
        if (((Repeat) child).Greedy) {
          return child;
        }
        return new Repeat(((Repeat) child).Child, greedy);
      }

      if (child is Optional) {
        return Optional(Repeat(((Optional) child).Child, greedy), ((Optional) child).Greedy);
      }

      return new Repeat(child, greedy);
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
