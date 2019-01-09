using System;
using CsRex;

namespace CsRex {
  public class Regex {
    private ushort[] _program;
    private ThreadManager _threads;

    internal const ushort instr_success = 0;
    internal const ushort instr_fail = 1;
    internal const ushort instr_branch = 2;
    internal const ushort instr_branchback = 3;
    internal const ushort instr_jump = 4;
    internal const ushort instr_jumpback = 5;
    internal const ushort instr_char = 6;
    internal const ushort instr_class = 7;
    internal const ushort instr_range = 8;

    public Regex (string pattern) {
      _program = Parsing.RegexParser.Parse(pattern);
      _threads = new ThreadManager(_program.Length);
    }

    internal void Dump () {
      Console.Write("--------------------------------\nCompiled Regex:\n\n");
      for (int i = 0; i < _program.Length; i++) {
        Console.Write(" {0,3}: ", i);
        switch (_program[i]) {
          case instr_success: {
            Console.Write("success");
            break;
          }
          case instr_fail: {
            Console.Write("fail");
            break;
          }
          case instr_branch: {
            Console.Write("branch :{0}", i + 2 + _program[++i]);
            break;
          }
          case instr_branchback: {
            Console.Write("branch :{0}", i - _program[++i]);
            break;
          }
          case instr_jump: {
            Console.Write("jump :{0}", i + 2 + _program[++i]);
            break;
          }
          case instr_jumpback: {
            Console.Write("jump :{0}", i - _program[++i]);
            break;
          }
          case instr_char: {
            Console.Write("char '{0}'", (char) _program[++i]);
            break;
          }
          case instr_class: {
            Console.Write("class :{0}", i + 2 + _program[++i]);
            break;
          }
          case instr_range: {
            Console.Write("range '{0}'-'{1}'", (char) _program[++i], (char) _program[++i]);
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

      if (offset < 0 || offset > line.Length) {
        throw new ArgumentException(nameof(offset));
      }

      tp = offset - 1;
      _threads.Clear();
      _threads.Push(0);
      match = new Match(false);

      while (_threads.Count > 0) {
        tp++;

        _threads.Reset();
        while (_threads.TryPull(out ip)) {
          switch (_program[ip]) {
            case instr_success: {
              match = new Match(true, offset, tp - offset);

              goto kill_thread;
            }
            case instr_fail: {
              goto kill_thread;
            }
            case instr_branch: {
              _threads.Push(ip + 2 + _program[ip + 1]);
              ip += 2;

              break;
            }
            case instr_branchback: {
              _threads.Push(ip - _program[ip + 1]);
              ip += 2;

              break;
            }
            case instr_jump: {
              ip += 2 + _program[ip + 1];

              break;
            }
            case instr_jumpback: {
              ip -= _program[ip + 1];

              break;
            }
            case instr_char: {
              if (tp >= line.Length || line[tp] != (char) _program[ip + 1]) {
                goto kill_thread;
              }
              ip += 2;

              goto next_thread;
            }
            case instr_class: {
              int skip;
              char c;

              if (tp >= line.Length) {
                goto kill_thread;
              }
              skip = ip + 2 + _program[ip + 1];
              c = line[tp];

              ip += 2;
              while (ip < skip) {
                switch (_program[ip]) {
                  case instr_char: {
                    if (c == (char) _program[ip + 1]) {
                      ip = skip;
                      goto next_thread;
                    }
                    ip += 2;
                    break;
                  }
                  case instr_range: {
                    if (_program[ip + 1] <= c && c <= _program[ip + 2]) {
                      ip = skip;
                      goto next_thread;
                    }
                    ip += 3;
                    break;
                  }
                  default: {
                    throw new Exception("Unkown instruction.");
                  }
                }
              }

              goto kill_thread;
            }
            default: {
              throw new Exception("Unkown instruction.");
            }
          }

          _threads.Push(ip);
          continue;

          next_thread:
          _threads.PushBack(ip);
          continue;

          kill_thread:
          continue;
        }
      }

      return match.Success;
    }
  }
}
