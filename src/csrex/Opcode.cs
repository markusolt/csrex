using System;
using CsRex;

namespace CsRex {
  internal enum Opcode : byte {
    Character,
    Range,
    Class,
    Branch,
    Branchback,
    Jump,
    Jumpback,
  }
}
