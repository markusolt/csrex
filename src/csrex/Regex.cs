using System;
using CsRex.Parsing;
using CsRex;

namespace CsRex {
  public class Regex {
    private Instruction[] _program;
    private ThreadManager _threads;
    private int _minLength;

    public Regex (string pattern) {
      Node tree;

      tree = RegexParser.Parse(pattern);

      _program = tree.Compile();
      _threads = new ThreadManager(_program.Length + 1); // include space for implied trailing success
      _minLength = tree.MinLength;
    }

    internal void Dump () {
      Instruction instr;

      Console.Write("--------------------------------\nCompiled Regex:\n\n");
      for (int i = 0; i < _program.Length; i++) {
        instr = _program[i];
        Console.Write(" {0,3}: ", i);

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

      while (_threads.Count > 0) {
        tp++;

        _threads.Swap();
        while (_threads.TryPull(out ip)) {
          if (ip >= _program.Length) {
            match = new Match(true, offset, tp - offset);

            goto kill_thread;
          }
          instr = _program[ip];

          switch (instr.Op) {
            case Opcode.Character: {
              if (tp >= line.Length || line[tp] != (char) instr.Parameter) {
                goto kill_thread;
              }

              goto next_thread;
            }
            case Opcode.Range: {
              if (tp >= line.Length || line[tp] < (char) instr.Parameter || line[tp] > (char) (instr.Parameter + instr.Length)) {
                goto kill_thread;
              }

              goto next_thread;
            }
            case Opcode.Class: {
              int skip;
              char c;

              if (tp >= line.Length) {
                goto kill_thread;
              }
              skip = ip + instr.Parameter + 1;
              c = line[tp];

              for (ip = ip + 1; ip < skip; ip++) {
                instr = _program[ip];

                switch (instr.Op) {
                  case Opcode.Character: {
                    if (c == (char) instr.Parameter) {
                      ip = skip - 1;
                      goto next_thread;
                    }
                    break;
                  }
                  case Opcode.Range: {
                    if ((char) instr.Parameter <= c && c <= (char) (instr.Parameter + instr.Length)) {
                      ip = skip - 1;
                      goto next_thread;
                    }
                    break;
                  }
                  default: {
                    throw new Exception("Unkown instruction.");
                  }
                }
              }

              goto kill_thread;
            }
            case Opcode.Branch: {
              _threads.Push(ip + instr.Parameter + 1);

              break;
            }
            case Opcode.Branchback: {
              _threads.Push(ip - instr.Parameter);

              break;
            }
            case Opcode.Jump: {
              ip = ip  + instr.Parameter;

              break;
            }
            case Opcode.Jumpback: {
              ip = ip - instr.Parameter - 1;

              break;
            }
            default: {
              throw new Exception("Unkown instruction.");
            }
          }

          _threads.Push(ip + 1);
          continue;

          next_thread:
          _threads.Continue(ip + 1);
          continue;

          kill_thread:
          continue;
        }
      }

      return match.Success;
    }
  }
}
