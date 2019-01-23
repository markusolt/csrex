using System;
using CsRex.Parsing;
using CsRex;

namespace CsRex {
  public class Regex {
    private Instruction[] _program;
    private char[] _words;
    private ThreadManager _threads;
    private int _minLength;

    public Regex (string pattern) {
      Node tree;

      tree = RegexParser.Parse(pattern);

      (_program, _words) = tree.Compile();
      _threads = new ThreadManager(_program.Length + 1); // include space for implied trailing success
      _minLength = tree.MinLength;
    }

    internal void Dump () {
      Instruction instr;

      Console.Write("--------------------------------\nCompiled Regex:\n");
      Console.Write(" length: {0}\n\n", _program.Length);
      for (int i = 0; i < _program.Length; i++) {
        instr = _program[i];
        Console.Write("  {0,2}: ", i);

        switch (instr.Op) {

          case Opcode.Character: {
            Console.Write("character {0}", (char) instr.Parameter);
            break;
          }
          case Opcode.Range: {
            Console.Write("range {0}-{1}", (char) instr.Parameter, (char) (instr.Parameter + instr.Length));
            break;
          }
          case Opcode.Class: {
            Console.Write("class {0}:", i + instr.Parameter + 1);
            break;
          }
          case Opcode.Word: {
            Console.Write("word \"{0}\"", _words.AsSpan().Slice(instr.Parameter, instr.Length).ToString());
            break;
          }
          case Opcode.BranchFast: {
            Console.Write("branchfast {0}:", i + instr.Parameter + 1);
            break;
          }
          case Opcode.BranchbackFast: {
            Console.Write("branchfast {0}:", i - instr.Parameter);
            break;
          }
          case Opcode.Branch: {
            Console.Write("branch {0}:", i + instr.Parameter + 1);
            break;
          }
          case Opcode.Branchback: {
            Console.Write("branch {0}:", i - instr.Parameter);
            break;
          }
          case Opcode.Jump: {
            Console.Write("jump {0}:", i + instr.Parameter + 1);
            break;
          }
          case Opcode.Jumpback: {
            Console.Write("jump {0}:", i - instr.Parameter);
            break;
          }
        }
        Console.Write("\n");
      }
      Console.Write("--------------------------------\n\n");
    }

    public bool Match (ReadOnlySpan<char> line, out Match match, int offset = 0) {
      int tp;
      int ip;
      int sleep;
      Instruction instr;

      if (offset < 0 || offset > line.Length) {
        throw new ArgumentException(nameof(offset));
      }

      tp = offset - 1;
      _threads.Clear();
      _threads.Push(0);
      match = new Match(false);

      if (tp + 1 > line.Length - _minLength) {
        return false;
      }

      while (_threads.Swap(out sleep)) {
        tp += sleep + 1;
        while (_threads.TryPull(out ip)) {
          if (ip >= _program.Length) {
            match = new Match(true, offset, tp - offset);

            break;
          }
          instr = _program[ip];

          switch (instr.Op) {
            case Opcode.Character: {
              if (tp >= line.Length || line[tp] != (char) instr.Parameter) {
                continue;
              }

              _threads.Push(ip + 1);
              continue;
            }
            case Opcode.Range: {
              if (tp >= line.Length || line[tp] < (char) instr.Parameter || line[tp] > (char) (instr.Parameter + instr.Length)) {
                continue;
              }

              _threads.Push(ip + 1);
              continue;
            }
            case Opcode.Class: {
              int skip;
              char c;

              if (tp >= line.Length) {
                continue;
              }
              skip = ip + instr.Parameter + 1;
              c = line[tp];

              for (ip = ip + 1; ip < skip; ip++) {
                instr = _program[ip];

                switch (instr.Op) {
                  case Opcode.Character: {
                    if (c == (char) instr.Parameter) {
                      _threads.Push(skip);
                      goto end_of_class;
                    }
                    break;
                  }
                  case Opcode.Range: {
                    if ((char) instr.Parameter <= c && c <= (char) (instr.Parameter + instr.Length)) {
                      _threads.Push(skip);
                      goto end_of_class;
                    }
                    break;
                  }
                  default: {
                    throw new Exception("Unkown instruction.");
                  }
                }
              }

              end_of_class:
              continue;
            }
            case Opcode.Word: {
              ReadOnlySpan<char> word;

              if (instr.Length == 0) {
                _threads.PushFront(ip + 1);
                continue;
              }

              if (tp > line.Length - instr.Length) {
                continue;
              }

              word = _words.AsSpan().Slice(instr.Parameter, instr.Length);
              for (int i = 0; i < word.Length; i++) {
                if (line[tp + i] != word[i]) {
                  continue;
                }
              }

              _threads.Push(ip + 1, word.Length - 1);
              continue;
            }
            case Opcode.BranchFast: {
              _threads.PushFront(ip + 1);
              _threads.PushFront(ip + instr.Parameter + 1);

              continue;
            }
            case Opcode.BranchbackFast: {
              _threads.PushFront(ip + 1);
              _threads.PushFront(ip - instr.Parameter);

              continue;
            }
            case Opcode.Branch: {
              _threads.PushFront(ip + instr.Parameter + 1);
              _threads.PushFront(ip + 1);

              continue;
            }
            case Opcode.Branchback: {
              _threads.PushFront(ip - instr.Parameter);
              _threads.PushFront(ip + 1);

              continue;
            }
            case Opcode.Jump: {
              _threads.PushFront(ip + instr.Parameter + 1);

              continue;
            }
            case Opcode.Jumpback: {
              _threads.PushFront(ip - instr.Parameter);

              continue;
            }
            default: {
              throw new Exception("Unkown instruction.");
            }
          }

          throw new Exception("Instruction did not complete properly.");
        }
      }

      return match.Success;
    }
  }
}
