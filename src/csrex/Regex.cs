using System;
using CsRex;

namespace CsRex {
  public class Regex {
    private Instruction[] _program;
    private ThreadManager _threads;

    internal const byte instr_character = 0;
    internal const byte instr_range = 1;
    internal const byte instr_class = 2;
    internal const byte instr_branch = 3;
    internal const byte instr_branchback = 4;
    internal const byte instr_jump = 5;
    internal const byte instr_jumpback = 6;

    public Regex (string pattern) {
      _program = Parsing.RegexParser.Parse(pattern);
      _threads = new ThreadManager(_program.Length + 1); // include space for implied trailing success
    }

    internal void Dump () {
      Instruction instr;

      Console.Write("--------------------------------\nCompiled Regex:\n\n");
      for (int i = 0; i < _program.Length; i++) {
        instr = _program[i];
        Console.Write(" {0,3}: ", i);

        switch (instr.Id) {

          case instr_character: {
            Console.Write("character {0}", (char) instr.Parameter);
            break;
          }
          case instr_range: {
            Console.Write("range {0}-{1}", (char) instr.Parameter, (char) (instr.Parameter + instr.Length));
            break;
          }
          case instr_class: {
            Console.Write("class {0}:", instr.Parameter);
            break;
          }
          case instr_branch: {
            Console.Write("branch {0}:", i + instr.Parameter + 1);
            break;
          }
          case instr_branchback: {
            Console.Write("branch {0}:", i - instr.Parameter);
            break;
          }
          case instr_jump: {
            Console.Write("jump {0}:", i + instr.Parameter + 1);
            break;
          }
          case instr_jumpback: {
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

      while (_threads.Count > 0) {
        tp++;

        _threads.Swap();
        while (_threads.TryPull(out ip)) {
          if (ip >= _program.Length) {
            match = new Match(true, offset, tp - offset);

            goto kill_thread;
          }
          instr = _program[ip];

          switch (instr.Id) {
            case instr_character: {
              if (tp >= line.Length || line[tp] != (char) instr.Parameter) {
                goto kill_thread;
              }

              goto next_thread;
            }
            case instr_range: {
              if (tp >= line.Length || line[tp] < (char) instr.Parameter || line[tp] > (char) (instr.Parameter + instr.Length)) {
                goto kill_thread;
              }

              goto next_thread;
            }
            case instr_class: {
              int skip;
              char c;

              if (tp >= line.Length) {
                goto kill_thread;
              }
              skip = ip + instr.Parameter + 1;
              c = line[tp];

              for (ip = ip + 1; ip < skip; ip++) {
                instr = _program[ip];

                switch (instr.Id) {
                  case instr_character: {
                    if (c == (char) instr.Parameter) {
                      ip = skip - 1;
                      goto next_thread;
                    }
                    break;
                  }
                  case instr_range: {
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
            case instr_branch: {
              _threads.Push(ip + instr.Parameter + 1);

              break;
            }
            case instr_branchback: {
              _threads.Push(ip - instr.Parameter);

              break;
            }
            case instr_jump: {
              ip = ip  + instr.Parameter;

              break;
            }
            case instr_jumpback: {
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
